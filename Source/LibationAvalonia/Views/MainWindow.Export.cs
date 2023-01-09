using ApplicationServices;
using Avalonia.Platform.Storage;
using LibationFileManager;
using System;
using System.Linq;

namespace LibationAvalonia.Views
{
	public partial class MainWindow
	{
		private void Configure_Export() { }

		public async void exportLibraryToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			try
			{
				var options = new FilePickerSaveOptions
				{
					Title = "Where to export Library",
					SuggestedStartLocation = new Avalonia.Platform.Storage.FileIO.BclStorageFolder(Configuration.Instance.Books),
					SuggestedFileName = $"Libation Library Export {DateTime.Now:yyyy-MM-dd}.xlsx",
					DefaultExtension = "xlsx",
					ShowOverwritePrompt = true,
					FileTypeChoices = new FilePickerFileType[]
					{
						new("Excel Workbook (*.xlsx)") { Patterns = new[] { "xlsx" } },
						new("CSV files (*.csv)") { Patterns = new[] { "csv" } },
						new("JSON files (*.json)") { Patterns = new[] { "json" } },
						new("All files (*.*)") { Patterns = new[] { "*" } },
					}
				};

				var selectedFile = await StorageProvider.SaveFilePickerAsync(options);

				if (selectedFile?.TryGetUri(out var uri) is not true) return;

				var ext = System.IO.Path.GetExtension(uri.LocalPath);
				switch (ext)
				{
					case "xlsx": // xlsx
					default:
						LibraryExporter.ToXlsx(uri.LocalPath);
						break;
					case "csv": // csv
						LibraryExporter.ToCsv(uri.LocalPath);
						break;
					case "json": // json
						LibraryExporter.ToJson(uri.LocalPath);
						break;
				}

				await MessageBox.Show("Library exported to:\r\n" + uri.LocalPath, "Library Exported");
			}
			catch (Exception ex)
			{
				await MessageBox.ShowAdminAlert(this, "Error attempting to export your library.", "Error exporting", ex);
			}
		}
	}
}
