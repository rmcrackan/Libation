using ApplicationServices;
using AudibleUtilities;
using DataLayer;
using LibationWinForms.AvaloniaUI.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI.Views.ProductsGrid
{
	public partial class ProductsDisplay2
	{
		private void Configure_ScanAndRemove() { }

		private bool RemoveColumnVisible
		{
			get => removeGVColumn.IsVisible;
			set
			{
				if (value)
				{
					foreach (var book in bindingList.AllItems())
						book.Remove = false;
				}

				removeGVColumn.DisplayIndex = 0;
				removeGVColumn.CanUserReorder = value;
				removeGVColumn.IsVisible = value;
			}
		}

		public void CloseRemoveBooksColumn()
		{
			RemoveColumnVisible = false;

			foreach (var item in bindingList.AllItems())
				item.PropertyChanged -= Item_PropertyChanged;
		}

		public async Task RemoveCheckedBooksAsync()
		{
			var selectedBooks = GetAllBookEntries().Where(lbe => lbe.Remove == true).ToList();

			if (selectedBooks.Count == 0)
				return;

			var libraryBooks = selectedBooks.Select(rge => rge.LibraryBook).ToList();
			var result = MessageBoxLib.ShowConfirmationDialog(
				libraryBooks,
				$"Are you sure you want to remove {selectedBooks.Count} books from Libation's library?",
				"Remove books from Libation?");

			if (result != System.Windows.Forms.DialogResult.Yes)
				return;

			foreach (var book in selectedBooks)
				book.PropertyChanged -= Item_PropertyChanged;

			var idsToRemove = libraryBooks.Select(lb => lb.Book.AudibleProductId).ToList();
			bindingList.CollectionChanged += BindingList_CollectionChanged;

			//The RemoveBooksAsync will fire LibrarySizeChanged, which calls ProductsDisplay2.Display(),
			//so there's no need to remove books from the grid display here.
			var removeLibraryBooks = await LibraryCommands.RemoveBooksAsync(idsToRemove);

			foreach (var b in GetAllBookEntries())
				b.Remove = false;

			RemovableCountChanged?.Invoke(this, 0);
		}

		void BindingList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			if (e.Action != System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
				return;

			//After ProductsDisplay2.Display() re-creates the list,
			//re-subscribe to all items' PropertyChanged events.

			foreach (var b in GetAllBookEntries())
				b.PropertyChanged += Item_PropertyChanged;

			bindingList.CollectionChanged -= BindingList_CollectionChanged;
		}

		public async Task ScanAndRemoveBooksAsync(params Account[] accounts)
		{
			RemovableCountChanged?.Invoke(this, 0);
			removeGVColumn.IsVisible = true;

			try
			{
				if (accounts is null || accounts.Length == 0)
					return;

				var allBooks = GetAllBookEntries();

				foreach (var b in allBooks)
					b.Remove = false;

				var lib = allBooks
					.Select(lbe => lbe.LibraryBook)
					.Where(lb => !lb.Book.HasLiberated());

				var removedBooks = await LibraryCommands.FindInactiveBooks(Login.WinformLoginChoiceEager.ApiExtendedFunc, lib, accounts);

				var removable = allBooks.Where(lbe => removedBooks.Any(rb => rb.Book.AudibleProductId == lbe.AudibleProductId)).ToList();

				foreach (var r in removable)
					r.Remove = true;

				RemovableCountChanged?.Invoke(this, GetAllBookEntries().Count(lbe => lbe.Remove is true));

				foreach (var item in bindingList.AllItems())
					item.PropertyChanged += Item_PropertyChanged;
			}
			catch (Exception ex)
			{
				MessageBoxLib.ShowAdminAlert(
					null,
					"Error scanning library. You may still manually select books to remove from Libation's library.",
					"Error scanning library",
					ex);
			}
		}

		private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(GridEntry2.Remove) && sender is LibraryBookEntry2 lbEntry)
			{
				int removeCount = GetAllBookEntries().Count(lbe => lbe.Remove is true);
				RemovableCountChanged?.Invoke(this, removeCount);
			}
		}
	}
}
