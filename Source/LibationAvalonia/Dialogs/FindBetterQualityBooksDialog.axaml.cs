using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using DataLayer;
using LibationUiBase;
using System.Linq;

namespace LibationAvalonia.Dialogs;

public partial class FindBetterQualityBooksDialog : DialogWindow
{
    private FindBetterQualityBooksViewModel VM { get; }
	public FindBetterQualityBooksDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			var library = Enumerable.Repeat(MockLibraryBook.CreateBook(), 3);
			AvaloniaList<FindBetterQualityBooksViewModel.BookData> list = new(library.Select(lb => new FindBetterQualityBooksViewModel.BookData(lb)));
			DataContext = VM = new FindBetterQualityBooksViewModel(list);
			VM.Books[0].AvailableCodec = "xHE-AAC";
			VM.Books[0].AvailableBitrate = 256;
			VM.Books[0].ScanStatus = FindBetterQualityBooksViewModel.ScanStatus.Completed;
			VM.Books[1].ScanStatus = FindBetterQualityBooksViewModel.ScanStatus.Error;
			VM.Books[2].ScanStatus = FindBetterQualityBooksViewModel.ScanStatus.Cancelled;
			VM.SignificantCount = 1;
		}
		else
		{
			var library = DbContexts.GetLibrary_Flat_NoTracking();
			AvaloniaList<FindBetterQualityBooksViewModel.BookData> list = new(library.Where(FindBetterQualityBooksViewModel.ShouldScan).Select(lb => new FindBetterQualityBooksViewModel.BookData(lb)));
			DataContext = VM = new FindBetterQualityBooksViewModel(list);
			VM.BookScanned += VM_BookScanned;
		}
	}

	private void VM_BookScanned(object? sender, FindBetterQualityBooksViewModel.BookData e)
	{
		booksDataGrid.ScrollIntoView(e, booksDataGrid.Columns[0]);
	}


	public static FuncValueConverter<FindBetterQualityBooksViewModel.ScanStatus, IBrush?> RowConverter { get; } = new(status =>
    {
        var brush = status switch
        {
			FindBetterQualityBooksViewModel.ScanStatus.Completed => "ProcessQueueBookCompletedBrush",
			FindBetterQualityBooksViewModel.ScanStatus.Cancelled => "ProcessQueueBookCancelledBrush",
			FindBetterQualityBooksViewModel.ScanStatus.Error => "ProcessQueueBookFailedBrush",
            _ => null,
        };
        return brush is not null && App.Current.TryGetResource(brush, App.Current.ActualThemeVariant, out var res) ? res as Brush : null;
    });
}