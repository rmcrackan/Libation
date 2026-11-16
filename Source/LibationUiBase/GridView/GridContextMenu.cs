using ApplicationServices;
using DataLayer;
using Dinah.Core;
using FileLiberator;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationUiBase.GridView;

public delegate void LiberateClickedHandler(object? sender, System.Collections.Generic.IList<LibraryBook> libraryBooks, Configuration config);
public class GridContextMenu
{
	public string CopyCellText => string.Format(Strings.GridContextMenu_CopyCellContents, Accelerator);
	public string LiberateEpisodesText => string.Format(Strings.GridContextMenu_LiberateAllEpisodes, Accelerator);
	public string SetDownloadedText => string.Format(Strings.GridContextMenu_MarkAsDownloaded, Accelerator);
	public string SetNotDownloadedText => string.Format(Strings.GridContextMenu_MarkAsDownloadPending, Accelerator);
	public string SetPdfDownloadedText => Strings.GridContextMenu_MarkPDFAsDownloaded;
	public string SetPdfNotDownloadedText => Strings.GridContextMenu_MarkPDFAsDownloadPending;
	public string RemoveText => string.Format(Strings.GridContextMenu_RemoveFromLibrary, Accelerator);
	public string RemoveFromAudibleText => GridEntries.Count(e => e.LibraryBook.IsAudiblePlus) == 1 ? Strings.GridContextMenu_RemovePlusBookFromAudible : Strings.GridContextMenu_RemovePlusBooksFromAudible;
	public string LocateFileText => string.Format(Strings.GridContextMenu_LocateFile, Accelerator);
	public string LocateFileDialogTitle => string.Format(Strings.GridContextMenu_LocateTheAudioFileFor, GridEntries[0].Book?.TitleWithSubtitle ?? "[null]");
	public string LocateFileErrorMessage => Strings.GridContextMenu_ErrorSavingBookSLocation;
	public string ConvertToMp3Text => string.Format(Strings.GridContextMenu_ConvertToMp3, Accelerator);
	public string DownloadAsChapters => string.Format(Strings.GridContextMenu_DownloadSplitByChapters, Accelerator);
	public string ReDownloadText => Strings.GridContextMenu_ReDownloadThisAudiobook;
	public string DownloadSingleText => Strings.GridContextMenu_DownloadThisAudiobook;
	public string DownloadSelectedText => Strings.GridContextMenu_DownloadSelectedAudiobooks;
	public string EditTemplatesText => Strings.GridContextMenu_EditTemplates;
	public string FolderTemplateText => Strings.GridContextMenu_FolderTemplate;
	public string FileTemplateText => Strings.GridContextMenu_FileTemplate;
	public string MultipartTemplateText => Strings.GridContextMenu_MultipartFileTemplate;
	public string ViewBookmarksText => string.Format(Strings.GridContextMenu_ViewBookmarksClips, Accelerator);
	public string ViewSeriesText => GridEntries[0].Liberate?.IsSeries is true ? Strings.GridContextMenu_ViewAllEpisodesInSeries : Strings.GridContextMenu_ViewAllBooksInSeries;

