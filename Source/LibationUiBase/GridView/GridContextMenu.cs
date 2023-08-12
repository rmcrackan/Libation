using ApplicationServices;
using DataLayer;
using FileLiberator;
using LibationFileManager;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LibationUiBase.GridView;

public class GridContextMenu
{
	public string CopyCellText => $"{Accelerator}Copy Cell Contents";
	public string LiberateEpisodesText => $"{Accelerator}Liberate All Episodes";
	public string SetDownloadedText => $"Set Download status to '{Accelerator}Downloaded'";
	public string SetNotDownloadedText => $"Set Download status to '{Accelerator}Not Downloaded'";
	public string RemoveText => $"{Accelerator}Remove from library";
	public string LocateFileText => $"{Accelerator}Locate file...";
	public string LocateFileDialogTitle => $"Locate the audio file for '{GridEntry.Book.TitleWithSubtitle}'";
	public string LocateFileErrorMessage => "Error saving book's location";
	public string ConvertToMp3Text => $"{Accelerator}Convert to Mp3";
	public string ReDownloadText => "Re-download this audiobook";
	public string EditTemplatesText => "Edit Templates";
	public string FolderTemplateText => "Folder Template";
	public string FileTemplateText => "File Template";
	public string MultipartTemplateText => "Multipart File Template";
	public string ViewBookmarksText => "View _Bookmarks/Clips";
	public string ViewSeriesText => GridEntry.Liberate.IsSeries ? "View All Episodes in Series" : "View All Books in Series";

	public bool LiberateEpisodesEnabled => GridEntry is ISeriesEntry sEntry && sEntry.Children.Any(c => c.Liberate.BookStatus is LiberatedStatus.NotLiberated or LiberatedStatus.PartialDownload);
	public bool SetDownloadedEnabled => GridEntry.Book.UserDefinedItem.BookStatus != LiberatedStatus.Liberated || GridEntry.Liberate.IsSeries;
	public bool SetNotDownloadedEnabled => GridEntry.Book.UserDefinedItem.BookStatus != LiberatedStatus.NotLiberated || GridEntry.Liberate.IsSeries;
	public bool ConvertToMp3Enabled => GridEntry.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated;
	public bool ReDownloadEnabled => GridEntry.Book.UserDefinedItem.BookStatus is LiberatedStatus.Liberated;

	public IGridEntry GridEntry { get; }
	public char Accelerator { get; }

	public GridContextMenu(IGridEntry gridEntry, char accelerator)
	{
		GridEntry = gridEntry;
		Accelerator = accelerator;
	}

	public void SetDownloaded()
	{
		if (GridEntry is ISeriesEntry series)
		{
			series.Children.Select(c => c.LibraryBook).UpdateBookStatus(LiberatedStatus.Liberated);
		}
		else
		{
			GridEntry.LibraryBook.UpdateBookStatus(LiberatedStatus.Liberated);
		}
	}

	public void SetNotDownloaded()
	{
		if (GridEntry is ISeriesEntry series)
		{
			series.Children.Select(c => c.LibraryBook).UpdateBookStatus(LiberatedStatus.NotLiberated);
		}
		else
		{
			GridEntry.LibraryBook.UpdateBookStatus(LiberatedStatus.NotLiberated);
		}
	}

	public async Task RemoveAsync()
	{
		if (GridEntry is ISeriesEntry series)
		{
			await series.Children.Select(c => c.LibraryBook).RemoveBooksAsync();
		}
		else
		{
			await Task.Run(GridEntry.LibraryBook.RemoveBook);
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
			using var context = DbContexts.GetContext();
			var seriesParent = context.GetLibraryBook_Flat_NoTracking(series.Series.AudibleSeriesId);
			folderDto = seriesParent?.ToDto() ?? fileDto;
		}

		return TemplateEditor<T>.CreateFilenameEditor(Configuration.Instance.Books, existingTemplate, folderDto, fileDto);
	}


}
class Command : ICommand
{
	public event EventHandler CanExecuteChanged;

	public bool CanExecute(object parameter)
	{
		throw new NotImplementedException();
	}

	public void Execute(object parameter)
	{
		throw new NotImplementedException();
	}
}