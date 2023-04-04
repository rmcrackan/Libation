using ApplicationServices;
using AudibleApi.Common;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using DataLayer;
using FileLiberator;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public partial class BookRecordsDialog : DialogWindow
	{
		public DataGridCollectionView DataGridCollectionView { get; }
		private readonly AvaloniaList<BookRecordEntry> bookRecordEntries = new();
		private readonly LibraryBook libraryBook;
		public BookRecordsDialog()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
			{
				bookRecordEntries.Add(new BookRecordEntry(new Clip(DateTimeOffset.Now.AddHours(1), TimeSpan.FromHours(6.8667), "xxxxxxx", DateTimeOffset.Now.AddHours(1), TimeSpan.FromHours(6.8668), "Note 2", "title 2")));
				bookRecordEntries.Add(new BookRecordEntry(new Clip(DateTimeOffset.Now, TimeSpan.FromHours(4.5667), "xxxxxxx", DateTimeOffset.Now, TimeSpan.FromHours(4.5668), "Note", "title")));
			}

			DataGridCollectionView = new DataGridCollectionView(bookRecordEntries);
			DataContext = this;
		}

		public BookRecordsDialog(LibraryBook libraryBook) : this()
		{
			this.libraryBook = libraryBook;
			Title = $"{libraryBook.Book.Title} - Clips and Bookmarks";

			Loaded += BookRecordsDialog_Loaded;
		}

		private async void BookRecordsDialog_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			try
			{
				var api = await libraryBook.GetApiAsync();
				var records = await api.GetRecordsAsync(libraryBook.Book.AudibleProductId);

				bookRecordEntries.AddRange(records.Select(r => new BookRecordEntry(r)));
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, "Failed to retrieve records for {libraryBook}", libraryBook);
			}
		}

		#region Buttons

		private async Task setControlEnabled(object control, bool enabled)
		{
			if (control is InputElement c)
				await Dispatcher.UIThread.InvokeAsync(() => c.IsEnabled = enabled);
		}
		public async void ExportChecked_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			await setControlEnabled(sender, false);
			await saveRecords(bookRecordEntries.Where(r => r.IsChecked).Select(r => r.Record));
			await setControlEnabled(sender, true);
		}
		public async void ExportAll_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			await setControlEnabled(sender, false);
			await saveRecords(bookRecordEntries.Select(r => r.Record));
			await setControlEnabled(sender, true);
		}

		public void CheckAll()
		{
			foreach (var record in bookRecordEntries)
				record.IsChecked = true;
		}
		public void UncheckAll()
		{
			foreach (var record in bookRecordEntries)
				record.IsChecked = false;
		}
		public async void DeleteChecked_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var records = bookRecordEntries.Where(r => r.IsChecked).Select(r => r.Record).ToList();

			if (!records.Any()) return;

			await setControlEnabled(sender, false);

			bool success = false;
			try
			{
				var api = await libraryBook.GetApiAsync();
				success = await api.DeleteRecordsAsync(libraryBook.Book.AudibleProductId, records);
				records = await api.GetRecordsAsync(libraryBook.Book.AudibleProductId);

				var removed = bookRecordEntries.ExceptBy(records, r => r.Record).ToList();

				foreach (var r in removed)
					bookRecordEntries.Remove(r);
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, ex.Message);
			}
			finally { await setControlEnabled(sender, true); }

			if (!success)
				await MessageBox.Show(this, $"Libation was unable to delete the {records.Count} selected records", "Deletion Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}

		public async void ReloadAll_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			await setControlEnabled(sender, false);
			try
			{
				var api = await libraryBook.GetApiAsync();
				var records = await api.GetRecordsAsync(libraryBook.Book.AudibleProductId);

				bookRecordEntries.Clear();
				bookRecordEntries.AddRange(records.Select(r => new BookRecordEntry(r)));
			}
			catch (Exception ex)
			{
				Serilog.Log.Error(ex, ex.Message);
				await MessageBox.Show(this, $"Libation was unable to reload records", "Reload Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally { await setControlEnabled(sender, true); }
		}

		#endregion

		private async Task saveRecords(IEnumerable<IRecord> records)
		{
			if (!records.Any()) return;

			try
			{
				var saveFileDialog =
				await Dispatcher.UIThread.InvokeAsync(() => new FilePickerSaveOptions
				{
					Title = "Where to export book records",
					SuggestedFileName = $"{libraryBook.Book.Title} - Records",
					DefaultExtension = "xlsx",
					ShowOverwritePrompt = true,
					FileTypeChoices = new FilePickerFileType[]
					{
						new("Excel Workbook (*.xlsx)")
						{
							Patterns = new[] { "*.xlsx" },
							AppleUniformTypeIdentifiers = new[] { "org.openxmlformats.spreadsheetml.sheet" }
						},
						new("CSV files (*.csv)")
						{
							Patterns = new[] { "*.csv" },
							AppleUniformTypeIdentifiers = new[] { "public.comma-separated-values-text" }
						},
						new("JSON files (*.json)")
						{
							Patterns = new[] { "*.json" },
							AppleUniformTypeIdentifiers  = new[] { "public.json" }
						},
						new("All files (*.*)") { Patterns = new[] { "*" } },
					}
				});

				var selectedFile = (await StorageProvider.SaveFilePickerAsync(saveFileDialog))?.TryGetLocalPath();

				if (selectedFile is null) return;

				var ext = System.IO.Path.GetExtension(selectedFile).ToLowerInvariant();

				switch (ext)
				{
					case ".xlsx":
					default:
						await Task.Run(() => RecordExporter.ToXlsx(selectedFile, records));
						break;
					case ".csv":
						await Task.Run(() => RecordExporter.ToCsv(selectedFile, records));
						break;
					case ".json":
						await Task.Run(() => RecordExporter.ToJson(selectedFile, libraryBook, records));
						break;
				}
			}
			catch (Exception ex)
			{
				await MessageBox.ShowAdminAlert(this, "Error attempting to export your library.", "Error exporting", ex);
			}
		}

		#region DataGrid Bindings

		private class BookRecordEntry : ViewModels.ViewModelBase
		{
			private const string DateFormat = "yyyy-MM-dd HH\\:mm";
			private bool _ischecked;
			public IRecord Record { get; }
			public bool IsChecked { get => _ischecked; set => this.RaiseAndSetIfChanged(ref _ischecked, value); }
			public string Type => Record.GetType().Name;
			public string Start => formatTimeSpan(Record.Start);
			public string Created => Record.Created.ToString(DateFormat);
			public string Modified => Record is IAnnotation annotation ? annotation.Created.ToString(DateFormat) : string.Empty;
			public string End => Record is IRangeAnnotation range ? formatTimeSpan(range.End) : string.Empty;
			public string Note => Record is IRangeAnnotation range ? range.Text : string.Empty;
			public string Title => Record is Clip range ? range.Title : string.Empty;
			public BookRecordEntry(IRecord record) => Record = record;

			private static string formatTimeSpan(TimeSpan timeSpan)
			{
				int h = (int)timeSpan.TotalHours;
				int m = timeSpan.Minutes;
				int s = timeSpan.Seconds;
				int ms = timeSpan.Milliseconds;

				return ms == 0 ? $"{h:d2}:{m:d2}:{s:d2}" : $"{h:d2}:{m:d2}:{s:d2}.{ms:d3}";
			}
		}

		#endregion
	}
}