	public bool LiberateEpisodesEnabled => GridEntries.OfType<SeriesEntry>().Any(sEntry => sEntry.Children.Any(c => c.Liberate?.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload));
	public bool SetDownloadedEnabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus != LiberatedStatus.Liberated || ge.Liberate?.IsSeries is true);
	public bool SetNotDownloadedEnabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated || ge.Liberate?.IsSeries is true);

	/// <summary>
	/// Whether to offer the PDF status on its own. The pair above deliberately moves both statuses together,
	/// so resetting a title's PDF from here also queues its audiobook for a fresh download. The only PDF-only
	/// control was the bulk one under Visible Books, which applies to everything on screen, leaving no way to
	/// say it about one title. Offered only for a selection that has a PDF, since for anything else the pair
	/// above is already audio-only.
	/// </summary>
	public bool ShowPdfStatusItems => PdfItems.Any();
	public bool SetPdfDownloadedEnabled => PdfItems.Any(udi => udi.PdfStatus != LiberatedStatus.Liberated);
	public bool SetPdfNotDownloadedEnabled => PdfItems.Any(udi => udi.PdfStatus != LiberatedStatus.NotLiberated);

	private IEnumerable<UserDefinedItem> PdfItems
		=> LibraryBookEntries.Select(ge => ge.Book?.UserDefinedItem).OfType<UserDefinedItem>().Where(udi => udi.Book.HasPdf);
	public bool ConvertToMp3Enabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus is LiberatedStatus.Liberated);
	public bool DownloadBookEnabled => LibraryBookEntries.Any(ge => ge.LibraryBook.NeedsBookDownload || ge.LibraryBook.NeedsPdfDownload);
	public bool DownloadAsChaptersEnabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus is not LiberatedStatus.Error);
	public bool ReDownloadEnabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus is LiberatedStatus.Liberated);
	public bool RemoveFromAudibleEnabled => LibraryBookEntries.Any(ge => ge.LibraryBook.IsAudiblePlus);

	private GridEntry[] GridEntries { get; }
	public LibraryBookEntry[] LibraryBookEntries { get; }
	public char Accelerator { get; }

	public GridContextMenu(GridEntry[] gridEntries, char accelerator)
	{
		ArgumentNullException.ThrowIfNull(gridEntries, nameof(gridEntries));
		ArgumentOutOfRangeException.ThrowIfZero(gridEntries.Length, $"{nameof(gridEntries)}.{nameof(gridEntries.Length)}");

		GridEntries = gridEntries;
		Accelerator = accelerator;
		LibraryBookEntries
			= GridEntries
			.OfType<SeriesEntry>()
			.SelectMany(s => s.Children)
			.Concat(GridEntries.OfType<LibraryBookEntry>())
			.ToArray();
	}

	public Task SetDownloaded()
	{
		return LibraryBookEntries.Select(e => e.LibraryBook).ToArray()
			.UpdateUserDefinedItemAsync(udi =>
			{
				udi.BookStatus = LiberatedStatus.Liberated;
				if (udi.Book.HasPdf)
					udi.SetPdfStatus(LiberatedStatus.Liberated);
			});
	}

	public Task SetNotDownloaded()
	{
		return LibraryBookEntries.Select(e => e.LibraryBook).ToArray()
			.UpdateUserDefinedItemAsync(udi =>
			{
				udi.BookStatus = LiberatedStatus.NotLiberated;
				if (udi.Book.HasPdf)
					udi.SetPdfStatus(LiberatedStatus.NotLiberated);
			});
	}

	public Task SetPdfDownloaded() => setPdfStatus(LiberatedStatus.Liberated);

	public Task SetPdfNotDownloaded() => setPdfStatus(LiberatedStatus.NotLiberated);

	private Task setPdfStatus(LiberatedStatus pdfStatus)
	{
		return LibraryBookEntries.Select(e => e.LibraryBook).ToArray()
			.UpdateUserDefinedItemAsync(udi =>
			{
				if (udi.Book.HasPdf)
					udi.SetPdfStatus(pdfStatus);
			});
	}

	public async Task RemoveAsync()
	{
		await LibraryBookEntries.Select(e => e.LibraryBook).RemoveBooksAsync();
	}

	public async Task RemoveFromAudibleAsync()
	{
		LibraryBook[] toRemove = LibraryBookEntries.Select(l => l.LibraryBook).Where(lb => lb.IsAudiblePlus).ToArray();
		if (toRemove.Length == 0)
			return;

		string confirmMessage = string.Format(
			toRemove.Length == 1 ? Strings.GridContextMenu_AboutToRemoveOneFromAudible : Strings.GridContextMenu_AboutToRemoveManyFromAudible,
			"book".PluralizeWithCount(toRemove.Length),
			toRemove.AggregateTitles());
		DialogResult result = await MessageBoxBase.Show(confirmMessage, Strings.GridContextMenu_ConfirmRemoveFromAudibleLibrary, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
		if (result != DialogResult.Yes)
			return;

		List<LibraryBook> removedFromAudible = [];
		List<LibraryBook> failedToRemove = [];

		//Getting the API loads AccountsSettings every time and es expensive
		//cache Api to improve perfomanc on large batches of deletions
		Dictionary<string, AudibleApi.Api> apis = [];

		foreach (var entry in toRemove)
		{
			try
			{
				if (!apis.TryGetValue(entry.Account, out var api))
				{
					apis[entry.Account] = api = await entry.GetApiAsync();
				}

				bool success = await api.RemoveItemFromLibraryAsync(entry.Book.AudibleProductId);
				if (success)
				{
					removedFromAudible.Add(entry);
				}
				else
				{
					failedToRemove.Add(entry);
				}
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Failed to remove book from audible account. {@Book}", entry.LogFriendly());
				failedToRemove.Add(entry);
			}
		}
		if (failedToRemove.Count > 0)
		{
			string booksStr = "book".PluralizeWithCount(failedToRemove.Count);
			string message = string.Format(Strings.GridContextMenu_FailedToRemoveFromAudible, booksStr, failedToRemove.AggregateTitles());
			await MessageBoxBase.Show(message, string.Format(Strings.GridContextMenu_FailedToRemoveFromAudible2, booksStr));
		}
		try
		{
			await removedFromAudible.PermanentlyDeleteBooksAsync();
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to delete locally removed from Audible books.");

			string booksStr = "book".PluralizeWithCount(removedFromAudible.Count);
			string message = string.Format(Strings.GridContextMenu_FailedToDeleteFromLibation, booksStr, removedFromAudible.AggregateTitles());
			await MessageBoxBase.Show(message, string.Format(Strings.GridContextMenu_FailedToDeleteFromLibation2, booksStr));
		}
	}

	public ITemplateEditor CreateTemplateEditor<T>(LibraryBook libraryBook, string existingTemplate)
			where T : Templates, ITemplate, new()
	{
		LibraryBookDto fileDto = libraryBook.ToDto(), folderDto = fileDto;

		if (libraryBook.Book.IsEpisodeChild() &&
			Configuration.Instance.SavePodcastsToParentFolder &&
			libraryBook.Book.SeriesLink.SingleOrDefault() is SeriesBook series)
		{
			var seriesParent = DbContexts.GetLibraryBook_Flat_NoTracking(series.Series.AudibleSeriesId, account: libraryBook.Account);
			folderDto = seriesParent?.ToDto() ?? fileDto;
		}

		return TemplateEditor<T>.CreateFilenameEditor(Configuration.Instance.Books ?? System.IO.Path.GetTempPath(), existingTemplate, folderDto, fileDto);
	}
}
