using ApplicationServices;
using LibationAvalonia.Views;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using Splat;

namespace LibationAvalonia.ViewModels
{
    public partial class MainVM : ViewModelBase
	{
		public SidebarViewModel Sidebar { get; } = ServiceLocator.Get<SidebarViewModel>();
		public ProductsDisplayViewModel ProductsDisplay { get; } = ServiceLocator.Get<ProductsDisplayViewModel>();

		private double? _downloadProgress = null;
		public double? DownloadProgress { get => _downloadProgress; set => this.RaiseAndSetIfChanged(ref _downloadProgress, value); }

		public readonly MainWindow MainWindow;

        public MainVM(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            ProductsDisplay.RemovableCountChanged += (_, removeCount) => RemoveBooksButtonText = removeCount == 1 ? "Remove 1 Book from Libation" : $"Remove {removeCount} Books from Libation";
			LibraryCommands.LibrarySizeChanged += async (_, _) => await ProductsDisplay.UpdateGridAsync(DbContexts.GetLibrary_Flat_NoTracking(includeParents: true));

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

		private static string menufyText(string header) => Configuration.IsMacOs ? header : $"_{header}";
	}
}
