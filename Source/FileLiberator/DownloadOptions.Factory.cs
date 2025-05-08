using AaxDecrypter;
using AudibleApi;
using AudibleApi.Common;
using AudibleUtilities.Widevine;
using DataLayer;
using LibationFileManager;
using NAudio.Lame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

#nullable enable
namespace FileLiberator;

public partial class DownloadOptions
{
	private const string Ec3Codec = "ec+3";
	private const string Ac4Codec = "ac-4";

	/// <summary>
	/// Initiate an audiobook download from the audible api.
	/// </summary>
	public static async Task<DownloadOptions> InitiateDownloadAsync(Api api, Configuration config, LibraryBook libraryBook)
	{
		var license = await ChooseContent(api, libraryBook, config);

		//Some audiobooks will have incorrect chapters in the metadata returned from the license request,
		//but the metadata returned by the content metadata endpoint will be correct. Call the content
		//metadata endpoint and use its chapters. Only replace the license request chapters if the total
		//lengths match (defensive against different audio formats having slightly different lengths).
		var metadata = await api.GetContentMetadataAsync(libraryBook.Book.AudibleProductId);
		if (metadata.ChapterInfo.RuntimeLengthMs == license.ContentMetadata.ChapterInfo.RuntimeLengthMs)
			license.ContentMetadata.ChapterInfo = metadata.ChapterInfo;

		var options = BuildDownloadOptions(libraryBook, config, license);

		return options;
	}

	private static async Task<ContentLicense> ChooseContent(Api api, LibraryBook libraryBook, Configuration config)
	{
		var dlQuality = config.FileDownloadQuality == Configuration.DownloadQuality.Normal ? DownloadQuality.Normal : DownloadQuality.High;

		if (!config.UseWidevine || await Cdm.GetCdmAsync() is not Cdm cdm)
			return await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId, dlQuality);

		ContentLicense? contentLic = null, fallback = null;

