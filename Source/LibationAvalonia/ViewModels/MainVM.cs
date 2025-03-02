using ApplicationServices;
using DataLayer;
using LibationAvalonia.Views;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	public partial class MainVM : ViewModelBase
	{
		public ProcessQueueViewModel ProcessQueue { get; } = new ProcessQueueViewModel();
		public ProductsDisplayViewModel ProductsDisplay { get; } = new ProductsDisplayViewModel();

		private double? _downloadProgress = null;
		public double? DownloadProgress { get => _downloadProgress; set => this.RaiseAndSetIfChanged(ref _downloadProgress, value); }


		private readonly MainWindow MainWindow;
		public MainVM(MainWindow mainWindow)
		{
			MainWindow = mainWindow;

			ProductsDisplay.RemovableCountChanged += (_, removeCount) => RemoveBooksButtonText = removeCount == 1 ? "Remove 1 Book from Libation" : $"Remove {removeCount} Books from Libation";
			LibraryCommands.LibrarySizeChanged += LibraryCommands_LibrarySizeChanged;

			Configure_NonUI();
			Configure_BackupCounts();
			Configure_Export();
			Configure_Filters();
			Configure_Import();
			Configure_Liberate();
			Configure_ProcessQueue();
			Configure_ScanAuto();
			Configure_Settings();
			Configure_VisibleBooks();
		}

		private async void LibraryCommands_LibrarySizeChanged(object sender, List<LibraryBook> fullLibrary)
		{
			await Task.WhenAll(
				SetBackupCountsAsync(fullLibrary),
				Task.Run(() => ProductsDisplay.UpdateGridAsync(fullLibrary)));
		}

		private static string menufyText(string header) => Configuration.IsMacOs ? header : $"_{header}";
	}
}
