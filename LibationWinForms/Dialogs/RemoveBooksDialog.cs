using ApplicationServices;
using DataLayer;
using InternalUtilities;
using LibationWinForms.Login;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Dinah.Core.DataBinding;
using System.Runtime.CompilerServices;
using Dinah.Core.Drawing;
using System.Collections;

namespace LibationWinForms.Dialogs
{
    public partial class RemoveBooksDialog : Form
	{
		public bool BooksRemoved { get; private set; }

		private Account[] _accounts { get; }
		private List<LibraryBook> _libraryBooks;
		private SortableBindingList<RemovableGridEntry> _removableGridEntries;
		private string _labelFormat;
		private int SelectedCount => SelectedEntries?.Count() ?? 0;
		private IEnumerable<RemovableGridEntry> SelectedEntries => _removableGridEntries?.Where(b => b.Remove);

        public RemoveBooksDialog(params Account[] accounts)
		{
			_libraryBooks = DbContexts.GetContext().GetLibrary_Flat_NoTracking();
			_accounts = accounts;
			InitializeComponent();
			_labelFormat = label1.Text;

			dataGridView1.CellContentClick += (s, e) => dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.BindingContextChanged += (s, e) => UpdateSelection();

			var orderedGridEntries = _libraryBooks
				.Select(lb => new RemovableGridEntry(new GridEntry(lb)))
				.OrderByDescending(ge => ge.GridEntry.PurchaseDate)
				.ToList();

			_removableGridEntries = orderedGridEntries.ToSortableBindingList();
			gridEntryBindingSource.DataSource = _removableGridEntries;

			dataGridView1.Enabled = false;
		}

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (e.ColumnIndex == 0)
			{
				UpdateSelection();
			}
		}

		private async void RemoveBooksDialog_Shown(object sender, EventArgs e)
		{
			if (_accounts == null || _accounts.Length == 0)
				return;
			try
			{
				var rmovedBooks = await LibraryCommands.FindInactiveBooks((account) => new WinformResponder(account), _libraryBooks, _accounts);

				var removable = _removableGridEntries.Where(rge => rmovedBooks.Count(rb => rb.Book.AudibleProductId == rge.GridEntry.AudibleProductId) == 1);

				if (removable.Count() == 0)
					return;

				foreach (var r in removable)
					r.Remove = true;

				UpdateSelection();
			}
			catch (Exception ex)
			{
				MessageBoxAlertAdmin.Show(
					"Error scanning library. You may still manually select books to remove from Libation's library.",
					"Error scanning library",
					ex);
			}
			finally
            {
				dataGridView1.Enabled = true;
            }
		}

		private void btnRemoveBooks_Click(object sender, EventArgs e)
		{
			var selected = SelectedEntries.ToList();

			if (selected.Count == 0) return;

			string titles = string.Join("\r\n", selected.Select(rge => "-" + rge.Title));

			var result = MessageBox.Show(
				this,
				$"Are you sure you want to remove the following {selected.Count} books from Libation's library?\r\n\r\n{titles}",
				"Remove books from Libation?",
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				MessageBoxDefaultButton.Button1);

			if (result == DialogResult.Yes)
            {
				using var context = DbContexts.GetContext();

				var libBooks = context.GetLibrary_Flat_NoTracking();

				var removeLibraryBooks = libBooks.Where(lb => selected.Count(rge => rge.GridEntry.AudibleProductId == lb.Book.AudibleProductId) == 1).ToArray();
				context.Library.RemoveRange(removeLibraryBooks);
				context.SaveChanges();
				BooksRemoved = true;

				foreach (var rEntry in selected)
					_removableGridEntries.Remove(rEntry);

				UpdateSelection();
			}
		}
		private void UpdateSelection()
        {
			dataGridView1.Sort(dataGridView1.Columns[0], ListSortDirection.Descending);
			var selectedCount = SelectedCount;
			label1.Text = string.Format(_labelFormat, selectedCount);
			btnRemoveBooks.Enabled = selectedCount > 0;
		}	
    }
    class CompareBool : IComparer
    {
        public int Compare(object x, object y)
        {
			var rge1 = x as RemovableGridEntry;
			var rge2 = y as RemovableGridEntry;

			return rge1.Remove.CompareTo(rge2.Remove);
        }
    }


    internal class RemovableGridEntry :  INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public GridEntry GridEntry { get; }

		public bool Remove
        {
            get
            {
				return _remove;
            }
            set
            {
				if (_remove != value)
				{
					_remove = value;
					NotifyPropertyChanged();
				}
			}
		}
		public Image Cover
		{
			get
			{
				return _cover;
			}
			set
			{
				_cover = value;
				NotifyPropertyChanged();
			}
		}
		public string Title =>  GridEntry.Title;
		public string Authors => GridEntry.Authors;
		public string Misc =>  GridEntry.Misc;
		public string DatePurchased => GridEntry.PurchaseDate;

		private bool _remove = false;
		private Image _cover;

		public RemovableGridEntry(GridEntry gridEntry)
		{
			GridEntry = gridEntry;

			var picDef = new FileManager.PictureDefinition(GridEntry.LibraryBook.Book.PictureId, FileManager.PictureSize._80x80);
			(bool isDefault, byte[] picture) = FileManager.PictureStorage.GetPicture(picDef);

			if (isDefault)
                FileManager.PictureStorage.PictureCached += PictureStorage_PictureCached;

			_cover = ImageReader.ToImage(picture);
		}

        private void PictureStorage_PictureCached(object sender, string pictureId)
        {
			if (pictureId == GridEntry.LibraryBook.Book.PictureId)
			{
				Cover = WindowsDesktopUtilities.WinAudibleImageServer.GetImage(pictureId, FileManager.PictureSize._80x80);
				FileManager.PictureStorage.PictureCached -= PictureStorage_PictureCached;
			}
		}

		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => 
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    }
}
