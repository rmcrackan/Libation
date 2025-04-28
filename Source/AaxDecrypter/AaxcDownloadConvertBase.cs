using AAXClean;
using System;
using System.Threading.Tasks;

namespace AaxDecrypter
{	
	public abstract class AaxcDownloadConvertBase : AudiobookDownloadBase
	{
		public event EventHandler<AppleTags> RetrievedMetadata;

		protected Mp4File AaxFile { get; private set; }
		protected Mp4Operation AaxConversion { get; set; }

		protected AaxcDownloadConvertBase(string outFileName, string cacheDirectory, IDownloadOptions dlOptions)
			: base(outFileName, cacheDirectory, dlOptions) { }

		/// <summary>Setting cover art by this method will insert the art into the audiobook metadata</summary>
		public override void SetCoverArt(byte[] coverArt)
		{
			base.SetCoverArt(coverArt);
			if (coverArt is not null && AaxFile?.AppleTags is not null)
				AaxFile.AppleTags.Cover = coverArt;
		}

		public override async Task CancelAsync()
		{
			IsCanceled = true;
			await (AaxConversion?.CancelAsync() ?? Task.CompletedTask);
			FinalizeDownload();
		}

		private Mp4File Open()
		{
			if (DownloadOptions.InputType is FileType.Dash)
			{
				var dash = new DashFile(InputFileStream);
				dash.SetDecryptionKey(DownloadOptions.AudibleKey, DownloadOptions.AudibleIV);
				return dash;
			}
			else if (DownloadOptions.InputType is FileType.Aax)
			{
				var aax = new AaxFile(InputFileStream);
				aax.SetDecryptionKey(DownloadOptions.AudibleKey);
				return aax;
			}
			else if (DownloadOptions.InputType is FileType.Aaxc)
			{
				var aax = new AaxFile(InputFileStream);
				aax.SetDecryptionKey(DownloadOptions.AudibleKey, DownloadOptions.AudibleIV);
				return aax;
			}
			else throw new InvalidOperationException($"{nameof(DownloadOptions.InputType)} of '{DownloadOptions.InputType}' is unknown.");
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

				if (DownloadOptions.SeriesNumber is float part)
					AaxFile.AppleTags.AppleListBox.EditOrAddFreeformTag(tagDomain, "PART", part.ToString());
			}

			OnInitialized();
			OnRetrievedTitle(AaxFile.AppleTags.TitleSansUnabridged);
			OnRetrievedAuthors(AaxFile.AppleTags.FirstAuthor ?? "[unknown]");
			OnRetrievedNarrators(AaxFile.AppleTags.Narrator ?? "[unknown]");
			OnRetrievedCoverArt(AaxFile.AppleTags.Cover);

			return !IsCanceled;
		}

		protected virtual void OnInitialized() { }
	}
}
