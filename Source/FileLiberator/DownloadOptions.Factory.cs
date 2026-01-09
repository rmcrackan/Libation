using AaxDecrypter;
using AudibleApi;
using AudibleApi.Common;
using AudibleUtilities.Widevine;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using NAudio.Lame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace FileLiberator;

public partial class DownloadOptions
{
	/// <summary>
	/// Requests a download license from the Api using the Configuration settings to choose the appropriate content.
	/// </summary>
	public static async Task<LicenseInfo> GetDownloadLicenseAsync(Api api, LibraryBook libraryBook, Configuration config, CancellationToken token)
	{
		var license = await ChooseContent(api, libraryBook, config, token);
		Serilog.Log.Logger.Debug("Content License {@License}", new
		{
			license.DrmType,
			license.ContentMetadata.ContentReference
		});

		token.ThrowIfCancellationRequested();

		//Some audiobooks will have incorrect chapters in the metadata returned from the license request,
		//but the metadata returned by the content metadata endpoint will be correct. Call the content
		//metadata endpoint and use its chapters. Only replace the license request chapters if the content
		//references match (defensive against different audio formats having slightly different lengths).
		var metadata = await api.GetContentMetadataAsync(
		   libraryBook.Book.AudibleProductId,
		   license.DrmType,
		   license.ContentMetadata.ContentReference.Acr,
		   license.ContentMetadata.ContentReference.FileVersion);

		if (metadata is null)
		{
			Serilog.Log.Logger.Warning("Unable to retrieve metadata for {@FileReference}", new
			{
				libraryBook.Book.AudibleProductId,
				license.DrmType,
				license.ContentMetadata.ContentReference.Acr,
				license.ContentMetadata.ContentReference.FileVersion
			});
		}
		else if (metadata.ContentReference != license.ContentMetadata.ContentReference)
		{
			Serilog.Log.Logger.Warning("Metadata ContentReference does not match License ContentReference with drm_type = {DrmType}. {@Metadata}. {@License} ",
			license.DrmType,
			metadata.ContentReference,
			license.ContentMetadata.ContentReference);
		}
		else
			license.ContentMetadata.ChapterInfo = metadata.ChapterInfo;

		token.ThrowIfCancellationRequested();
		return license;
	}

	public class LicenseInfo
	{
		public DrmType DrmType { get; set; }
		public ContentMetadata ContentMetadata { get; set; }
		public KeyData[]? DecryptionKeys { get; set; }

		[JsonConstructor]
		private LicenseInfo()
		{
			ContentMetadata = null!;
		}
		public LicenseInfo(ContentLicense license, IEnumerable<KeyData>? keys = null)
		{
			DrmType = license.DrmType;
			ContentMetadata = license.ContentMetadata;
			DecryptionKeys = keys?.ToArray() ?? ToKeys(license.Voucher);
		}

		private static KeyData[]? ToKeys(VoucherDtoV10? voucher)
			=> voucher is null ? null : [new KeyData(voucher.Key, voucher.Iv)];
	}

	private static async Task<LicenseInfo> ChooseContent(Api api, LibraryBook libraryBook, Configuration config, CancellationToken token)
	{
		Serilog.Log.Logger.Information("Download Settings {@Settings}", new
		{
			config.FileDownloadQuality,
			config.UseWidevine,
			config.Request_xHE_AAC,
			config.RequestSpatial,
			config.SpatialAudioCodec
		});

		var dlQuality = config.FileDownloadQuality == Configuration.DownloadQuality.Normal ? DownloadQuality.Normal : DownloadQuality.High;

		bool canUseWidevine = api.SupportsWidevine();
		if (!config.UseWidevine || !canUseWidevine || await Cdm.GetCdmAsync() is not Cdm cdm)
		{
			if (config.UseWidevine)
			{
				if (canUseWidevine)
					Serilog.Log.Logger.Warning("Unable to get a Widevine CDM. Falling back to ADRM.");
				else
					Serilog.Log.Logger.Warning("Account {account} is not registered as an android device, so content will not be downloaded with Widevine DRM. Remove and re-add the account in Libation to fix.", libraryBook.Account.ToMask());
			}

			token.ThrowIfCancellationRequested();
			var license = await api.GetDownloadLicenseAsync(libraryBook.Book.AudibleProductId, dlQuality);
			return new LicenseInfo(license);
		}

		token.ThrowIfCancellationRequested();
		try
		{
			//try to request a widevine content license using the user's audio settings
			var aacCodecChoice = config.Request_xHE_AAC ? Codecs.xHE_AAC : Codecs.AAC_LC;
			//Always use the ec+3 codec if converting to mp3
			var spatialCodecChoice = config.SpatialAudioCodec is Configuration.SpatialCodec.AC_4 ? Codecs.AC_4 : Codecs.EC_3;

			var contentLic
				= await api.GetDownloadLicenseAsync(
					libraryBook.Book.AudibleProductId,
					dlQuality,
					ChapterTitlesType.Tree,
					DrmType.Widevine,
					config.RequestSpatial,
					aacCodecChoice,
					spatialCodecChoice);

			if (contentLic.DrmType is not DrmType.Widevine)
				return new LicenseInfo(contentLic);

			using var client = new HttpClient();
			using var mpdResponse = await client.GetAsync(contentLic.LicenseResponse, token);
			var dash = new MpegDash(mpdResponse.Content.ReadAsStream(token));

			if (!dash.TryGetUri(new Uri(contentLic.LicenseResponse), out var contentUri))
				throw new InvalidDataException("Failed to get mpeg-dash content download url.");

			contentLic.ContentMetadata.ContentUrl = new() { OfflineUrl = contentUri.ToString() };

			using var session = cdm.OpenSession();
			var challenge = session.GetLicenseChallenge(dash);
			var licenseMessage = await api.WidevineDrmLicense(libraryBook.Book.AudibleProductId, challenge);
			var keys = session.ParseLicense(licenseMessage);
			return new LicenseInfo(contentLic, keys.Select(k => new KeyData(k.Kid.ToByteArray(bigEndian: true), k.Key)));
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to request a Widevine license.");
			//We failed to get a widevine content license. Depending on the
			//failure reason, users can potentially still download this audiobook
			//by disabling the "Use Widevine DRM" feature.
			throw;
		}
	}

