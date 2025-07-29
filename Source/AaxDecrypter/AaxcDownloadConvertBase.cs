using AAXClean;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace AaxDecrypter
{	
	public abstract class AaxcDownloadConvertBase : AudiobookDownloadBase
	{
		public event EventHandler<AppleTags>? RetrievedMetadata;

		public Mp4File? AaxFile { get; private set; }
		protected Mp4Operation? AaxConversion { get; set; }

		protected AaxcDownloadConvertBase(string outDirectory, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outDirectory, cacheDirectory, dlOptions) { }

		/// <summary>Setting cover art by this method will insert the art into the audiobook metadata</summary>
		public override void SetCoverArt(byte[] coverArt)
		{
			base.SetCoverArt(coverArt);
			if (coverArt is not null && AaxFile?.AppleTags is not null)
				AaxFile.AppleTags.Cover = coverArt;
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
				var kidIndex = Array.IndexOf(keyIds, dash.Tenc.DefaultKID);

				if (kidIndex == -1)
					throw new InvalidOperationException($"None of the {keyIds.Length} key IDs match the dash file's default KeyID of {dash.Tenc.DefaultKID}");

				keys[0] = keys[kidIndex];
				var keyId = keys[kidIndex].KeyPart1;
				var key = keys[kidIndex].KeyPart2 ?? throw new InvalidOperationException($"{nameof(DownloadOptions.DecryptionKeys)} for '{DownloadOptions.InputType}' must have a non-null decryption key (KeyPart2).");
				dash.SetDecryptionKey(keyId, key);
				WriteKeyFile($"KeyId={Convert.ToHexString(keyId)}{Environment.NewLine}Key={Convert.ToHexString(key)}");
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

			RetrievedMetadata?.Invoke(this, AaxFile.AppleTags);

			if (DownloadOptions.StripUnabridged)
			{
				AaxFile.AppleTags.Title = AaxFile.AppleTags.TitleSansUnabridged;
				AaxFile.AppleTags.Album = AaxFile.AppleTags.Album?.Replace(" (Unabridged)", "");
			}

			if (DownloadOptions.FixupFile)
			{
				if (!string.IsNullOrWhiteSpace(AaxFile.AppleTags.Narrator))
					AaxFile.AppleTags.AppleListBox.EditOrAddTag("©wrt", AaxFile.AppleTags.Narrator);

				if (!string.IsNullOrWhiteSpace(AaxFile.AppleTags.Copyright))
					AaxFile.AppleTags.Copyright = AaxFile.AppleTags.Copyright.Replace("(P)", "℗").Replace("&#169;", "©");

				//Add audiobook shelf tags
				//https://github.com/advplyr/audiobookshelf/issues/1794#issuecomment-1565050213
				const string tagDomain = "com.pilabor.tone";

				AaxFile.AppleTags.Title = DownloadOptions.Title;

				if (DownloadOptions.Subtitle is string subtitle)
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "SUBTITLE", subtitle);

				if (DownloadOptions.Publisher is string publisher)
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "PUBLISHER", publisher);

				if (DownloadOptions.Language is string language)
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "LANGUAGE", language);

				if (DownloadOptions.AudibleProductId is string asin)
				{
					AaxFile.AppleTags.Asin = asin;
					AaxFile.AppleTags.AppleListBox.EditOrAddTag("asin", asin);
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "AUDIBLE_ASIN", asin);
				}

				if (DownloadOptions.SeriesName is string series)
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "SERIES", series);

				if (DownloadOptions.SeriesNumber is string part)
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "PART", part);
			}

			OnRetrievedTitle(AaxFile.AppleTags.TitleSansUnabridged);
			OnRetrievedAuthors(AaxFile.AppleTags.FirstAuthor);
			OnRetrievedNarrators(AaxFile.AppleTags.Narrator);
			OnRetrievedCoverArt(AaxFile.AppleTags.Cover);
			OnInitialized();

			return !IsCanceled;
		}

		protected virtual void OnInitialized() { }
	}
}
