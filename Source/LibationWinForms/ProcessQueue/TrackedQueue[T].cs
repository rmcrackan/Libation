using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationWinForms.ProcessQueue
{
	public enum QueuePosition
	{
		Fisrt,
		OneUp,
		OneDown,
		Last
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
			lock (lockObject)
			{
				bool removed = _queued.Remove(item);
				if (removed)
					QueuededCountChanged?.Invoke(this, _queued.Count);
				return removed;
			}
		}

		public void ClearQueue()
		{
			lock (lockObject)
			{
				_queued.Clear();
				QueuededCountChanged?.Invoke(this, 0);
			}
		}

		public void ClearCompleted()
		{
			lock (lockObject)
			{
				_completed.Clear();
				CompletedCountChanged?.Invoke(this, 0);
			}
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
		}

		public bool MoveNext()
		{
			lock (lockObject)
			{
				if (Current != null)
				{
					_completed.Add(Current);
					CompletedCountChanged?.Invoke(this, _completed.Count);
				}
				if (_queued.Count == 0)
				{
					Current = null;
					return false;
				}
				Current = _queued[0];
				_queued.RemoveAt(0);

				QueuededCountChanged?.Invoke(this, _queued.Count);
				return true;
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

		public void Enqueue(T item)
		{
			lock (lockObject)
			{
				_queued.Add(item);
				QueuededCountChanged?.Invoke(this, _queued.Count);
			}
		}
	}
}
