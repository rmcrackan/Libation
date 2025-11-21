using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using DataLayer;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.Dialogs
{
	public partial class LocateAudiobooksDialog : DialogWindow
	{
		private event EventHandler<FilePathCache.CacheEntry>? FileFound;
		private readonly CancellationTokenSource tokenSource = new();
		private readonly List<string> foundAsins = new();
		private readonly LocatedAudiobooksViewModel _viewModel;
		public LocateAudiobooksDialog()
		{
			InitializeComponent();

			DataContext = _viewModel = new();
			this.RestoreSizeAndLocation(Configuration.Instance);

			if (Design.IsDesignMode)
			{
				_viewModel.FoundFiles.Add(new("[0000001]", "Filename 1.m4b"));
				_viewModel.FoundFiles.Add(new("[0000002]", "Filename 2.m4b"));
			}
			else
			{
				Opened += LocateAudiobooksDialog_Opened;
				FileFound += LocateAudiobooks_FileFound;
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

		private void LocateAudiobooks_FileFound(object? sender, FilePathCache.CacheEntry e)
		{
			var newItem = new Tuple<string, string>($"[{e.Id}]", Path.GetFileName(e.Path));
			_viewModel.FoundFiles.Add(newItem);
			foundAudiobooksLB.SelectedItem = newItem;

			if (!foundAsins.Any(asin => asin == e.Id))
			{
				foundAsins.Add(e.Id);
				_viewModel.FoundAsins = foundAsins.Count;
			}
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
				return;
			}

			await foreach (var book in AudioFileStorage.FindAudiobooksAsync(selectedFolder, tokenSource.Token))
			{
				try
				{
					FilePathCache.Insert(book);

					var lb = DbContexts.GetLibraryBook_Flat_NoTracking(book.Id);
					if (lb is not null && lb.Book?.UserDefinedItem.BookStatus is not LiberatedStatus.Liberated)
						await lb.UpdateBookStatusAsync(LiberatedStatus.Liberated);

					tokenSource.Token.ThrowIfCancellationRequested();
					FileFound?.Invoke(this, book);
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "Error adding found audiobook file to Libation. {@audioFile}", book);
				}
			}

			await MessageBox.Show(this, $"Libation has found {foundAsins.Count} unique audiobooks and added them to its database. ", $"Found {foundAsins.Count} Audiobooks");
			await SaveAndCloseAsync();
		}
	}

	public class LocatedAudiobooksViewModel : ViewModelBase
	{
		private int _foundAsins = 0;
		public AvaloniaList<Tuple<string, string>> FoundFiles { get; } = new();
		public int FoundAsins { get => _foundAsins; set => this.RaiseAndSetIfChanged(ref _foundAsins, value); }
	}
}
