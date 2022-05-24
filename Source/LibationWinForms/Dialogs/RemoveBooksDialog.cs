using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using ApplicationServices;
using AudibleUtilities;
using DataLayer;
using Dinah.Core.DataBinding;
using LibationFileManager;
using LibationWinForms.Login;

namespace LibationWinForms.Dialogs
{
	public partial class RemoveBooksDialog : Form
	{
		private Account[] _accounts { get; }
		private List<LibraryBook> _libraryBooks { get; }
		private SortableBindingList<RemovableGridEntry> _removableGridEntries { get; }
		private string _labelFormat { get; }
		private int SelectedCount => SelectedEntries?.Count() ?? 0;
		private IEnumerable<RemovableGridEntry> SelectedEntries => _removableGridEntries?.Where(b => b.Remove);

		public RemoveBooksDialog(params Account[] accounts)
		{
			_libraryBooks = DbContexts.GetLibrary_Flat_NoTracking();
			_accounts = accounts;

			InitializeComponent();

			this.Load += (_, _) => this.RestoreSizeAndLocation(Configuration.Instance);
			this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);

			_labelFormat = label1.Text;

			_dataGridView.CellContentClick += (_, _) => _dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
			_dataGridView.CellValueChanged += (_, _) => UpdateSelection();
			_dataGridView.BindingContextChanged += _dataGridView_BindingContextChanged;

			var orderedGridEntries = _libraryBooks
				.Select(lb => new RemovableGridEntry(lb))
				.OrderByDescending(ge => (DateTime)ge.GetMemberValue(nameof(ge.PurchaseDate)))
				.ToList();

			_removableGridEntries = new SortableBindingList<RemovableGridEntry>(orderedGridEntries);
			gridEntryBindingSource.DataSource = _removableGridEntries;

			_dataGridView.Enabled = false;
			this.SetLibationIcon();
		}

		private void _dataGridView_BindingContextChanged(object sender, EventArgs e)
		{
			_dataGridView.Sort(_dataGridView.Columns[0], ListSortDirection.Descending);
			UpdateSelection();
		}

		private async void RemoveBooksDialog_Shown(object sender, EventArgs e)
		{
			if (_accounts is null || _accounts.Length == 0)
				return;
			try
			{
				var removedBooks = await LibraryCommands.FindInactiveBooks(WinformLoginChoiceEager.ApiExtendedFunc, _libraryBooks, _accounts);

				var removable = _removableGridEntries.Where(rge => removedBooks.Any(rb => rb.Book.AudibleProductId == rge.AudibleProductId)).ToList();

				if (!removable.Any())
					return;

				foreach (var r in removable)
					r.Remove = true;

				UpdateSelection();
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					"Error scanning library. You may still manually select books to remove from Libation's library.",
					"Error scanning library",
					ex);
			}
			finally
			{
				_dataGridView.Enabled = true;
			}
		}

		private async void btnRemoveBooks_Click(object sender, EventArgs e)
		{
			var selectedBooks = SelectedEntries.ToList();

			if (selectedBooks.Count == 0)
				return;

			var libraryBooks = selectedBooks.Select(rge => rge.LibraryBook).ToList();
			var result = MessageBoxLib.ShowConfirmationDialog(
				libraryBooks,
				$"Are you sure you want to remove {0} from Libation's library?",
				"Remove books from Libation?");

			if (result != DialogResult.Yes)
				return;

			var idsToRemove = libraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			var removeLibraryBooks = await LibraryCommands.RemoveBooksAsync(idsToRemove);

			foreach (var rEntry in selectedBooks)
				_removableGridEntries.Remove(rEntry);

			UpdateSelection();
		}

		private void UpdateSelection()
		{
			var selectedCount = SelectedCount;
			label1.Text = string.Format(_labelFormat, selectedCount, selectedCount != 1 ? "s" : string.Empty);
			btnRemoveBooks.Enabled = selectedCount > 0;
		}
	}

	internal class RemovableGridEntry : GridView.LibraryBookEntry
	{
		private bool _remove = false;
		public RemovableGridEntry(LibraryBook libraryBook) : base(libraryBook) { }

		public bool Remove
		{
			get
			{
				return _remove;
			}
			set
			{
				_remove = value;
				NotifyPropertyChanged();
			}
		}

		public override object GetMemberValue(string memberName)
		{
			if (memberName == nameof(Remove))
				return Remove;
			return base.GetMemberValue(memberName);
		}
	}
}
