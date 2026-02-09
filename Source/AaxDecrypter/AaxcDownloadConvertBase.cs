using AAXClean;
using Mpeg4Lib;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AaxDecrypter;

public abstract class AaxcDownloadConvertBase : AudiobookDownloadBase
{
	public event EventHandler<MetadataItems>? RetrievedMetadata;

	public Mp4File? AaxFile { get; private set; }
	protected Mp4Operation? AaxConversion { get; set; }

	protected AaxcDownloadConvertBase(string outDirectory, string cacheDirectory, IDownloadOptions dlOptions)
		: base(outDirectory, cacheDirectory, dlOptions) { }

	/// <summary>Setting cover art by this method will insert the art into the audiobook metadata</summary>
	public override void SetCoverArt(byte[] coverArt)
	{
		base.SetCoverArt(coverArt);
		if (coverArt is not null && AaxFile?.MetadataItems is not null)
			AaxFile.MetadataItems.Cover = coverArt;
	}

	public override async Task CancelAsync()
	{
		await base.CancelAsync();
		await (AaxConversion?.CancelAsync() ?? Task.CompletedTask);
	}

	private Mp4File Open()
	{
		if (DownloadOptions.DecryptionKeys is not KeyData[] keys || keys.Length == 0)
			throw new InvalidOperationException($"{nameof(DownloadOptions.DecryptionKeys)} cannot be null or empty for a '{DownloadOptions.InputType}' file.");
		else if (DownloadOptions.InputType is FileType.Dash)
		{
			//We may have multiple keys , so use the key whose key ID matches
			//the dash files default Key ID.
			var keyIds = keys.Select(k => new Guid(k.KeyPart1, bigEndian: true)).ToArray();

			var dash = new DashFile(InputFileStream);
			if (dash.Tenc is null)
				throw new InvalidOperationException("The DASH file does not contain 'tenc' box, indicating that it is unencrypted.");

			var kidIndex = Array.IndexOf(keyIds, dash.Tenc.DefaultKID);
			if (kidIndex == -1)
				throw new InvalidOperationException($"None of the {keyIds.Length} key IDs match the dash file's default KeyID of {dash.Tenc.DefaultKID}");

			keys[0] = keys[kidIndex];
			var keyId = keys[kidIndex].KeyPart1;
			var key = keys[kidIndex].KeyPart2 ?? throw new InvalidOperationException($"{nameof(DownloadOptions.DecryptionKeys)} for '{DownloadOptions.InputType}' must have a non-null decryption key (KeyPart2).");
			dash.SetDecryptionKey(keyId, key);
			WriteKeyFile($"KeyId={Convert.ToHexString(keyId)}{Environment.NewLine}Key={Convert.ToHexString(key)}");

			//Remove meta box containing DRM info
			if (DownloadOptions.FixupFile && dash.Moov.GetChild<Mpeg4Lib.Boxes.MetaBox>() is { } meta)
				dash.Moov.Children.Remove(meta);

			return dash;
		}
		else if (DownloadOptions.InputType is FileType.Aax)
		{
			var aax = new AaxFile(InputFileStream);
			var key = keys[0].KeyPart1;
			aax.SetDecryptionKey(keys[0].KeyPart1);
			WriteKeyFile($"ActivationBytes={Convert.ToHexString(key)}");
			return aax;
		}
		else if (DownloadOptions.InputType is FileType.Aaxc)
		{
			var aax = new AaxFile(InputFileStream);
			var key = keys[0].KeyPart1;
			var iv = keys[0].KeyPart2 ?? throw new InvalidOperationException($"{nameof(DownloadOptions.DecryptionKeys)} for '{DownloadOptions.InputType}' must have a non-null initialization vector (KeyPart2).");
			aax.SetDecryptionKey(keys[0].KeyPart1, iv);
			WriteKeyFile($"Key={Convert.ToHexString(key)}{Environment.NewLine}IV={Convert.ToHexString(iv)}");
			return aax;
		}
		else throw new InvalidOperationException($"{nameof(DownloadOptions.InputType)} of '{DownloadOptions.InputType}' is unknown.");

		void WriteKeyFile(string contents)
		{
			var keyFile = Path.Combine(Path.ChangeExtension(InputFileStream.SaveFilePath, ".key"));
			File.WriteAllText(keyFile, contents + Environment.NewLine);
			OnTempFileCreated(new(keyFile));
		}
	}

	protected bool Step_GetMetadata()
	{
		AaxFile = Open();

		RetrievedMetadata?.Invoke(this, AaxFile.MetadataItems);

		if (DownloadOptions.StripUnabridged)
		{
			AaxFile.MetadataItems.Title = AaxFile.MetadataItems.TitleSansUnabridged;
			AaxFile.MetadataItems.Album = AaxFile.MetadataItems.Album?.Replace(" (Unabridged)", "");
		}

		if (DownloadOptions.FixupFile)
		{
			if (!string.IsNullOrWhiteSpace(AaxFile.MetadataItems.Narrator))
				AaxFile.MetadataItems.AppleListBox.EditOrAddTag("©wrt", AaxFile.MetadataItems.Narrator);

			if (!string.IsNullOrWhiteSpace(AaxFile.MetadataItems.Copyright))
				AaxFile.MetadataItems.Copyright = AaxFile.MetadataItems.Copyright.Replace("(P)", "℗").Replace("&#169;", "©");

			//Add audiobook shelf tags
			//https://github.com/advplyr/audiobookshelf/issues/1794#issuecomment-1565050213
			const string tagDomain = "com.pilabor.tone";

			AaxFile.MetadataItems.Title = DownloadOptions.Title;

			if (DownloadOptions.Subtitle is string subtitle)
				AaxFile.MetadataItems.AppleListBox.EditOrAddFreeformTag(tagDomain, "SUBTITLE", subtitle);

			if (DownloadOptions.Publisher is string publisher)
				AaxFile.MetadataItems.AppleListBox.EditOrAddFreeformTag(tagDomain, "PUBLISHER", publisher);

			if (DownloadOptions.Language is string language)
				AaxFile.MetadataItems.AppleListBox.EditOrAddFreeformTag(tagDomain, "LANGUAGE", language);

			if (DownloadOptions.AudibleProductId is string asin)
			{
				AaxFile.MetadataItems.Asin = asin;
				AaxFile.MetadataItems.AppleListBox.EditOrAddTag("asin", asin);
				AaxFile.MetadataItems.AppleListBox.EditOrAddFreeformTag(tagDomain, "AUDIBLE_ASIN", asin);
			}

			if (DownloadOptions.SeriesName is string series)
				AaxFile.MetadataItems.AppleListBox.EditOrAddFreeformTag(tagDomain, "SERIES", series);

			if (DownloadOptions.SeriesNumber is string part)
				AaxFile.MetadataItems.AppleListBox.EditOrAddFreeformTag(tagDomain, "PART", part);
		}

		OnRetrievedTitle(AaxFile.MetadataItems.TitleSansUnabridged);
		OnRetrievedAuthors(AaxFile.MetadataItems.FirstAuthor);
		OnRetrievedNarrators(AaxFile.MetadataItems.Narrator);
		OnRetrievedCoverArt(AaxFile.MetadataItems.Cover);
		OnInitialized();

		return !IsCanceled;
	}

	protected virtual void OnInitialized() { }
}
