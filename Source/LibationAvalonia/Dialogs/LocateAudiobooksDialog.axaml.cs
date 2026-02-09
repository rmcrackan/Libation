using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace LibationAvalonia.Dialogs;

public partial class LocateAudiobooksDialog : DialogWindow
{
	private readonly CancellationTokenSource tokenSource = new();
	private readonly LocatedAudiobooksViewModel _viewModel;
	public LocateAudiobooksDialog()
	{
		InitializeComponent();

		var list = new AvaloniaList<FoundAudiobook>();
		DataContext = _viewModel = new(list);
		list.CollectionChanged += (_, _) => foundFilesDataGrid.ScrollIntoView(list[^1], foundFilesDataGrid.Columns[0]);
		this.RestoreSizeAndLocation(Configuration.Instance);

		if (Design.IsDesignMode)
		{
			_viewModel.AddFoundFile(new("0000000001", FileType.Audio, "Filename 1.m4b"));
			_viewModel.AddFoundFile(new("0000000002", FileType.Audio, "Filename 2.m4b"));
		}
		else
		{
			Opened += LocateAudiobooksDialog_Opened;
			Closing += LocateAudiobooksDialog_Closing;
		}
	}

	private void LocateAudiobooksDialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
	{
		tokenSource.Cancel();
		//If this dialog is closed before it's completed, Closing is fired
		//once for the form closing and again for the MessageBox closing.
		Closing -= LocateAudiobooksDialog_Closing;
		this.SaveSizeAndLocation(Configuration.Instance);
	}

	private async void LocateAudiobooksDialog_Opened(object? sender, EventArgs e)
	{
		var folderPicker = new FolderPickerOpenOptions
		{
			Title = "Select the folder to search for audiobooks",
			AllowMultiple = false,
			SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(Configuration.Instance.Books?.PathWithoutPrefix ?? "")
		};

		var selectedFolder = (await StorageProvider.OpenFolderPickerAsync(folderPicker))?.SingleOrDefault()?.TryGetLocalPath();

		if (selectedFolder is null || !Directory.Exists(selectedFolder))
		{
			await CancelAndCloseAsync();
		}
		else
		{
			await _viewModel.FindAndAddBooksAsync(selectedFolder, tokenSource.Token);
			await MessageBox.Show(this, $"Libation has found {_viewModel.FoundAsinCount} unique audiobooks and added them to its database. ", $"Found {_viewModel.FoundAsinCount} Audiobooks");
		}
	}

	private void foundFilesDataGrid_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
	{
		if (sender is DataGrid dg && dg.SelectedItem is FoundAudiobook foundAudiobook)
			Go.To.File(foundAudiobook.Entry.Path);
	}
}
