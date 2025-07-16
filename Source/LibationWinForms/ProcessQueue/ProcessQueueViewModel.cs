using DataLayer;
using LibationUiBase.ProcessQueue;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

#nullable enable
namespace LibationWinForms.ProcessQueue;

internal class ProcessQueueViewModel : ProcessQueueViewModelBase
{
	public event EventHandler<string>? LogWritten;
	/// <summary>
	/// Fires when a ProcessBookViewModelBase in the queue has a property changed
	/// </summary>
	public event EventHandler<PropertyChangedEventArgs>? BookPropertyChanged;
	private ObservableCollection<ProcessBookViewModelBase> Items { get; }

	public ProcessQueueViewModel() : base(CreateEmptyList())
	{
		Items = Queue.UnderlyingList as ObservableCollection<ProcessBookViewModelBase>
			?? throw new ArgumentNullException(nameof(Queue.UnderlyingList));
		Items.CollectionChanged += Items_CollectionChanged;
	}

	private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		switch  (e.Action)
		{
			case NotifyCollectionChangedAction.Add:
				subscribe(e.NewItems);
				break;
			case NotifyCollectionChangedAction.Remove:
				unubscribe(e.OldItems);
				break;
		}

		void subscribe(IList? items)
		{
			foreach (var item in e.NewItems?.OfType<ProcessBookViewModel>() ?? [])
				item.PropertyChanged += Item_PropertyChanged;
		}

		void unubscribe(IList? items)
		{
			foreach (var item in e.NewItems?.OfType<ProcessBookViewModel>() ?? [])
				item.PropertyChanged -= Item_PropertyChanged;
		}
	}

	private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		=> BookPropertyChanged?.Invoke(sender, e);

	public override void WriteLine(string text) => Invoke(() => LogWritten?.Invoke(this, text.Trim()));

	protected override ProcessBookViewModelBase CreateNewBook(LibraryBook libraryBook)
		=> new ProcessBookViewModel(libraryBook, Logger);

	private static ObservableCollection<ProcessBookViewModelBase> CreateEmptyList()
		=> new ProcessBookCollection();

	private class ProcessBookCollection : ObservableCollection<ProcessBookViewModelBase>
	{
		protected override void ClearItems()
		{
			//ObservableCollection doesn't raise Remove for each item on Clear, so we need to do it ourselves.
			OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this));
			base.ClearItems();
		}
	}
}
