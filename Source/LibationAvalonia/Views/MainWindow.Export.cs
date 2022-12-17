using ApplicationServices;
using Avalonia.Controls;
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
				var saveFileDialog = new SaveFileDialog
				{
					Title = "Where to export Library",
				};
				saveFileDialog.Filters.Add(new FileDialogFilter { Name = "Excel Workbook (*.xlsx)", Extensions = new() { "xlsx" } });
				saveFileDialog.Filters.Add(new FileDialogFilter { Name = "CSV files (*.csv)", Extensions = new() { "csv" } });
				saveFileDialog.Filters.Add(new FileDialogFilter { Name = "JSON files (*.json)", Extensions = new() { "json" } });
				saveFileDialog.Filters.Add(new FileDialogFilter { Name = "All files (*.*)", Extensions = new() { "*" } });

				var fileName = await saveFileDialog.ShowAsync(this);
				if (fileName is null) return;

				var ext = System.IO.Path.GetExtension(fileName);
				switch (ext)
				{
					case "xlsx": // xlsx
					default:
						LibraryExporter.ToXlsx(fileName);
						break;
					case "csv": // csv
						LibraryExporter.ToCsv(fileName);
						break;
					case "json": // json
						LibraryExporter.ToJson(fileName);
						break;
				}

				await MessageBox.Show("Library exported to:\r\n" + fileName, "Library Exported");
			}
			catch (Exception ex)
			{
				await MessageBox.ShowAdminAlert(this, "Error attempting to export your library.", "Error exporting", ex);
			}
		}
	}
}