	/// <summary>
	/// Builds DownloadOptions from the given LibraryBook, Configuration, and LicenseInfo.
	/// </summary>
	public static DownloadOptions BuildDownloadOptions(LibraryBook libraryBook, Configuration config, LicenseInfo licInfo)
	{
		var titleConcat = config.CombineNestedChapterTitles ? ": " : null;
		var chapters
			= flattenChapters(licInfo.ContentMetadata.ChapterInfo.Chapters, titleConcat)
			.OrderBy(c => c.StartOffsetMs)
			.ToList();

		if (config.MergeOpeningAndEndCredits)
			combineCredits(chapters);

		if (config.StripAudibleBrandAudio)
			stripBranding(chapters, licInfo.ContentMetadata.ChapterInfo.BrandIntroDurationMs, licInfo.ContentMetadata.ChapterInfo.BrandOutroDurationMs);

		if (config.SplitFilesByChapter)
			combineShortChapters(chapters, config.MinimumFileDuration * 1000);

		var dlOptions = new DownloadOptions(config, libraryBook, licInfo)
		{
			ChapterInfo = new AAXClean.ChapterInfo(TimeSpan.FromMilliseconds(chapters[0].StartOffsetMs)),
			RuntimeLength = TimeSpan.FromMilliseconds(licInfo.ContentMetadata.ChapterInfo.RuntimeLengthMs),
		};

		//Build AAXClean.ChapterInfo
		for (int i = 0; i < chapters.Count; i++)
		{
			dlOptions.ChapterInfo.AddChapter(chapters[i].Title, TimeSpan.FromMilliseconds(chapters[i].LengthMs));
		}

		return dlOptions;
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

	public static List<Chapter> flattenChapters(IList<Chapter>? chapters, string? titleConcat = ": ")
	{
		if (chapters is null)
			return [];

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

	/*
	https://github.com/rmcrackan/Libation/pull/127#issuecomment-939088489

	If the chapter truly is empty, that is, 0 audio frames in length, then yes it is ignored.
	If the chapter is shorter than minChapterLength but still has some audio frames, those
	frames are combined with the following chapter and not split into a new file.

	When 2 or more consecutive chapters are combined, chapter titles are concatenated
	with a apace in between. For example, given an audiobook with the following chapters:

	00:00:00 - 00:00:02 | Part 1
	00:00:02 - 00:35:00 | Chapter 1
	00:35:02 - 01:02:00 | Chapter 2
	01:02:00 - 01:02:02 | Part 2
	01:02:02 - 01:41:00 | Chapter 3
	01:41:00 - 02:05:00 | Chapter 4

	The book will be split into the following files:

	00:00:00 - 00:35:00 | Book - 01 - Part 1 Chapter 1.m4b
	00:35:00 - 01:02:00 | Book - 02 - Chapter 2.m4b
	01:02:00 - 01:41:00 | Book - 03 - Part 2.m4b
	01:41:00 - 02:05:00 | Book - 04 - Chapter 4.m4b
	*/
	public static void combineShortChapters(List<Chapter> chapters, long minChapterLengthMs)
	{
		for (int i = 0; i < chapters.Count; i++)
		{
			while (chapters[i].LengthMs < minChapterLengthMs && chapters.Count > i + 1)
			{
				chapters[i].Title += " " + chapters[i + 1].Title;
				chapters[i].LengthMs += chapters[i + 1].LengthMs;
				chapters.RemoveAt(i + 1);
			}
		}
	}

	public static void stripBranding(List<Chapter> chapters, long introMs, long outroMs)
	{
		chapters[0].LengthMs -= introMs;
		chapters[0].StartOffsetMs += introMs;
		chapters[^1].LengthMs -= outroMs;
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
}
