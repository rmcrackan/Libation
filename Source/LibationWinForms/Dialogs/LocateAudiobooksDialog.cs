using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class LocateAudiobooksDialog : Form
	{
		private readonly CancellationTokenSource tokenSource = new();
		private readonly LocatedAudiobooksViewModel _viewModel;
		public LocateAudiobooksDialog()
		{
			InitializeComponent();

			this.SetLibationIcon();
			this.RestoreSizeAndLocation(Configuration.Instance);

			_viewModel = new LocatedAudiobooksViewModel(new SortBindingList<FoundAudiobook>());
			dataGridView1.EnableHeadersVisualStyles = !Application.IsDarkModeEnabled;
			dataGridView1.RowsAdded += DataGridView1_RowsAdded;
			foundAudiobookBindingSource.DataSource = _viewModel.FoundFiles;
			booksFoundLbl.DataBindings.Add(new Binding(nameof(booksFoundLbl.Text), _viewModel, nameof(_viewModel.FoundAsinCount), true, DataSourceUpdateMode.OnPropertyChanged, 0, booksFoundLbl.Text));
		}

		private void DataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			dataGridView1.FirstDisplayedScrollingRowIndex = e.RowIndex;
		}

		private void LocateAudiobooks_FormClosing(object sender, FormClosingEventArgs e)
		{
			tokenSource.Cancel();
			this.SaveSizeAndLocation(Configuration.Instance);
		}

		private async void LocateAudiobooks_Shown(object sender, EventArgs e)
		{
			var fbd = new FolderBrowserDialog
			{
				Description = "Select the folder to search for audiobooks",
				UseDescriptionForTitle = true,
				InitialDirectory = Configuration.Instance.Books
			};

			var result = fbd.ShowDialog(this);
			if (result != DialogResult.OK || !Directory.Exists(fbd.SelectedPath))
			{
				DialogResult = result;
			}
			else
			{
				await _viewModel.FindAndAddBooksAsync(fbd.SelectedPath, tokenSource.Token);
				MessageBox.Show(this, $"Libation has found {_viewModel.FoundAsinCount} unique audiobooks and added them to its database. ", $"Found {_viewModel.FoundAsinCount} Audiobooks");
			}
		}

		private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.RowIndex < _viewModel.FoundFiles.Count)
				Go.To.File(_viewModel.FoundFiles[e.RowIndex].Entry.Path);
		}
	}
}
