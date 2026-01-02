using ApplicationServices;
using Avalonia.Collections;
using Avalonia.Controls;
using DataLayer;
using LibationAvalonia.Controls;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public partial class TrashBinDialog : DialogWindow
	{
		public TrashBinDialog()
		{
			InitializeComponent();
			DataContext = new TrashBinViewModel();

			this.Closing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);

			KeyBindings.Add(new Avalonia.Input.KeyBinding
			{
				Gesture = new Avalonia.Input.KeyGesture(Avalonia.Input.Key.Escape),
				Command = ReactiveCommand.Create(Close)
			});
		}
	}

	public class TrashBinViewModel : ViewModelBase, IDisposable
	{
		public AvaloniaList<CheckBoxViewModel> DeletedBooks { get; }
		public string CheckedCountText => $"Checked : {_checkedBooksCount} of {_totalBooksCount}";
		public bool ControlsEnabled { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = true;

		private bool? everythingChecked = false;
		public bool? EverythingChecked
		{
			get => everythingChecked;
			set
			{
				everythingChecked = value ?? false;

				if (everythingChecked is true)
					CheckAll();
				else if (everythingChecked is false)
					UncheckAll();
			}
		}

		private int _totalBooksCount = 0;
		private int _checkedBooksCount = -1;
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

				everythingChecked
					= _checkedBooksCount == 0 || _totalBooksCount == 0 ? false
					: _checkedBooksCount == _totalBooksCount ? true
					: null;

				this.RaisePropertyChanged(nameof(EverythingChecked));
			}
		}

		public IEnumerable<LibraryBook> CheckedBooks => DeletedBooks.Where(i => i.IsChecked).Select(i => i.Item).Cast<LibraryBook>();

		public TrashBinViewModel()
		{
			DeletedBooks = new()
			{
				ResetBehavior = ResetBehavior.Remove
			};

			tracker = DeletedBooks.TrackItemPropertyChanged(CheckboxPropertyChanged);
			Reload();
		}

		public void CheckAll()
		{
			foreach (var item in DeletedBooks)
				item.IsChecked = true;
		}

		public void UncheckAll()
		{
			foreach (var item in DeletedBooks)
				item.IsChecked = false;
		}

		public async Task RestoreCheckedAsync()
		{
			ControlsEnabled = false;
			var qtyChanges = await CheckedBooks.RestoreBooksAsync();
			if (qtyChanges > 0)
				Reload();
			ControlsEnabled = true;
		}

		public async Task PermanentlyDeleteCheckedAsync()
		{
			ControlsEnabled = false;
			var qtyChanges = await CheckedBooks.PermanentlyDeleteBooksAsync();
			if (qtyChanges > 0)
				Reload();
			ControlsEnabled = true;
		}

		private void Reload()
		{
			var deletedBooks = DbContexts.GetDeletedLibraryBooks();

			DeletedBooks.Clear();
			DeletedBooks.AddRange(deletedBooks.Select(lb => new CheckBoxViewModel { Item = lb }));

			_totalBooksCount = DeletedBooks.Count;
			CheckedBooksCount = 0;
		}

		private IDisposable tracker;
		private void CheckboxPropertyChanged(Tuple<object?, PropertyChangedEventArgs> e)
		{
			if (e.Item2.PropertyName == nameof(CheckBoxViewModel.IsChecked))
				CheckedBooksCount = DeletedBooks.Count(b => b.IsChecked);
		}

		public void Dispose() => tracker?.Dispose();
	}
}