		try
		{
			//try to request a widevine content license using the user's spatial audio settings
			var codecChoice = config.SpatialAudioCodec switch
			{
				Configuration.SpatialCodec.EC_3 => Ec3Codec,
				Configuration.SpatialCodec.AC_4 => Ac4Codec,
				_ => throw new NotSupportedException($"Unknown value for {nameof(config.SpatialAudioCodec)}")
			};

			contentLic
				= await api.GetDownloadLicenseAsync(
					libraryBook.Book.AudibleProductId,
					dlQuality,
					ChapterTitlesType.Tree,
					DrmType.Widevine,
					config.RequestSpatial,
					codecChoice);
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to request a Widevine license.");
			//We failed to get a widevine license, so fall back to AAX(C)
			return await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId, dlQuality);
		}

		if (!contentLic.ContentMetadata.ContentReference.IsSpatial && contentLic.DrmType != DrmType.Adrm)
		{
			/*
			We got a widevine license and we have a Cdm, but we still need to decide if we WANT the file
			being delivered with widevine. This file is not "spatial", so it may be no better than the
			audio in the Adrm files. All else being equal, we prefer Adrm files because they have more
			build-in metadata and always AAC-LC, which is a codec playable by pretty much every device
			in existence.

			Unfortunately, there appears to be no way to determine which codec/quality combination we'll
			get until we make the request and see what content gets delivered. For some books,
			Widevine/High delivers 44.1 kHz / 128 kbps audio and Adrm/High delivers 22.05 kHz / 64 kbps.
			In those cases, the Widevine content size is much larger. Other books will deliver the same
			sample rate / bitrate for both Widevine and Adrm, the only difference being codec. Widevine
			is usually xHE-AAC, but is sometimes AAC-LC. Adrm is always AAC-LC.

			To decide which file we want, use this simple rule: if files are different codecs and
			Widevine is significantly larger, use Widevine. Otherwise use ADRM.
			*/

			fallback = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId, dlQuality);

			var wvCr = contentLic.ContentMetadata.ContentReference;
			var adrmCr = fallback.ContentMetadata.ContentReference;

			if (wvCr.Codec == adrmCr.Codec ||
				adrmCr.ContentSizeInBytes > wvCr.ContentSizeInBytes ||
				RelativePercentDifference(adrmCr.ContentSizeInBytes, wvCr.ContentSizeInBytes) < 0.05)
			{
				contentLic = fallback;
			}
		}		

		if (contentLic.DrmType == DrmType.Widevine)
		{
			try
			{
				using var client = new HttpClient();
				using var mpdResponse = await client.GetAsync(contentLic.LicenseResponse);
				var dash = new MpegDash(mpdResponse.Content.ReadAsStream());

				if (!dash.TryGetUri(new Uri(contentLic.LicenseResponse), out var contentUri))
					throw new InvalidDataException("Failed to get mpeg-dash content download url.");

				contentLic.ContentMetadata.ContentUrl = new() { OfflineUrl = contentUri.ToString() };

				using var session = cdm.OpenSession();
				var challenge = session.GetLicenseChallenge(dash);
				var licenseMessage = await api.WidevineDrmLicense(libraryBook.Book.AudibleProductId, challenge);
				var keys = session.ParseLicense(licenseMessage);
				contentLic.Voucher = new VoucherDtoV10()
				{
					Key = Convert.ToHexStringLower(keys[0].Kid.ToByteArray()),
					Iv = Convert.ToHexStringLower(keys[0].Key)
				};
			}
			catch
			{
				if (fallback != null)
					return fallback;

				//We won't have a fallback if the requested license is for a spatial audio file.
				//Throw so that the user is aware that spatial audio exists and that they were not able to download it.
				throw;
			}
		}
		return contentLic;
	}


	private static DownloadOptions BuildDownloadOptions(LibraryBook libraryBook, Configuration config, ContentLicense contentLic)
	{
		//If DrmType is not Adrm or Widevine, the delivered file is an unencrypted mp3.
		var outputFormat
			= contentLic.DrmType is not DrmType.Adrm and not DrmType.Widevine ||
			(config.AllowLibationFixup && config.DecryptToLossy && contentLic.ContentMetadata.ContentReference.Codec != "ac-4")
			? OutputFormat.Mp3
			: OutputFormat.M4b;

		long chapterStartMs
			= config.StripAudibleBrandAudio
			? contentLic.ContentMetadata.ChapterInfo.BrandIntroDurationMs
			: 0;

		AAXClean.FileType? inputType
			= contentLic.DrmType is DrmType.Widevine ? AAXClean.FileType.Dash
			: contentLic.DrmType is DrmType.Adrm && contentLic.Voucher?.Key.Length == 8 && contentLic.Voucher?.Iv == null ? AAXClean.FileType.Aax
			: contentLic.DrmType is DrmType.Adrm && contentLic.Voucher?.Key.Length == 32 && contentLic.Voucher?.Iv.Length == 32 ? AAXClean.FileType.Aaxc
			: null;

		var dlOptions = new DownloadOptions(config, libraryBook, contentLic.ContentMetadata.ContentUrl?.OfflineUrl)
		{
			AudibleKey = contentLic.Voucher?.Key,
			AudibleIV = contentLic.Voucher?.Iv,
			InputType = inputType,
			OutputFormat = outputFormat,
			DrmType = contentLic.DrmType,
			ContentMetadata = contentLic.ContentMetadata,
			LameConfig = outputFormat == OutputFormat.Mp3 ? GetLameOptions(config) : null,
			ChapterInfo = new AAXClean.ChapterInfo(TimeSpan.FromMilliseconds(chapterStartMs)),
			RuntimeLength = TimeSpan.FromMilliseconds(contentLic.ContentMetadata.ChapterInfo.RuntimeLengthMs),
		};

		dlOptions.LibraryBookDto.Codec = contentLic.ContentMetadata.ContentReference.Codec;
		if (TryGetAudioInfo(contentLic.ContentMetadata.ContentUrl, out int? bitrate, out int? sampleRate, out int? channels))
		{
			dlOptions.LibraryBookDto.BitRate = bitrate;
			dlOptions.LibraryBookDto.SampleRate = sampleRate;
			dlOptions.LibraryBookDto.Channels = channels;
		}

		var titleConcat = config.CombineNestedChapterTitles ? ": " : null;
		var chapters
			= flattenChapters(contentLic.ContentMetadata.ChapterInfo.Chapters, titleConcat)
			.OrderBy(c => c.StartOffsetMs)
			.ToList();

		if (config.MergeOpeningAndEndCredits)
			combineCredits(chapters);

		for (int i = 0; i < chapters.Count; i++)
		{
			var chapter = chapters[i];
			long chapLenMs = chapter.LengthMs;

			if (i == 0)
				chapLenMs -= chapterStartMs;

			if (config.StripAudibleBrandAudio && i == chapters.Count - 1)
				chapLenMs -= contentLic.ContentMetadata.ChapterInfo.BrandOutroDurationMs;

			dlOptions.ChapterInfo.AddChapter(chapter.Title, TimeSpan.FromMilliseconds(chapLenMs));
		}

		return dlOptions;
	}

	/// <summary>
	/// The most reliable way to get these audio file properties is from the filename itself.
	/// Using AAXClean to read the metadata works well for everything except AC-4 bitrate.
	/// </summary>
	private static bool TryGetAudioInfo(ContentUrl? contentUrl, out int? bitrate, out int? sampleRate, out int? channels)
	{
		bitrate = sampleRate = channels = null;

		if (contentUrl?.OfflineUrl is not string url || !Uri.TryCreate(url, default, out var uri))
			return false;

		var file = Path.GetFileName(uri.LocalPath);

		var match = AdrmAudioProperties().Match(file);
		if (match.Success)
		{
			bitrate = int.Parse(match.Groups[1].Value);
			sampleRate = int.Parse(match.Groups[2].Value);
			channels = int.Parse(match.Groups[3].Value);
			return true;
		}
		else if ((match = WidevineAudioProperties().Match(file)).Success)
		{
			bitrate = int.Parse(match.Groups[2].Value);
			sampleRate = int.Parse(match.Groups[1].Value) * 1000;
			channels = match.Groups[3].Value switch
			{
				"ec3" => 6,
				"ac4" => 3,
				_ => null
			};
			return true;
		}

		return false;
	}

	public static LameConfig GetLameOptions(Configuration config)
	{
		LameConfig lameConfig = new()
		{
			Mode = MPEGMode.Mono,
			Quality = config.LameEncoderQuality,
			OutputSampleRate = (int)config.MaxSampleRate
		};

		if (config.LameTargetBitrate)
		{
			if (config.LameConstantBitrate)
				lameConfig.BitRate = config.LameBitrate;
			else
			{
				lameConfig.ABRRateKbps = config.LameBitrate;
				lameConfig.VBR = VBRMode.ABR;
				lameConfig.WriteVBRTag = true;
			}
		}
		else
		{
			lameConfig.VBR = VBRMode.Default;
			lameConfig.VBRQuality = config.LameVBRQuality;
			lameConfig.WriteVBRTag = true;
		}
		return lameConfig;
	}

	/*

	Flatten Audible's new hierarchical chapters, combining children into parents.

	Audible may deliver chapters like this:

	00:00 - 00:10	Opening Credits
	00:10 - 00:12	Book 1
	00:12 - 00:14	|	Part 1
	00:14 - 01:40	|	|	Chapter 1
	01:40 - 03:20	|	|	Chapter 2
	03:20 - 03:22	|	Part 2
	03:22 - 05:00	|	|	Chapter 3
	05:00 - 06:40	|	|	Chapter 4
	06:40 - 06:42	Book 2
	06:42 - 06:44	|	Part 3
	06:44 - 08:20	|	|	Chapter 5
	08:20 - 10:00	|	|	Chapter 6
	10:00 - 10:02	|	Part 4
	10:02 - 11:40	|	|	Chapter 7
	11:40 - 13:20	|	|	Chapter 8
	13:20 - 13:30	End Credits

	And flattenChapters will combine them into this:

	00:00 - 00:10	Opening Credits
	00:10 - 01:40	Book 1: Part 1: Chapter 1
	01:40 - 03:20	Book 1: Part 1: Chapter 2
	03:20 - 05:00	Book 1: Part 2: Chapter 3
	05:00 - 06:40	Book 1: Part 2: Chapter 4
	06:40 - 08:20	Book 2: Part 3: Chapter 5
	08:20 - 10:00	Book 2: Part 3: Chapter 6
	10:00 - 11:40	Book 2: Part 4: Chapter 7
	11:40 - 13:20	Book 2: Part 4: Chapter 8
	13:20 - 13:40	End Credits

	However, if one of the parent chapters is longer than 10000 milliseconds, it's kept as its own
	chapter. A duration longer than a few seconds implies that the chapter contains more than just
	the narrator saying the chapter title, so it should probably be preserved as a separate chapter.
	Using the example above, if "Book 1" was 15 seconds long and "Part 3" was 20 seconds long:

	00:00 - 00:10	Opening Credits
	00:10 - 00:25	Book 1
	00:25 - 00:27	|	Part 1
	00:27 - 01:40	|	|	Chapter 1
	01:40 - 03:20	|	|	Chapter 2
	03:20 - 03:22	|	Part 2
	03:22 - 05:00	|	|	Chapter 3
	05:00 - 06:40	|	|	Chapter 4
	06:40 - 06:42	Book 2
	06:42 - 07:02	|	Part 3
	07:02 - 08:20	|	|	Chapter 5
	08:20 - 10:00	|	|	Chapter 6
	10:00 - 10:02	|	Part 4
	10:02 - 11:40	|	|	Chapter 7
	11:40 - 13:20	|	|	Chapter 8
	13:20 - 13:30	End Credits

	then flattenChapters will combine them into this:

	00:00 - 00:10	Opening Credits
	00:10 - 00:25	Book 1
	00:25 - 01:40	Book 1: Part 1: Chapter 1
	01:40 - 03:20	Book 1: Part 1: Chapter 2
	03:20 - 05:00	Book 1: Part 2: Chapter 3
	05:00 - 06:40	Book 1: Part 2: Chapter 4
	06:40 - 07:02	Book 2: Part 3
	07:02 - 08:20	Book 2: Part 3: Chapter 5
	08:20 - 10:00	Book 2: Part 3: Chapter 6
	10:00 - 11:40	Book 2: Part 4: Chapter 7
	11:40 - 13:20	Book 2: Part 4: Chapter 8
	13:20 - 13:40	End Credits

	*/

	public static List<Chapter> flattenChapters(IList<Chapter> chapters, string? titleConcat = ": ")
	{
		List<Chapter> chaps = new();

		foreach (var c in chapters)
		{
			if (c.Chapters is null)
				chaps.Add(c);
			else if (titleConcat is null)
			{
				chaps.Add(c);
				chaps.AddRange(flattenChapters(c.Chapters, titleConcat));
			}
			else
			{
				if (c.LengthMs < 10000)
				{
					c.Chapters[0].StartOffsetMs = c.StartOffsetMs;
					c.Chapters[0].StartOffsetSec = c.StartOffsetSec;
					c.Chapters[0].LengthMs += c.LengthMs;
				}
				else
					chaps.Add(c);

				var children = flattenChapters(c.Chapters, titleConcat);

				foreach (var child in children)
					child.Title = $"{c.Title}{titleConcat}{child.Title}";

				chaps.AddRange(children);
			}
		}
		return chaps;
	}

	public static void combineCredits(IList<Chapter> chapters)
	{
		if (chapters.Count > 1 && chapters[0].Title == "Opening Credits")
		{
			chapters[1].StartOffsetMs = chapters[0].StartOffsetMs;
			chapters[1].StartOffsetSec = chapters[0].StartOffsetSec;
			chapters[1].LengthMs += chapters[0].LengthMs;
			chapters.RemoveAt(0);
		}
		if (chapters.Count > 1 && chapters[^1].Title == "End Credits")
		{
			chapters[^2].LengthMs += chapters[^1].LengthMs;
			chapters.Remove(chapters[^1]);
		}
	}

	static double RelativePercentDifference(long num1, long num2)
		=> Math.Abs(num1 - num2) / (double)(num1 + num2);

	[GeneratedRegex(@".+_(\d+)_(\d+)-(\w+).mp4", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
	private static partial Regex WidevineAudioProperties();
	[GeneratedRegex(@".+_lc_(\d+)_(\d+)_(\d+).aax", RegexOptions.Singleline | RegexOptions.IgnoreCase)]
	private static partial Regex AdrmAudioProperties();
}
