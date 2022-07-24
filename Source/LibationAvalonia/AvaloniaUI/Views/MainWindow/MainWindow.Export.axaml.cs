using ApplicationServices;
using Avalonia.Controls;
using System;
using System.Linq;

namespace LibationAvalonia.AvaloniaUI.Views
{
	//DONE
	public partial class MainWindow
	{
		private void Configure_Export() { }

		public void exportLibraryToolStripMenuItem_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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
			

				// FilterIndex is 1-based, NOT 0-based
				/*
				switch (saveFileDialog.FilterIndex)
				{
					case 1: // xlsx
					default:
						LibraryExporter.ToXlsx(saveFileDialog.FileName);
						break;
					case 2: // csv
						LibraryExporter.ToCsv(saveFileDialog.FileName);
						break;
					case 3: // json
						LibraryExporter.ToJson(saveFileDialog.FileName);
						break;
				}

				MessageBox.Show("Library exported to:\r\n" + saveFileDialog.FileName);
				*/
			}
			catch (Exception ex)
			{
				MessageBox.ShowAdminAlert(this, "Error attempting to export your library.", "Error exporting", ex);
			}
		}
	}
}
