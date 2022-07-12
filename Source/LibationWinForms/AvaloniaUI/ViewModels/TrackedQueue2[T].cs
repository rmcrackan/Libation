using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.ViewModels
{
	public enum QueuePosition
	{
		Fisrt,
		OneUp,
		OneDown,
		Last,
	}

	/*
	 * This data structure is like lifting a metal chain one link at a time.
	 * Each time you grab and lift a new link (MoveNext call):
	 * 
	 *   1) you're holding a new link in your hand (Current)
	 *   2) the remaining chain to be lifted shortens by 1 link (Queued)
	 *   3) the pile of chain at your feet grows by 1 link (Completed)
	 *   
	 * The index is the link position from the first link you lifted to the
	 * last one in the chain.
	 * 
	 * 
	 * For this to work with Avalonia's ItemsRepeater, it must be an ObservableCollection
	 * (not merely a Collection with INotifyCollectionChanged, INotifyPropertyChanged). 
	 * So TrackedQueue maintains 2 copies of the list. The primary copy of the list is
	 * split into Completed, Current and Queued and is used by ProcessQueue to keep track
	 * of what's what. The secondary copy is a concatenation of primary's three sources
	 * and is stored in ObservableCollection.Items.  When the primary list changes, the
	 * secondary list is cleared and reset to match the primary. 
	 */
	public class TrackedQueue2<T> : ObservableCollection<T> where T : class
	{
		public event EventHandler<int> CompletedCountChanged;
		public event EventHandler<int> QueuededCountChanged;

		public T Current { get; private set; }

		public IReadOnlyList<T> Queued => _queued;
		public IReadOnlyList<T> Completed => _completed;

		private readonly List<T> _queued = new();
		private readonly List<T> _completed = new();
		private readonly object lockObject = new();

		public bool RemoveQueued(T item)
		{
			bool itemsRemoved;
			int queuedCount;

			lock (lockObject)
			{
				itemsRemoved = _queued.Remove(item);
				queuedCount = _queued.Count;
			}

			if (itemsRemoved)
			{
				QueuededCountChanged?.Invoke(this, queuedCount);
				RebuildSecondary();
			}
			return itemsRemoved;
		}

		public void ClearCurrent()
		{
			lock(lockObject)
				Current = null;
			RebuildSecondary();
		}
		
		public bool RemoveCompleted(T item)
		{
			bool itemsRemoved;
			int completedCount;

			lock (lockObject)
			{
				itemsRemoved = _completed.Remove(item);
				completedCount = _completed.Count;
			}

			if (itemsRemoved)
			{
				CompletedCountChanged?.Invoke(this, completedCount);
				RebuildSecondary();
			}
			return itemsRemoved;
		}

		public void ClearQueue()
		{
			lock (lockObject)
				_queued.Clear();
			QueuededCountChanged?.Invoke(this, 0);
			RebuildSecondary();
		}

		public void ClearCompleted()
		{
			lock (lockObject)
				_completed.Clear();
			CompletedCountChanged?.Invoke(this, 0);
			RebuildSecondary();
		}

		public bool Any(Func<T, bool> predicate)
		{
			lock (lockObject)
			{
				return (Current != null && predicate(Current)) || _completed.Any(predicate) || _queued.Any(predicate);
			}
		}

		public void MoveQueuePosition(T item, QueuePosition requestedPosition)
		{
			lock (lockObject)
			{
				if (_queued.Count == 0 || !_queued.Contains(item)) return;

				if ((requestedPosition == QueuePosition.Fisrt || requestedPosition == QueuePosition.OneUp) && _queued[0] == item)
					return;
				if ((requestedPosition == QueuePosition.Last || requestedPosition == QueuePosition.OneDown) && _queued[^1] == item)
					return;

				int queueIndex = _queued.IndexOf(item);

				if (requestedPosition == QueuePosition.OneUp)
				{
					_queued.RemoveAt(queueIndex);
					_queued.Insert(queueIndex - 1, item);
				}
				else if (requestedPosition == QueuePosition.OneDown)
				{
					_queued.RemoveAt(queueIndex);
					_queued.Insert(queueIndex + 1, item);
				}
				else if (requestedPosition == QueuePosition.Fisrt)
				{
					_queued.RemoveAt(queueIndex);
					_queued.Insert(0, item);
				}
				else
				{
					_queued.RemoveAt(queueIndex);
					_queued.Insert(_queued.Count, item);
				}
			}
			RebuildSecondary();
		}

		public bool MoveNext()
		{
			int completedCount = 0, queuedCount = 0;
			bool completedChanged = false;
			try
			{
				lock (lockObject)
				{
					if (Current != null)
					{
						_completed.Add(Current);
						completedCount = _completed.Count;
						completedChanged = true;
					}
					if (_queued.Count == 0)
					{
						Current = null;
						return false;
					}
					Current = _queued[0];
					_queued.RemoveAt(0);

					queuedCount = _queued.Count;
					return true;
				}
			}
			finally
			{
				if (completedChanged)
					CompletedCountChanged?.Invoke(this, completedCount);
				QueuededCountChanged?.Invoke(this, queuedCount);
				RebuildSecondary();
			}
		}

		public bool TryPeek(out T item)
		{
			lock (lockObject)
			{
				if (_queued.Count == 0)
				{
					item = null;
					return false;
				}
				item = _queued[0];
				return true;
			}
		}

		public T Peek()
		{
			lock (lockObject)
			{
				if (_queued.Count == 0) throw new InvalidOperationException("Queue empty");
				return _queued.Count > 0 ? _queued[0] : default;
			}
		}

		public void Enqueue(IEnumerable<T> item)
		{
			int queueCount;
			lock (lockObject)
			{
				_queued.AddRange(item);
				queueCount = _queued.Count;
			}
			foreach (var i in item)
				base.Add(i);
			QueuededCountChanged?.Invoke(this, queueCount);
		}

		private void RebuildSecondary()
		{
			base.ClearItems();
			foreach (var item in GetAllItems())
				base.Add(item);
		}

		public IEnumerable<T> GetAllItems()
		{
			if (Current is null) return Completed.Concat(Queued);
			return Completed.Concat(new List<T> { Current }).Concat(Queued);
		}
	}
}
