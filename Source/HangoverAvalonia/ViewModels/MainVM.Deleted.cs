using ApplicationServices;
using DataLayer;
using ReactiveUI;
using System.Collections.Generic;

namespace HangoverAvalonia.ViewModels
{
	public partial class MainVM
	{
		private List<LibraryBook> _deletedBooks;
		public List<LibraryBook> DeletedBooks { get => _deletedBooks; set => this.RaiseAndSetIfChanged(ref _deletedBooks, value); }
		public string CheckedCountText => $"Checked : {_checkedBooksCount} of {_totalBooksCount}";

		private int _totalBooksCount = 0;
		private int _checkedBooksCount = 0;
		public int CheckedBooksCount
		{
			get => _checkedBooksCount;
			set
			{
				if (_checkedBooksCount != value)
				{
					_checkedBooksCount = value;
					this.RaisePropertyChanged(nameof(CheckedCountText));
				}
			}
		}
		private void Load_deletedVM()
		{
			reload();
		}

		public void reload()
		{
			DeletedBooks = DbContexts.GetContext().GetDeletedLibraryBooks();
			_checkedBooksCount = 0;
			_totalBooksCount = DeletedBooks.Count;
			this.RaisePropertyChanged(nameof(CheckedCountText));
		}
	}
}
