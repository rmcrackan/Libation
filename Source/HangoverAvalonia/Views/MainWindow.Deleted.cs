using ApplicationServices;
using DataLayer;
using HangoverAvalonia.Controls;
using System;
using System.Linq;

namespace HangoverAvalonia.Views
{
	public partial class MainWindow
	{
		private void deletedTab_VisibleChanged(bool isVisible)
		{
			if (!isVisible)
				return;

			if (_viewModel.DeletedBooks.Count == 0)
				_viewModel.reload();
		}
		public void Deleted_CheckedListBox_ItemCheck(object sender, ItemCheckEventArgs args)
		{
			_viewModel.CheckedBooksCount = deletedCbl.CheckedItems.Count();
		}
		public void Deleted_CheckAll_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			foreach (var item in deletedCbl.Items)
				deletedCbl.SetItemChecked(item, true);
		}
		public void Deleted_UncheckAll_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			foreach (var item in deletedCbl.Items)
				deletedCbl.SetItemChecked(item, false);
		}
		public void Deleted_Save_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var libraryBooksToRestore = deletedCbl.CheckedItems.Cast<LibraryBook>().ToList();
			var qtyChanges = libraryBooksToRestore.RestoreBooks();
			if (qtyChanges > 0)
				_viewModel.reload();
		}
	}
}
