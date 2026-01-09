using ApplicationServices;
using DataLayer;
using Dinah.Core;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Wordprocessing;
using FileLiberator;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase.Forms;
using Lucene.Net.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationUiBase.GridView;

public delegate void LiberateClickedHandler(object sender, System.Collections.Generic.IList<LibraryBook> libraryBooks, Configuration config);
public class GridContextMenu
{
	public string CopyCellText => $"{Accelerator}Copy Cell Contents";
	public string LiberateEpisodesText => $"{Accelerator}Liberate All Episodes";
	public string SetDownloadedText => $"Set Download status to '{Accelerator}Downloaded'";
	public string SetNotDownloadedText => $"Set Download status to '{Accelerator}Not Downloaded'";
	public string RemoveText => $"{Accelerator}Remove from library";
	public string RemoveFromAudibleText => $"Remove Plus {(GridEntries.Count(e => e.LibraryBook.IsAudiblePlus) == 1 ? "Book" : "Books")} from Audible Library";
	public string LocateFileText => $"{Accelerator}Locate file...";
	public string LocateFileDialogTitle => $"Locate the audio file for '{GridEntries[0].Book?.TitleWithSubtitle ?? "[null]"}'";
	public string LocateFileErrorMessage => "Error saving book's location";
	public string ConvertToMp3Text => $"{Accelerator}Convert to Mp3";
	public string DownloadAsChapters => $"Download {Accelerator}split by chapters";
	public string ReDownloadText => "Re-download this audiobook";
	public string DownloadSelectedText => "Download selected audiobooks";
	public string EditTemplatesText => "Edit Templates";
	public string FolderTemplateText => "Folder Template";
	public string FileTemplateText => "File Template";
	public string MultipartTemplateText => "Multipart File Template";
	public string ViewBookmarksText => $"View {Accelerator}Bookmarks/Clips";
	public string ViewSeriesText => GridEntries[0].Liberate?.IsSeries is true ? "View All Episodes in Series" : "View All Books in Series";

	public bool LiberateEpisodesEnabled => GridEntries.OfType<SeriesEntry>().Any(sEntry => sEntry.Children.Any(c => c.Liberate?.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload));
	public bool SetDownloadedEnabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus != LiberatedStatus.Liberated || ge.Liberate?.IsSeries is true);
	public bool SetNotDownloadedEnabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated || ge.Liberate?.IsSeries is true);
	public bool ConvertToMp3Enabled => LibraryBookEntries.Any(ge => ge.Book?.UserDefinedItem.BookStatus is LiberatedStatus.Liberated);
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

	public void SetDownloaded()
	{
		LibraryBookEntries.Select(e => e.LibraryBook)
			.UpdateUserDefinedItemAsync(udi =>
			{
				udi.BookStatus = LiberatedStatus.Liberated;
				if (udi.Book.HasPdf)
					udi.SetPdfStatus(LiberatedStatus.Liberated);
			});
	}

	public void SetNotDownloaded()
	{
		LibraryBookEntries.Select(e => e.LibraryBook)
			.UpdateUserDefinedItemAsync(udi =>
			{
				udi.BookStatus = LiberatedStatus.NotLiberated;
				if (udi.Book.HasPdf)
					udi.SetPdfStatus(LiberatedStatus.NotLiberated);
			});
	}

	public async Task RemoveAsync()
	{
		await LibraryBookEntries.Select(e => e.LibraryBook).RemoveBooksAsync();
	}

	public async Task RemoveFromAudibleAsync()
	{
		List<LibraryBook> removedFromAudible = [];
		List<LibraryBook> failedToRemove = [];

		foreach (var entry in LibraryBookEntries.Select(l => l.LibraryBook).Where(lb => lb.IsAudiblePlus))
		{
			try
			{
				var api = await entry.GetApiAsync();
				var success = await api.RemoveItemFromLibraryAsync(entry.Book.AudibleProductId);
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
			var count = failedToRemove.Count;
			string bookBooks = count == 1 ? "book" : "books";

			var message = $"""
				Failed to remove {count} {bookBooks} from Audible.

				{failedToRemove.AggregateTitles()}
				""";
			await MessageBoxBase.Show(message, $"Failed to Remove {bookBooks.FirstCharToUpper()} from Audible");
		}
		try
		{
			await removedFromAudible.PermanentlyDeleteBooksAsync();
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to delete locally removed from Audible books.");
			
			var count = removedFromAudible.Count;
			string bookBooks = count == 1 ? "book" : "books";

			var message = $"""
				Failed to delete {count} {bookBooks} from Libation.

				{removedFromAudible.AggregateTitles()}
				""";
			await MessageBoxBase.Show(message, $"Failed to Delete {bookBooks.FirstCharToUpper()} from Libation");
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
			var seriesParent = DbContexts.GetLibraryBook_Flat_NoTracking(series.Series.AudibleSeriesId);
			folderDto = seriesParent?.ToDto() ?? fileDto;
		}

		return TemplateEditor<T>.CreateFilenameEditor(Configuration.Instance.Books ?? System.IO.Path.GetTempPath(), existingTemplate, folderDto, fileDto);
	}
}