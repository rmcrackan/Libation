using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.ProcessQueue
{
	internal class TrackedQueue<T> where T : class
	{
		public T Current { get; private set; }
		private readonly LinkedList<T> Queued = new();
		private readonly List<T> Completed = new();
		private readonly object lockObject = new();

		public int Count => Queued.Count + Completed.Count + (Current == null ? 0 : 1);

		public T this[int index]
		{
			get
			{
				if (index < Completed.Count)
					return Completed[index];
				index -= Completed.Count;

				if (index == 0&& Current != null) return Current;

				if (Current != null) index--;

				if (index < Queued.Count) return Queued.ElementAt(index);

				throw new IndexOutOfRangeException();
			}
		}

		public List<T> QueuedItems()
		{
			lock (lockObject)
				return Queued.ToList();
		}
		public List<T> CompletedItems()
		{
			lock (lockObject)
				return Completed.ToList();
		}

		public bool QueueCount
		{
			get
			{
				lock (lockObject)
					return Queued.Count > 0;
			}
		}

		public bool CompleteCount
		{
			get
			{
				lock (lockObject)
					return Completed.Count > 0;
			}
		}


		public void ClearQueue()
		{
			lock (lockObject)
			{
				Queued.Clear();
			}
		}

		public void ClearCompleted()
		{
			lock (lockObject)
			{
				Completed.Clear();
			}
		}

		public bool Any(Func<T, bool> predicate)
		{
			lock (lockObject)
			{
				return (Current != null && predicate(Current)) || Completed.Any(predicate) || Queued.Any(predicate);
			}
		}

		public QueuePosition MoveQueuePosition(T item, QueuePositionRequest requestedPosition)
		{
			lock (lockObject)
			{
				if (Queued.Count == 0)
				{

					if (Current != null && Current == item)
						return QueuePosition.Current;
					if (Completed.Contains(item))
						return QueuePosition.Completed;
					return QueuePosition.Absent;
				}

				var node = Queued.Find(item);
				if (node is null) return QueuePosition.Absent;

				if ((requestedPosition == QueuePositionRequest.Fisrt || requestedPosition == QueuePositionRequest.OneUp) && Queued.First.Value == item)
					return QueuePosition.Fisrt;
				if ((requestedPosition == QueuePositionRequest.Last || requestedPosition == QueuePositionRequest.OneDown) && Queued.Last.Value == item)
					return QueuePosition.Last;

				if (requestedPosition == QueuePositionRequest.OneUp)
				{
					var oneUp = node.Previous;
					Queued.Remove(node);
					Queued.AddBefore(oneUp, node.Value);
					return Queued.First.Value == item? QueuePosition.Fisrt : QueuePosition.OneUp;
				}
				else if (requestedPosition == QueuePositionRequest.OneDown)
				{
					var oneDown = node.Next;
					Queued.Remove(node);
					Queued.AddAfter(oneDown, node.Value);
					return Queued.Last.Value == item ? QueuePosition.Last : QueuePosition.OneDown;
				}
				else if (requestedPosition == QueuePositionRequest.Fisrt)
				{
					Queued.Remove(node);
					Queued.AddFirst(node);
					return QueuePosition.Fisrt;
				}
				else
				{
					Queued.Remove(node);
					Queued.AddLast(node);
					return QueuePosition.Last;
				}
			}
		}

		public bool Remove(T item)
		{
			lock (lockObject)
			{
				return Queued.Remove(item); 
			}
		}

		public bool MoveNext()
		{
			lock (lockObject)
			{
				if (Current != null)
					Completed.Add(Current);
				if (Queued.Count == 0) return false;
				Current = Queued.First.Value;
				Queued.RemoveFirst();
				return true;
			}
		}
		public T PeekNext()
		{
			lock (lockObject)
				return Queued.Count > 0 ? Queued.First.Value : default;
		}

		public void EnqueueBook(T item)
		{
			lock (lockObject)
				Queued.AddLast(item);
		}

	}
}
