using ApplicationServices;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using LibationFileManager.Templates;
using System;
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
	public string LocateFileText => $"{Accelerator}Locate file...";
	public string LocateFileDialogTitle => $"Locate the audio file for '{GridEntries[0].Book.TitleWithSubtitle}'";
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
	public string ViewSeriesText => GridEntries[0].Liberate.IsSeries ? "View All Episodes in Series" : "View All Books in Series";

	public bool LiberateEpisodesEnabled => GridEntries.OfType<SeriesEntry>().Any(sEntry => sEntry.Children.Any(c => c.Liberate.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload));
	public bool SetDownloadedEnabled => LibraryBookEntries.Any(ge => ge.Book.UserDefinedItem.BookStatus != LiberatedStatus.Liberated || ge.Liberate.IsSeries);
	public bool SetNotDownloadedEnabled => LibraryBookEntries.Any(ge => ge.Book.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated || ge.Liberate.IsSeries);
	public bool ConvertToMp3Enabled => LibraryBookEntries.Any(ge => ge.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated);
	public bool DownloadAsChaptersEnabled => LibraryBookEntries.Any(ge => ge.Book.UserDefinedItem.BookStatus is not LiberatedStatus.Error);
	public bool ReDownloadEnabled => LibraryBookEntries.Any(ge => ge.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated);

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

		return TemplateEditor<T>.CreateFilenameEditor(Configuration.Instance.Books, existingTemplate, folderDto, fileDto);
	}
}