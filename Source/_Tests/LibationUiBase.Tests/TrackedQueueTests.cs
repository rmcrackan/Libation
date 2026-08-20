using System.Collections.Specialized;
using System.ComponentModel;

namespace LibationUiBase.Tests;

/// <summary>
/// TrackedQueue&lt;T&gt; is a pure data structure with no dependencies beyond System, so the ordering
/// and notification behaviour that parallel downloads depends on can be pinned down directly. Every
/// case here is reachable only when more than one item is active at a time.
/// </summary>
[TestClass]
public class TrackedQueueTests
{
	private sealed class Book(string id)
	{
		public string Id { get; } = id;
		public override string ToString() => Id;
	}

	private static TrackedQueue<Book> QueueOf(params Book[] books)
	{
		var queue = new TrackedQueue<Book>();
		queue.Enqueue(books);
		return queue;
	}

	private static List<(object? Item, int OldIndex, int NewIndex)> RecordMoves(TrackedQueue<Book> queue)
	{
		var moves = new List<(object?, int, int)>();
		queue.CollectionChanged += (_, e) =>
		{
			if (e.Action is NotifyCollectionChangedAction.Move)
				moves.Add((e.NewItems?[0], e.OldStartingIndex, e.NewStartingIndex));
		};
		return moves;
	}

	[TestMethod]
	public void the_second_of_two_active_books_finishing_first_reports_the_reorder()
	{
		Book a = new("A"), b = new("B"), c = new("C"), d = new("D");
		var queue = QueueOf(a, b, c, d);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);
		CollectionAssert.AreEqual(new[] { a, b, c, d }, queue.ToList());

		var moves = RecordMoves(queue);
		queue.MarkCompleted(b);

		// B is now the only completed book, so it sorts ahead of A, which is still running.
		CollectionAssert.AreEqual(new[] { b, a, c, d }, queue.ToList());

