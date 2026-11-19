using ApplicationServices;
using DataLayer;
using LibationAvalonia.Views;
using LibationFileManager;
using LibationUiBase.ProcessQueue;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationAvalonia.ViewModels;

public partial class MainVM : ViewModelBase
{
	public Task? BindToGridTask { get; set; }
	public ProcessQueueViewModel ProcessQueue { get; } = new ProcessQueueViewModel();
	public ProductsDisplayViewModel ProductsDisplay { get; } = new() { SearchEngine = MainSearchEngine.Instance };

	public double? DownloadProgress { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }


	private readonly MainWindow MainWindow;
	public MainVM(MainWindow mainWindow)
	{
		MainWindow = mainWindow;

		ProductsDisplay.RemovableCountChanged += (_, removeCount) => RemoveBooksButtonText = removeCount == 1 ? Strings.MainVM_Remove1BookFromLibation : string.Format(Strings.MainVM_RemoveBooksFromLibation3, removeCount);
		LibraryCommands.LibrarySizeChanged += LibraryCommands_LibrarySizeChanged;

		Configure_NonUI();
		Configure_BackupCounts();
		Configure_Export();
		Configure_Filters();
		Configure_Import();
		Configure_Liberate();
		Configure_ProcessQueue();
		Configure_ScanAuto();
		Configure_SearchIndex();
		Configure_Settings();
		Configure_VisibleBooks();
	}

	private async void LibraryCommands_LibrarySizeChanged(object? sender, List<LibraryBook> fullLibrary)
	{
		try
		{
			//Prevent race condition which can occur if an auto-scan
			//completes before the initial grid binding completes.
			if (BindToGridTask is null)
				return;
			else if (BindToGridTask.IsCompleted is false)
				await BindToGridTask;

			await Task.WhenAll(
				SetBackupCountsAsync(fullLibrary),
				RefreshBooksInTrashAsync(),
				Task.Run(() => ProductsDisplay.UpdateGridAsync(fullLibrary)));
		}
		catch (System.Exception ex)
		{
			await MessageBox.ShowAdminAlert(MainWindow, Strings.MainVM_AnErrorOccurredWhileUpdatingThe, Strings.MainVM_LibrarySizeChangeError, ex);
		}
	}

	private static string menufyText(string header) => Configuration.IsMacOs ? header : $"_{header}";
}
