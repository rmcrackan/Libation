using System;
using System.Windows.Forms;
using ApplicationServices;

#nullable enable
namespace LibationWinForms
{
    public partial class Form1
    {
        private void Configure_Export() { }

		private void exportLibraryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				var saveFileDialog = new SaveFileDialog
				{
					Title = "Where to export Library",
					Filter = "Excel Workbook (*.xlsx)|*.xlsx|CSV files (*.csv)|*.csv|JSON files (*.json)|*.json" // + "|All files (*.*)|*.*"
				};

				if (saveFileDialog.ShowDialog() != DialogResult.OK)
					return;

				// FilterIndex is 1-based, NOT 0-based
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
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(this, "Error attempting to export your library.", "Error exporting", ex);
			}
		}
	}
}
