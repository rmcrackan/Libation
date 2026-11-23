using Avalonia;
using Avalonia.Controls;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationUiBase.GridView;
using LibationUiBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Views;

public partial class BookInspector : UserControl
{
	public static readonly StyledProperty<GridEntry?> EntryProperty =
		AvaloniaProperty.Register<BookInspector, GridEntry?>(nameof(Entry));
	public static readonly StyledProperty<string?> EmptyTextProperty =
		AvaloniaProperty.Register<BookInspector, string?>(nameof(EmptyText));

	public static readonly StyledProperty<IReadOnlyList<GridEntry>?> SelectionProperty =
		AvaloniaProperty.Register<BookInspector, IReadOnlyList<GridEntry>?>(nameof(Selection));

	public event EventHandler? EditSelectionTagsClicked;
	public event LiberateClickedHandler? LiberateClicked;
	public event EventHandler<LibraryBook>? DetailsClicked;

	private readonly BookInspectorViewModel viewModel = new();

	public GridEntry? Entry { get => GetValue(EntryProperty); set => SetValue(EntryProperty, value); }
	public IReadOnlyList<GridEntry>? Selection { get => GetValue(SelectionProperty); set => SetValue(SelectionProperty, value); }
	public string? EmptyText { get => GetValue(EmptyTextProperty); set => SetValue(EmptyTextProperty, value); }

	public BookInspector()
	{
		InitializeComponent();
		root.DataContext = viewModel;
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);
		if (change.Property == EntryProperty)
		{
			tagEditor.CommitPending();
			viewModel.SetEntry(Entry);
		}
		else if (change.Property == SelectionProperty)
			viewModel.SetSelection(Selection);
	}

	private void Primary_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (Entry is LibraryBookEntry entry)
			LiberateClicked?.Invoke(this, [entry.LibraryBook], Configuration.Instance);
	}

	private void ShowFile_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => viewModel.ShowFile();
	private void Audible_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => viewModel.OpenInAudible();

	private void Details_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (Entry is LibraryBookEntry entry)
			DetailsClicked?.Invoke(this, entry.LibraryBook);
	}

	private void BulkLiberate_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (viewModel.Bulk is { } menu)
			LiberateClicked?.Invoke(this, menu.LibraryBookEntries.Select(b => b.LibraryBook).UnLiberated().ToList(), Configuration.Instance);
	}

	private void BulkEditTags_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => EditSelectionTagsClicked?.Invoke(this, EventArgs.Empty);

	private async void BulkSetDownloaded_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await RunBulkAsync(m => m.SetDownloaded(), "Could not update download status.");
	private async void BulkSetNotDownloaded_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await RunBulkAsync(m => m.SetNotDownloaded(), "Could not update download status.");
	private async void BulkSetPdfDownloaded_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await RunBulkAsync(m => m.SetPdfDownloaded(), "Could not update download status.");
	private async void BulkSetPdfNotDownloaded_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await RunBulkAsync(m => m.SetPdfNotDownloaded(), "Could not update download status.");

	private async void BulkRemove_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (viewModel.Bulk is not { } menu)
			return;

		var confirmed = await MessageBox.ShowConfirmationDialog(
			this.GetParentWindow(),
			menu.LibraryBookEntries.Select(b => b.LibraryBook).ToList(),
			// do not use `$` string interpolation. See impl.
			"Are you sure you want to remove {0} from Libation's library?",
			"Remove books from Libation?",
			LibationUiBase.Forms.MessageBoxDefaultButton.Button2);

		if (confirmed is LibationUiBase.Forms.DialogResult.Yes)
			await RunBulkAsync(m => m.RemoveAsync(), "Could not remove books from the library.");
	}

	private async Task RunBulkAsync(Func<GridContextMenu, Task> operation, string message)
	{
		if (viewModel.Bulk is not { } menu)
			return;

		await LibraryOperation.RunAsync(() => operation(menu),
			ex => MessageBox.ShowAdminAlert(this.GetParentWindow(), message, "Library operation failed", ex));
		viewModel.SetSelection(Selection);
	}
}
