using ApplicationServices;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FileManager;
using LibationFileManager;
using System;
using System.Linq;

namespace LibationAvalonia.Views
{
	//DONE
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
					SuggestedStartLocation = new Avalonia.Platform.Storage.FileIO.BclStorageFolder(Configuration.Instance.Books.PathWithoutPrefix),
					SuggestedFileName = $"Libation Library Export {DateTime.Now:yyyy-MM-dd}",
					DefaultExtension = "xlsx",
					ShowOverwritePrompt = true,
					FileTypeChoices = new FilePickerFileType[]
					{
						new("Excel Workbook (*.xlsx)")
						{
							Patterns = new[] { "*.xlsx" },
							//https://gist.github.com/RhetTbull/7221ef3cfd9d746f34b2550d4419a8c2
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
							AppleUniformTypeIdentifiers = new[] { "public.json" }
						},
						new("All files (*.*)") { Patterns = new[] { "*" } }
					}
				};

				var selectedFile = await StorageProvider.SaveFilePickerAsync(options);

				if (selectedFile?.TryGetUri(out var uri) is not true) return;

				var ext = FileUtility.GetStandardizedExtension(System.IO.Path.GetExtension(uri.LocalPath));
				switch (ext)
				{
					case ".xlsx": // xlsx
					default:
						LibraryExporter.ToXlsx(uri.LocalPath);
						break;
					case ".csv": // csv
						LibraryExporter.ToCsv(uri.LocalPath);
						break;
					case ".json": // json
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