		// Without the Move a bound list keeps painting A at row 0 and shows B's progress against it.
		Assert.AreEqual(1, moves.Count);
		Assert.AreSame(b, moves[0].Item);
		Assert.AreEqual(1, moves[0].OldIndex);
		Assert.AreEqual(0, moves[0].NewIndex);
	}

	[TestMethod]
	public void one_book_at_a_time_reports_no_reorder()
	{
		Book a = new("A"), b = new("B");
		var queue = QueueOf(a, b);
		queue.TryDequeueNext(out _);

		var moves = RecordMoves(queue);
		queue.MarkCompleted(a);

		// The sequential path is unchanged: the finishing book is already first, nothing moved.
		CollectionAssert.AreEqual(new[] { a, b }, queue.ToList());
		Assert.AreEqual(0, moves.Count);
	}

	[TestMethod]
	public void completing_books_out_of_order_keeps_a_bound_list_in_step()
	{
		Book a = new("A"), b = new("B"), c = new("C");
		var queue = QueueOf(a, b, c);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);

		// A list that only ever sees CollectionChanged, as a bound UI list does.
		var bound = queue.ToList();
		queue.CollectionChanged += (_, e) =>
		{
			if (e.Action is not NotifyCollectionChangedAction.Move || e.NewItems?[0] is not Book moved)
				return;
			bound.RemoveAt(e.OldStartingIndex);
			bound.Insert(e.NewStartingIndex, moved);
		};

		queue.MarkCompleted(c);
		CollectionAssert.AreEqual(queue.ToList(), bound);

		queue.MarkCompleted(b);
		CollectionAssert.AreEqual(queue.ToList(), bound);

		queue.MarkCompleted(a);
		CollectionAssert.AreEqual(queue.ToList(), bound);

		CollectionAssert.AreEqual(new[] { c, b, a }, bound);
	}

	[TestMethod]
	public void completing_a_book_still_reports_the_completed_count()
	{
		Book a = new("A"), b = new("B");
		var queue = QueueOf(a, b);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);

		var counts = new List<int>();
		queue.CompletedCountChanged += (_, count) => counts.Add(count);

		queue.MarkCompleted(b);
		queue.MarkCompleted(a);

		CollectionAssert.AreEqual(new[] { 1, 2 }, counts);
	}

	[TestMethod]
	public void the_queue_can_be_enumerated_while_it_is_being_mutated()
	{
		Book a = new("A"), b = new("B"), c = new("C");
		var queue = QueueOf(a, b, c);
		queue.TryDequeueNext(out _);

		// Before GetAllItems snapshotted under the lock this threw
		// InvalidOperationException: Collection was modified.
		var seen = new List<Book>();
		foreach (var book in queue)
		{
			queue.Enqueue([new Book("queued while reading")]);
			seen.Add(book);
		}

		CollectionAssert.AreEqual(new[] { a, b, c }, seen);
	}

	[TestMethod]
	public void the_active_list_is_handed_out_as_a_copy()
	{
		Book a = new("A"), b = new("B");
		var queue = QueueOf(a, b);
		queue.TryDequeueNext(out _);

		var active = queue.GetActive();
		queue.TryDequeueNext(out _);

		// Callers iterate this while book tasks start and finish; it must not be the live list.
		Assert.AreEqual(1, active.Count);
		Assert.AreSame(a, active[0]);
		Assert.AreEqual(2, queue.GetActive().Count);
	}

	[TestMethod]
	public void removing_an_active_book_removes_that_book_and_not_the_first_one()
	{
		Book a = new("A"), b = new("B"), c = new("C");
		var queue = QueueOf(a, b, c);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);

		var removed = new List<(object? Item, int Index)>();
		queue.CollectionChanged += (_, e) =>
		{
			if (e.Action is NotifyCollectionChangedAction.Remove)
				removed.Add((e.OldItems?[0], e.OldStartingIndex));
		};

		queue.RemoveActive(b);

		CollectionAssert.AreEqual(new[] { a, c }, queue.GetActive().ToList());
		Assert.AreEqual(1, removed.Count);
		Assert.AreSame(b, removed[0].Item);
		Assert.AreEqual(1, removed[0].Index);
	}

	[TestMethod]
	public void deferring_an_active_book_sends_it_to_the_back_and_leaves_the_others_running()
	{
		// What the daily download limit does when it holds a book back: the book being deferred is
		// removed and re-queued. ClearCurrent() would have dropped A, some other book's download.
		Book a = new("A"), b = new("B"), c = new("C");
		var queue = QueueOf(a, b, c);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);

		queue.RemoveActive(b);
		queue.Enqueue([b]);

		CollectionAssert.AreEqual(new[] { a, c, b }, queue.ToList());
		CollectionAssert.AreEqual(new[] { a, c }, queue.GetActive().ToList());
	}

	[TestMethod]
	public void completing_a_book_that_is_not_active_changes_nothing()
	{
		Book a = new("A"), b = new("B");
		var queue = QueueOf(a, b);
		queue.TryDequeueNext(out _);

		int collectionChanges = 0;
		queue.CollectionChanged += (_, _) => collectionChanges++;
		var counts = new List<int>();
		queue.CompletedCountChanged += (_, count) => counts.Add(count);

		// B is still queued, so it has not completed. Appending it to Completed anyway would move
		// Count with no CollectionChanged at all, which desynchronises a bound list silently.
		queue.MarkCompleted(b);

		Assert.AreEqual(2, queue.Count);
		CollectionAssert.AreEqual(new[] { a, b }, queue.ToList());
		Assert.AreEqual(0, collectionChanges);
		Assert.AreEqual(0, counts.Count);
	}

	/// <summary>
	/// Stands in for the UI thread. <see cref="BeginInvoke"/> only queues, so a test decides when
	/// delivery happens and can assert that nothing was delivered inline.
	/// </summary>
	private sealed class QueuingInvoker : ISynchronizeInvoke
	{
		private readonly Queue<Action> _posted = new();

		/// <summary>Always true, which is what makes the real invoker post rather than run inline.</summary>
		public bool InvokeRequired => true;

		public IAsyncResult BeginInvoke(Delegate method, object?[]? args)
		{
			lock (_posted)
				_posted.Enqueue(() => method.DynamicInvoke(args));
			return NotDelivered.Instance;
		}

		/// <summary>Runs what has been posted, in the order it was posted, on the calling thread.</summary>
		public int Drain()
		{
			int delivered = 0;
			while (true)
			{
				Action next;
				lock (_posted)
				{
					if (_posted.Count == 0)
						return delivered;
					next = _posted.Dequeue();
				}
				next();
				delivered++;
			}
		}

		public object? EndInvoke(IAsyncResult result) => throw new NotSupportedException();

		// A blocking Invoke from inside TrackedQueue's lock would deadlock against a UI thread
		// waiting on that same lock. Nothing may reach this.
		public object? Invoke(Delegate method, object?[]? args)
			=> throw new NotSupportedException("TrackedQueue must post its notifications, never block on them.");

		private sealed class NotDelivered : IAsyncResult
		{
			public static readonly NotDelivered Instance = new();
			public object? AsyncState => null;
			public WaitHandle AsyncWaitHandle => throw new NotSupportedException();
			public bool CompletedSynchronously => false;
			public bool IsCompleted => false;
		}
	}

	[TestMethod]
	public void notifications_are_posted_and_never_delivered_inline()
	{
		Book a = new("A"), b = new("B");
		var invoker = new QueuingInvoker();
		var queue = QueueOf(a, b);
		queue.NotificationInvoker = invoker;
		queue.TryDequeueNext(out _);
		queue.TryDequeueNext(out _);

		var moves = RecordMoves(queue);
		queue.MarkCompleted(b);

		// Called straight from this thread and still nothing has run. An invoker that delivered
		// inline when it was already on the UI thread would let a UI-thread mutation jump ahead of
		// notifications a book thread posted earlier.
		Assert.AreEqual(0, moves.Count);

		invoker.Drain();

		Assert.AreEqual(1, moves.Count);
		Assert.AreSame(b, moves[0].Item);
		Assert.AreEqual(1, moves[0].OldIndex);
		Assert.AreEqual(0, moves[0].NewIndex);
	}

	[TestMethod]
	public void concurrent_completions_reach_a_bound_list_in_the_order_they_happened()
	{
		// The ordering this pins down is only reachable through the posted path, which is the one
		// the app runs and the one the inline tests above never touch.
		for (int run = 0; run < 50; run++)
		{
			Book[] books = [new("A"), new("B"), new("C"), new("D")];
			var invoker = new QueuingInvoker();
			var queue = QueueOf(books);
			queue.NotificationInvoker = invoker;
			for (int i = 0; i < books.Length; i++)
				queue.TryDequeueNext(out _);

			// A list that only ever sees CollectionChanged, as a bound UI list does.
			var bound = queue.ToList();
			queue.CollectionChanged += (_, e) =>
			{
				if (e.Action is not NotifyCollectionChangedAction.Move || e.NewItems?[0] is not Book moved)
					return;
				bound.RemoveAt(e.OldStartingIndex);
				bound.Insert(e.NewStartingIndex, moved);
			};

			// Dedicated threads rather than the pool: the barrier needs all four running at once,
			// and a pool that injects threads slowly would stall instead of racing.
			using var allReady = new Barrier(books.Length);
			var threads = books
				.Select(book => new Thread(() => { allReady.SignalAndWait(); queue.MarkCompleted(book); }))
				.ToArray();

			foreach (var thread in threads)
				thread.Start();
			foreach (var thread in threads)
				thread.Join();

			// Delivery happens here, on one thread, exactly as the UI thread would run it.
			invoker.Drain();

			CollectionAssert.AreEqual(queue.ToList(), bound, $"run {run}");
		}
	}
}
