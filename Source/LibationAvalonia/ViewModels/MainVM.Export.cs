using ApplicationServices;
using Avalonia.Platform.Storage;
using FileManager;
using LibationFileManager;
using System;
using System.Threading.Tasks;
using LibationUiBase.Resources;

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
				Title = Strings.MainVM_WhereToExportLibrary,
				SuggestedStartLocation = startFolder is null ? null : await MainWindow.StorageProvider.TryGetFolderFromPathAsync(startFolder),
				SuggestedFileName = string.Format(Strings.MainVM_LibationLibraryExport, DateTime.Now),
				DefaultExtension = "xlsx",
				ShowOverwritePrompt = true,
				FileTypeChoices = new FilePickerFileType[]
				{
					new(Strings.MainVM_ExcelWorkbookXlsx)
					{
						Patterns = new[] { "*.xlsx" },
						//https://gist.github.com/RhetTbull/7221ef3cfd9d746f34b2550d4419a8c2
						AppleUniformTypeIdentifiers = new[] { "org.openxmlformats.spreadsheetml.sheet" }
					},
					new(Strings.MainVM_CSVFilesCsv)
					{
						Patterns = new[] { "*.csv" },
						AppleUniformTypeIdentifiers = new[] { "public.comma-separated-values-text" }
					},
					new(Strings.MainVM_JSONFilesJson)
					{
						Patterns = new[] { "*.json" },
						AppleUniformTypeIdentifiers = new[] { "public.json" }
					},
					new(Strings.MainVM_AllFiles) { Patterns = new[] { "*" } }
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

			await MessageBox.Show(string.Format(Strings.MainVM_LibraryExportedTo, selectedFile), Strings.MainVM_LibraryExported);
		}
		catch (Exception ex)
		{
			await MessageBox.ShowAdminAlert(MainWindow, Strings.MainVM_ErrorAttemptingToExportYourLibrary, Strings.MainVM_ErrorExporting, ex);
		}
	}
}
