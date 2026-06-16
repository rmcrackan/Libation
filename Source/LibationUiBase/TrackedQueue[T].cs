using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace LibationUiBase;

public enum QueuePosition
{
	First,
	OneUp,
	OneDown,
	Last,
}

/*
 * This data structure is like lifting a metal chain one link at a time.
 * Each time you grab and lift a new link (TryDequeueNext call):
 *
 *   1) you're holding new links in your hand (Active)
 *   2) the remaining chain to be lifted shortens (Queued)
 *   3) as links are finished, the pile at your feet grows (Completed)
 *
 * The index is the link position from the first link you lifted to the
 * last one in the chain.
 */
public class TrackedQueue<T> : IReadOnlyCollection<T>, IList, INotifyCollectionChanged where T : class
{
	public event EventHandler<int>? CompletedCountChanged;
	public event EventHandler<int>? QueuedCountChanged;
	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	/// <summary>Returns the first active item for backward compatibility (e.g. speed limit display).</summary>
	public T? Current => _active.FirstOrDefault();
	public IReadOnlyList<T> Active => _active;
	public IReadOnlyList<T> Completed => _completed;
	private List<T> Queued { get; } = new();

	private readonly List<T> _active = new();
	private readonly List<T> _completed = new();
	private readonly object lockObject = new();
	private int QueueStartIndex => Completed.Count + _active.Count;

	public T this[int index]
	{
		get
		{
			lock (lockObject)
			{
				if (index < Completed.Count)
					return Completed[index];
				int activeOffset = index - Completed.Count;
				if (activeOffset < _active.Count)
					return _active[activeOffset];
				int queueOffset = index - QueueStartIndex;
				if (queueOffset >= 0 && queueOffset < Queued.Count)
					return Queued[queueOffset];
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
				return QueueStartIndex + Queued.Count;
			}
		}
	}

	public int IndexOf(T item)
	{
		lock (lockObject)
		{
			int index = _completed.IndexOf(item);
			if (index < 0)
			{
				int activeIdx = _active.IndexOf(item);
				if (activeIdx >= 0)
					index = Completed.Count + activeIdx;
			}
			if (index < 0)
			{
				index = Queued.IndexOf(item);
				if (index >= 0)
					index += QueueStartIndex;
			}
			return index;
		}
	}

