using ApplicationServices;
using Avalonia.Platform.Storage;
using FileManager;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	private void Configure_Export() { }

	public async Task ExportLibraryAsync()
	{
		try
		{
			var startFolder = Configuration.Instance.Books?.PathWithoutPrefix;
			var options = new FilePickerSaveOptions
			{
				Title = "Where to export Library",
				SuggestedStartLocation = startFolder is null ? null : await MainWindow.StorageProvider.TryGetFolderFromPathAsync(startFolder),
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

			var selectedFile = (await MainWindow.StorageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();

			if (selectedFile is null) return;

			var ext = FileUtility.GetStandardizedExtension(System.IO.Path.GetExtension(selectedFile));
			switch (ext)
			{
				case ".xlsx": // xlsx
				default:
					LibraryExporter.ToXlsx(selectedFile);
					break;
				case ".csv": // csv
					LibraryExporter.ToCsv(selectedFile);
					break;
				case ".json": // json
					LibraryExporter.ToJson(selectedFile);
					break;
			}

			await MessageBox.Show("Library exported to:\r\n" + selectedFile, "Library Exported");
		}
		catch (Exception ex)
		{
			await MessageBox.ShowAdminAlert(MainWindow, "Error attempting to export your library.", "Error exporting", ex);
		}
	}
}
