using ApplicationServices;
using DataLayer;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class LocateAudiobooksDialog : Form
	{
		private event EventHandler<FilePathCache.CacheEntry> FileFound;
		private readonly CancellationTokenSource tokenSource = new();
		private readonly List<string> foundAsins = new();
		private readonly string labelFormatText;
		public LocateAudiobooksDialog()
		{
			InitializeComponent();

			labelFormatText = booksFoundLbl.Text;
			setFoundBookCount(0);

			this.SetLibationIcon();
			this.RestoreSizeAndLocation(Configuration.Instance);

			Shown += LocateAudiobooks_Shown;
			FileFound += LocateAudiobooks_FileFound;
			FormClosing += LocateAudiobooks_FormClosing;
		}

		private void setFoundBookCount(int count)
			=> booksFoundLbl.Text = string.Format(labelFormatText, count);

		private void LocateAudiobooks_FileFound(object sender, FilePathCache.CacheEntry e)
		{
			foundAudiobooksLV.Items
				.Add(new ListViewItem(new string[] { $"[{e.Id}]", Path.GetFileName(e.Path) }))
				.EnsureVisible();

			foundAudiobooksLV.AutoResizeColumn(1, ColumnHeaderAutoResizeStyle.ColumnContent);

			if (!foundAsins.Any(asin => asin == e.Id))
			{
				foundAsins.Add(e.Id);
				setFoundBookCount(foundAsins.Count);
			}
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

			if (fbd.ShowDialog() != DialogResult.OK || !Directory.Exists(fbd.SelectedPath))
			{
				Close();
				return;
			}

			await foreach (var book in AudioFileStorage.FindAudiobooksAsync(fbd.SelectedPath, tokenSource.Token))
			{
				try
				{
					FilePathCache.Insert(book);

					var lb = DbContexts.GetLibraryBook_Flat_NoTracking(book.Id);
					if (lb.Book.UserDefinedItem.BookStatus is not LiberatedStatus.Liberated)
						await lb.UpdateBookStatusAsync(LiberatedStatus.Liberated);

					tokenSource.Token.ThrowIfCancellationRequested();
					this.Invoke(FileFound, this, book);
				}
				catch (OperationCanceledException) { }
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "Error adding found audiobook file to Libation. {@audioFile}", book);
				}
			}

			MessageBox.Show(this, $"Libation has found {foundAsins.Count} unique audiobooks and added them to its database. ", $"Found {foundAsins.Count} Audiobooks");
			Close();
		}
	}
}
