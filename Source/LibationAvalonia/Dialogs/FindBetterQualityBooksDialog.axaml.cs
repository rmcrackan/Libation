using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Threading;
using DataLayer;
using LibationUiBase;
using LibationUiBase.Forms;
using LibationUiBase.ProcessQueue;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class FindBetterQualityBooksDialog : DialogWindow
{
	private FindBetterQualityBooksViewModel VM { get; }

	private Task? scanTask;
	public FindBetterQualityBooksDialog()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			var library = Enumerable.Repeat(MockLibraryBook.CreateBook(), 3);
			DataContext = VM = new FindBetterQualityBooksViewModel()
			{
				Books = new AvaloniaList<BookDataViewModel>(library.Select(lb => new BookDataViewModel(lb)))
			};
			VM.Books[0].AvailableCodec = "xHE-AAC";
			VM.Books[0].AvailableBitrate = 256;
			VM.Books[0].ScanStatus = ProcessBookStatus.Completed;
			VM.Books[1].ScanStatus = ProcessBookStatus.Failed;
			VM.Books[2].ScanStatus = ProcessBookStatus.Cancelled;
			VM.SignificantCount = 1;
		}
		else
		{
			DataContext = VM = new FindBetterQualityBooksViewModel();
			VM.BookScanned += VM_BookScanned;
			VM.PropertyChanged += VM_PropertyChanged;
			Opened += Opened_LoadLibrary;
			Opened += Opened_ShowInitialMessage;
			Closing += FindBetterQualityBooksDialog_Closing;
		}
	}

	private async void Opened_ShowInitialMessage(object? sender, System.EventArgs e)
	{
		if (!VM.ShowFindBetterQualityBooksHelp)
			return;
		var result = await MessageBox.Show(this, FindBetterQualityBooksViewModel.InitialMessage, Title ?? "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
		if (result == DialogResult.No)
		{
			VM.ShowFindBetterQualityBooksHelp = false;
		}
	}

	private async void Opened_LoadLibrary(object? sender, System.EventArgs e)
	{
		var library = await Task.Run(() => DbContexts.GetLibrary_Flat_NoTracking());
		VM.Books = new AvaloniaList<BookDataViewModel>(library.Where(FindBetterQualityBooksViewModel.ShouldScan).Select(lb => new BookDataViewModel(lb)));
		Dispatcher.UIThread.Invoke(() => scanBtn.IsEnabled = true);
	}

	private void VM_BookScanned(object? sender, BookDataViewModel e)
	{
		Dispatcher.UIThread.Invoke(() => booksDataGrid.ScrollIntoView(e, booksDataGrid.Columns[0]));
	}

	private void VM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(FindBetterQualityBooksViewModel.IsScanning))
		{
			Dispatcher.UIThread.Invoke(() => scanBtn.IsEnabled = true);
		}
	}

	private async void FindBetterQualityBooksDialog_Closing(object? sender, WindowClosingEventArgs e)
	{
		if (scanTask is not null)
		{
			await scanTask;
			scanTask = null;
			Dispatcher.UIThread.Invoke(Close);
		}
	}
	protected override void OnClosing(WindowClosingEventArgs e)
	{
		if (scanTask is not null)
		{
			this.SaveSizeAndLocation(LibationFileManager.Configuration.Instance);
			e.Cancel = true;
			VM.StopScan();
		}
		base.OnClosing(e);
	}

	public void Scan_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		(sender as Button)?.IsEnabled = false;
		scanTask = Task.Run(async () =>
		{
			try
			{
				if (VM.IsScanning)
					VM.StopScan();
				else
					await VM.ScanAsync();
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Failed to scan for better quality books");
				await MessageBox.Show(this, "An error occurred while scanning for better quality books. Please see the logs for more information.", "Error Scanning Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				Dispatcher.UIThread.Invoke(() =>
				{
					VM.IsScanning = false;
					(sender as Button)?.IsEnabled = true;
				});
			}
		});
	}

	public async void MarkBooks_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		(sender as Button)?.IsEnabled = false;
		try
		{
			await VM.MarkBooksAsync();
		}
		catch (Exception ex)
		{
			Serilog.Log.Error(ex, "Failed to mark books as Not Liberated");
			await MessageBox.Show(this, "An error occurred while marking books as Not Liberated. Please see the logs for more information.", "Error Marking Books", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			Dispatcher.UIThread.Invoke(() => (sender as Button)?.IsEnabled = true);
		}
	}

	public static FuncValueConverter<ProcessBookStatus, IBrush?> RowConverter { get; } = new(status =>
	{
		var brush = status switch
		{
			ProcessBookStatus.Completed => "ProcessQueueBookCompletedBrush",
			ProcessBookStatus.Cancelled => "ProcessQueueBookCancelledBrush",
			ProcessBookStatus.Failed => "ProcessQueueBookFailedBrush",
			_ => null,
		};
		return brush is not null && App.Current.TryGetResource(brush, App.Current.ActualThemeVariant, out var res) ? res as Brush : null;
	});
}