using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase
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
	public class TrackedQueue<T> where T : class
	{
		public event EventHandler<int> CompletedCountChanged;
		public event EventHandler<int> QueuededCountChanged;

		public T Current { get; private set; }

		public IReadOnlyList<T> Queued => _queued;
		public IReadOnlyList<T> Completed => _completed;

		private readonly List<T> _queued = new();
		private readonly List<T> _completed = new();
		private readonly object lockObject = new();

		private readonly ICollection<T> _underlyingList;

		public TrackedQueue(ICollection<T> underlyingList = null)
		{
			_underlyingList = underlyingList;
		}

		public T this[int index]
		{
			get
			{
				lock (lockObject)
				{
					if (index < _completed.Count)
						return _completed[index];
					index -= _completed.Count;

					if (index == 0 && Current != null) return Current;

					if (Current != null) index--;

					if (index < _queued.Count) return _queued.ElementAt(index);

					throw new IndexOutOfRangeException();
				}
			}
		}

		public int Count
		{
			get
			{
				lock (lockObject)
				{
					return _queued.Count + _completed.Count + (Current == null ? 0 : 1);
				}
			}
		}

		public int IndexOf(T item)
		{
			lock (lockObject)
			{
				if (_completed.Contains(item))
					return _completed.IndexOf(item);

				if (Current == item) return _completed.Count;

				if (_queued.Contains(item))
					return _queued.IndexOf(item) + (Current is null ? 0 : 1);
				return -1;
			}
		}

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

		public void Enqueue(IEnumerable<T> item)
		{
			int queueCount;
			lock (lockObject)
			{
				_queued.AddRange(item);
				queueCount = _queued.Count;
			}
			foreach (var i in item)
				_underlyingList?.Add(i);
			QueuededCountChanged?.Invoke(this, queueCount);
		}

		private void RebuildSecondary()
		{
			_underlyingList?.Clear();
			foreach (var item in GetAllItems())
				_underlyingList?.Add(item);
		}

		public IEnumerable<T> GetAllItems()
		{
			if (Current is null) return Completed.Concat(Queued);
			return Completed.Concat(new List<T> { Current }).Concat(Queued);
		}
	}
}
