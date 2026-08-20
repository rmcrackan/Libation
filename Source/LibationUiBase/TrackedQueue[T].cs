using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

	#region Notification dispatch

	/*
	 * Indices are only meaningful against the state they were computed from, so an index-based
	 * consumer can only follow the queue if it is told about mutations in the order they happened.
	 *
	 * Computing an index under lockObject and raising the event after releasing it does not give
	 * that: a second item can complete in the gap, and with two Move events in flight the one
	 * raised second can be delivered first. A bound list then reorders rows against a state that
	 * never existed - a duplicated row and a lost one. The gap is not tight, either, because
	 * MarkCompleted raises CompletedCountChanged first and its handler walks Completed twice
	 * before the Move goes out.
	 *
	 * So mutators append their notifications to _pending while they still hold lockObject, and
	 * delivery happens afterwards under dispatchLock. Whoever reaches dispatchLock first drains
	 * everything that is pending, so mutation order is delivery order no matter which thread does
	 * the delivering.
	 *
	 * Nothing is ever raised while lockObject is held. The UI thread reads Count, IndexOf and the
	 * indexer from inside these handlers - Avalonia's binding and WinForms' DoVirtualScroll both
	 * do - and those take lockObject, so raising under it would deadlock a book thread against
	 * the UI thread.
	 */

	private enum NotificationKind { CompletedCount, QueuedCount, Collection }

	private readonly record struct Notification(NotificationKind Kind, int Count, NotifyCollectionChangedEventArgs? Args);

	private readonly List<Notification> _pending = new();
	private readonly object dispatchLock = new();

	/// <summary>
	/// Marshals notifications onto the UI thread. Must post rather than run inline - see
	/// <see cref="NotificationInvoker"/> - and must never be a blocking <c>Invoke</c>, which would
	/// deadlock against a UI thread waiting on <see cref="lockObject"/>.
	/// </summary>
	/// <remarks>
	/// Null delivers inline on the mutating thread. That is the default because
	/// <see cref="TrackedQueue{T}"/> is a plain data structure with no thread of its own, and it is
	/// what the tests rely on to assert immediately after a mutation.
	/// <para>
	/// Whatever is assigned here must post unconditionally, including from the UI thread itself.
	/// An invoker that runs inline when it is already on the right thread lets a UI-thread mutation
	/// deliver ahead of notifications a book thread posted earlier, which is the reordering this
	/// exists to prevent. <c>Dinah.Core.Threading.SynchronizeInvoker</c> does that when constructed
	/// with <c>alwaysInvoke: true</c>.
	/// </para>
	/// </remarks>
	public ISynchronizeInvoke? NotificationInvoker { get; set; }

	/// <summary>Call while holding <see cref="lockObject"/>.</summary>
	private void Pend(int completedCount) => _pending.Add(new(NotificationKind.CompletedCount, completedCount, null));

	/// <summary>Call while holding <see cref="lockObject"/>.</summary>
	private void PendQueued(int queuedCount) => _pending.Add(new(NotificationKind.QueuedCount, queuedCount, null));

	/// <summary>Call while holding <see cref="lockObject"/>.</summary>
	private void Pend(NotifyCollectionChangedEventArgs args) => _pending.Add(new(NotificationKind.Collection, 0, args));

	/// <summary>Call only after <see cref="lockObject"/> has been released.</summary>
	private void DispatchPending()
	{
		lock (dispatchLock)
		{
			while (true)
			{
				Notification[] batch;
				lock (lockObject)
				{
					if (_pending.Count == 0)
						return;
					batch = _pending.ToArray();
					_pending.Clear();
				}

				var invoker = NotificationInvoker;
				if (invoker is null)
					Deliver(batch);
				else
					invoker.BeginInvoke((Action)(() => Deliver(batch)), null);
			}
		}
	}

	private void Deliver(Notification[] batch)
	{
		foreach (var notification in batch)
		{
			switch (notification.Kind)
			{
				case NotificationKind.CompletedCount:
					CompletedCountChanged?.Invoke(this, notification.Count);
					break;
				case NotificationKind.QueuedCount:
					QueuedCountChanged?.Invoke(this, notification.Count);
					break;
				default:
					CollectionChanged?.Invoke(this, notification.Args!);
					break;
			}
		}
	}

	#endregion

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
		bool removed;

		lock (lockObject)
		{
			int queueIndex = Queued.IndexOf(item);
			removed = queueIndex >= 0;
			if (removed)
			{
				Queued.RemoveAt(queueIndex);
				PendQueued(Queued.Count);
				Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, QueueStartIndex + queueIndex));
			}
		}

		DispatchPending();
		return removed;
	}

	public bool RemoveCompleted(T item)
	{
		bool removed;

		lock (lockObject)
		{
			int completedIndex = _completed.IndexOf(item);
			removed = completedIndex >= 0;
			if (removed)
			{
				_completed.RemoveAt(completedIndex);
				Pend(_completed.Count);
				Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, completedIndex));
			}
		}

		DispatchPending();
		return removed;
	}

	/// <summary>
	/// Pops the next item from the queue into the Active set.
	/// Returns false when the queue is empty.
	/// </summary>
	public bool TryDequeueNext([MaybeNullWhen(false)] out T item)
	{
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
			PendQueued(Queued.Count);
		}
		DispatchPending();
		return true;
	}

	/// <summary>
	/// Moves an active item into Completed when its processing succeeds or fails normally.
	/// </summary>
	/// <remarks>
	/// An item that is not active is not completed either. Appending it to <see cref="Completed"/>
	/// anyway would change <see cref="Count"/> with no <see cref="CollectionChanged"/> at all,
	/// which desynchronises every bound list silently - worse than the caller's own mistake.
	/// </remarks>
	public void MarkCompleted(T item)
	{
		lock (lockObject)
		{
			var activeIndex = _active.IndexOf(item);
			if (activeIndex < 0)
			{
				Serilog.Log.Logger.Error("MarkCompleted called on an item that is not active: {Item}", item);
				return;
			}

			int oldIndex = _completed.Count + activeIndex;
			_active.RemoveAt(activeIndex);
			_completed.Add(item);
			int newIndex = _completed.Count - 1;

			Pend(_completed.Count);

			// One book at a time, the finishing book is always the first active one, oldIndex equals
			// newIndex and nothing has moved. Concurrently it is normal for the second of two active
			// books to finish first, which puts it ahead of the other in the display order. Without
			// this a bound list keeps painting the previous order, so rows show the wrong book.
			if (oldIndex != newIndex)
				Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
		}
		DispatchPending();
	}

	/// <summary>
	/// Removes an active item from the queue display entirely (used for ValidationFail).
	/// </summary>
	public void RemoveActive(T item)
	{
		lock (lockObject)
		{
			int removedIndex = _active.IndexOf(item);
			if (removedIndex >= 0)
			{
				int displayIndex = _completed.Count + removedIndex;
				_active.RemoveAt(removedIndex);
				Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, displayIndex));
			}
		}
		DispatchPending();
	}

	/// <summary>
	/// The active items, copied under the lock. <see cref="Active"/> is the live list, so enumerating
	/// it while book tasks start and finish throws; callers that need to iterate use this.
	/// </summary>
	public IReadOnlyList<T> GetActive()
	{
		lock (lockObject)
			return _active.ToList();
	}

	/// <summary>Legacy single-item sequential accessor — kept for compatibility.</summary>
	public void ClearCurrent()
	{
		lock (lockObject)
		{
			var first = _active.FirstOrDefault();
			if (first != null)
			{
				int displayIndex = _completed.Count;
				_active.Remove(first);
				Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, first, displayIndex));
			}
		}
		DispatchPending();
	}

	public void ClearQueue()
	{
		lock (lockObject)
		{
			var queuedItems = Queued.ToList();
			Queued.Clear();
			PendQueued(0);
			Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, queuedItems, QueueStartIndex));
		}
		DispatchPending();
	}

	public void ClearCompleted()
	{
		lock (lockObject)
		{
			var completedItems = _completed.ToList();
			_completed.Clear();
			Pend(0);
			Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, completedItems, 0));
		}
		DispatchPending();
	}

	public void MoveQueuePosition(T item, QueuePosition requestedPosition)
	{
		lock (lockObject)
		{
			int oldIndex = Queued.IndexOf(item);
			int newIndex = requestedPosition switch
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
			Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, QueueStartIndex + newIndex, QueueStartIndex + oldIndex));
		}
		DispatchPending();
	}

	/// <summary>
	/// Legacy sequential MoveNext — completes the first active item and dequeues the next.
	/// Only valid when at most one item is active at a time.
	/// </summary>
	public bool MoveNext()
	{
		try
		{
			lock (lockObject)
			{
				var oldActive = _active.FirstOrDefault();
				if (oldActive != null)
				{
					_active.Remove(oldActive);
					_completed.Add(oldActive);
					Pend(_completed.Count);
				}
				if (Queued.Count == 0)
					return false;
				var next = Queued[0];
				Queued.RemoveAt(0);
				_active.Add(next);
				PendQueued(Queued.Count);
				return true;
			}
		}
		finally
		{
			DispatchPending();
		}
	}

	public void Enqueue(IList<T> item)
	{
		lock (lockObject)
		{
			Queued.AddRange(item);
			PendQueued(Queued.Count);
			Pend(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, QueueStartIndex + Queued.Count));
		}
		DispatchPending();
	}

	/// <summary>
	/// A snapshot, materialised while the lock is held. Returning the lazy <c>Concat</c> meant the
	/// enumeration ran after the lock was released, so any <c>foreach</c> or LINQ over the queue
	/// while a book task mutated it threw <see cref="InvalidOperationException"/>. That was
	/// unreachable while every mutation happened on the UI thread; concurrent processing makes it
	/// reachable, and it is the same failure that used to crash the visible-books menu.
	/// </summary>
	public IEnumerable<T> GetAllItems()
	{
		lock (lockObject)
		{
			var snapshot = new List<T>(_completed.Count + _active.Count + Queued.Count);
			snapshot.AddRange(_completed);
			snapshot.AddRange(_active);
			snapshot.AddRange(Queued);
			return snapshot;
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
