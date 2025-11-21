using ApplicationServices;
using Avalonia.Collections;
using DataLayer;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HangoverAvalonia.ViewModels;

public class TrashBinViewModel : ViewModelBase, IDisposable
{
	public AvaloniaList<CheckBoxViewModel> DeletedBooks { get; }
	public string CheckedCountText => $"Checked : {_checkedBooksCount} of {_totalBooksCount}";

	private bool _controlsEnabled = true;
	public bool ControlsEnabled { get => _controlsEnabled; set => this.RaiseAndSetIfChanged(ref _controlsEnabled, value); }

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
			_checkedBooksCount = value;
			this.RaisePropertyChanged(nameof(CheckedCountText));

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

	public void Reload()
	{
		var deletedBooks = DbContexts.GetDeletedLibraryBooks();

		DeletedBooks.Clear();
		DeletedBooks.AddRange(deletedBooks.Select(lb => new CheckBoxViewModel { Item = lb }));

		_totalBooksCount = DeletedBooks.Count;
		CheckedBooksCount = 0;
	}

	private IDisposable tracker;
	private void CheckboxPropertyChanged(Tuple<object, PropertyChangedEventArgs> e)
	{
		if (e.Item2.PropertyName == nameof(CheckBoxViewModel.IsChecked))
			CheckedBooksCount = DeletedBooks.Count(b => b.IsChecked);
	}

	public void Dispose() => tracker?.Dispose();
}