	public bool RemoveQueued(T item)
	{
		int queuedCount, queueIndex;

		lock (lockObject)
		{
			queueIndex = Queued.IndexOf(item);
			if (queueIndex >= 0)
				Queued.RemoveAt(queueIndex);
			queuedCount = Queued.Count;
		}

		if (queueIndex >= 0)
		{
			QueuedCountChanged?.Invoke(this, queuedCount);
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, QueueStartIndex + queueIndex));
			return true;
		}
		return false;
	}

	public bool RemoveCompleted(T item)
	{
		int completedCount, completedIndex;

		lock (lockObject)
		{
			completedIndex = _completed.IndexOf(item);
			if (completedIndex >= 0)
				_completed.RemoveAt(completedIndex);
			completedCount = _completed.Count;
		}

		if (completedIndex >= 0)
		{
			CompletedCountChanged?.Invoke(this, completedCount);
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, completedIndex));
			return true;
		}
		return false;
	}

	/// <summary>
	/// Pops the next item from the queue into the Active set.
	/// Returns false when the queue is empty.
	/// </summary>
	public bool TryDequeueNext([MaybeNullWhen(false)] out T item)
	{
		int queuedCount;
		lock (lockObject)
		{
			if (Queued.Count == 0)
			{
				item = null;
				return false;
			}
			item = Queued[0];
			Queued.RemoveAt(0);
			_active.Add(item);
			queuedCount = Queued.Count;
		}
		QueuedCountChanged?.Invoke(this, queuedCount);
		return true;
	}

	/// <summary>
	/// Moves an active item into Completed when its processing succeeds or fails normally.
	/// </summary>
	public void MarkCompleted(T item)
	{
		int completedCount;
		lock (lockObject)
		{
			_active.Remove(item);
			_completed.Add(item);
			completedCount = _completed.Count;
		}
		CompletedCountChanged?.Invoke(this, completedCount);
	}

	/// <summary>
	/// Removes an active item from the queue display entirely (used for ValidationFail).
	/// </summary>
	public void RemoveActive(T item)
	{
		int removedIndex;
		lock (lockObject)
		{
			removedIndex = _active.IndexOf(item);
			if (removedIndex >= 0)
				_active.RemoveAt(removedIndex);
		}
		if (removedIndex >= 0)
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, _completed.Count + removedIndex));
	}

	/// <summary>Legacy single-item sequential accessor — kept for compatibility.</summary>
	public void ClearCurrent()
	{
		T? first;
		lock (lockObject)
		{
			first = _active.FirstOrDefault();
			if (first != null)
				_active.Remove(first);
		}
		if (first != null)
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, first, _completed.Count));
	}

	public void ClearQueue()
	{
		List<T> queuedItems;
		lock (lockObject)
		{
			queuedItems = Queued.ToList();
			Queued.Clear();
		}
		QueuedCountChanged?.Invoke(this, 0);
		CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, queuedItems, QueueStartIndex));
	}

	public void ClearCompleted()
	{
		List<T> completedItems;
		lock (lockObject)
		{
			completedItems = _completed.ToList();
			_completed.Clear();
		}
		CompletedCountChanged?.Invoke(this, 0);
		CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, completedItems, 0));
	}

	public void MoveQueuePosition(T item, QueuePosition requestedPosition)
	{
		int oldIndex, newIndex;
		lock (lockObject)
		{
			oldIndex = Queued.IndexOf(item);
			newIndex = requestedPosition switch
			{
				QueuePosition.First => 0,
				QueuePosition.OneUp => oldIndex - 1,
				QueuePosition.OneDown => oldIndex + 1,
				QueuePosition.Last or _ => Queued.Count - 1
			};

			if (oldIndex < 0 || newIndex < 0 || newIndex >= Queued.Count || newIndex == oldIndex)
				return;

			Queued.RemoveAt(oldIndex);
			Queued.Insert(newIndex, item);
		}
		CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, QueueStartIndex + newIndex, QueueStartIndex + oldIndex));
	}

	/// <summary>
	/// Legacy sequential MoveNext — completes the first active item and dequeues the next.
	/// Only valid when at most one item is active at a time.
	/// </summary>
	public bool MoveNext()
	{
		T? oldActive;
		int completedCount = 0, queuedCount = 0;
		bool completedChanged = false;
		try
		{
			lock (lockObject)
			{
				oldActive = _active.FirstOrDefault();
				if (oldActive != null)
				{
					_active.Remove(oldActive);
					_completed.Add(oldActive);
					completedCount = _completed.Count;
					completedChanged = true;
				}
				if (Queued.Count == 0)
					return false;
				var next = Queued[0];
				Queued.RemoveAt(0);
				_active.Add(next);
				queuedCount = Queued.Count;
				return true;
			}
		}
		finally
		{
			if (completedChanged)
				CompletedCountChanged?.Invoke(this, completedCount);
			QueuedCountChanged?.Invoke(this, queuedCount);
		}
	}

	public void Enqueue(IList<T> item)
	{
		int queueCount;
		lock (lockObject)
		{
			Queued.AddRange(item);
			queueCount = Queued.Count;
		}
		QueuedCountChanged?.Invoke(this, queueCount);
		CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, QueueStartIndex + Queued.Count));
	}

	public IEnumerable<T> GetAllItems()
	{
		lock (lockObject)
		{
			return _completed.Concat(_active).Concat(Queued);
		}
	}

	public IEnumerator<T> GetEnumerator() => GetAllItems().GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	#region IList interface implementation
	object? IList.this[int index] { get => this[index]; set => throw new NotSupportedException(); }
	public bool IsReadOnly => true;
	public bool IsFixedSize => false;
	public bool IsSynchronized => false;
	public object SyncRoot => this;
	public int IndexOf(object? value) => value is T t ? IndexOf(t) : -1;
	public bool Contains(object? value) => IndexOf(value) >= 0;
	//These aren't used by anything, but they are IList interface members and this class needs to be an IList for Avalonia
	public int Add(object? value) => throw new NotSupportedException();
	public void Clear() => throw new NotSupportedException();
	public void Insert(int index, object? value) => throw new NotSupportedException();
	public void Remove(object? value) => throw new NotSupportedException();
	public void RemoveAt(int index) => throw new NotSupportedException();
	public void CopyTo(Array array, int index) => throw new NotSupportedException();
	#endregion
}
