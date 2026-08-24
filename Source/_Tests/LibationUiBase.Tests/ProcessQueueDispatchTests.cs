using DataLayer;
using LibationFileManager;
using LibationUiBase.Forms;
using LibationUiBase.ProcessQueue;
using System.Collections.Concurrent;

namespace LibationUiBase.Tests;

/// <summary>
/// Covers the dispatch loop itself: the capacity cap, the enqueue signal, and the abort drain.
/// <para>
/// All of the risk parallel downloads added lives in that loop, and none of it was reachable from a
/// test while the only way to run a book was to download one. <see cref="ProcessQueueViewModel.ProcessBookHandler"/>
/// is the seam: every book here finishes when this test says so and never touches the network, the
/// database or the disk. The loop is the real one.
/// </para>
/// </summary>
[TestClass]
[DoNotParallelize]
public class ProcessQueueDispatchTests
{
	private static readonly TimeSpan Patience = TimeSpan.FromSeconds(10);

	[TestInitialize]
	public void Initialize()
	{
		// ProcessQueueViewModel raises property changes through the current SynchronizationContext.
		SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
		Configuration.CreateMockInstance();
		MessageBoxBase.ShowAsyncImpl = (_, _, _, _, _, _, _) => Task.FromResult(DialogResult.OK);
	}

	[TestCleanup]
	public void Cleanup()
	{
		MessageBoxBase.ShowAsyncImpl = null!;
		Configuration.RestoreSingletonInstance();
	}

	/// <summary>
	/// A book with no processables attached, so <c>IncludesBookDownload</c> is false and the daily
	/// limit gate returns immediately without querying the download history.
	/// </summary>
	/// <remarks>
	/// Must be called before the test's first <c>await</c>. ReactiveObject captures the current
	/// SynchronizationContext in its constructor, and after an await the test has resumed on a
	/// thread-pool thread where there is none.
	/// </remarks>
	private static ProcessBookViewModel Book(string asin)
	{
		var contributor = Contributor.GetEmpty();
		var book = new Book(new AudibleProductId(asin), asin, null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		return new ProcessBookViewModel(new LibraryBook(book, new DateTime(2026, 8, 10), "account"), Configuration.Instance);
	}

	/// <summary>
	/// Hands out a gate per book so a test can decide, from outside, exactly when each one finishes
	/// and in what order. Also records how many were running at the same moment, which is the only
	/// way to observe the capacity cap.
	/// </summary>
	private sealed class FakeBooks
	{
		private readonly ConcurrentDictionary<string, TaskCompletionSource<ProcessBookResult>> gates = new();
		private readonly object countLock = new();
		private int running;

		public int HighWaterMark { get; private set; }
		public ConcurrentQueue<string> Started { get; } = new();

		public Task<ProcessBookResult> Handle(ProcessBookViewModel book)
		{
			var asin = book.LibraryBook.Book.AudibleProductId.ToString()!;

			// Count first, then publish. WaitForStarted polls Started, so enqueuing before the
			// counter moves lets a test wake up in the gap and assert a HighWaterMark that is one
			// short of the books it just waited for.
			lock (countLock)
			{
				running++;
				if (running > HighWaterMark)
					HighWaterMark = running;
			}

			Started.Enqueue(asin);

			return GateFor(asin).Task.ContinueWith(t =>
			{
				lock (countLock) running--;
				// The real ProcessOneAsync records its own outcome on the book, and the queue loop reads
				// it back afterwards. A fake that only returned the value would leave Result unset.
				book.Result = t.Result;
				return t.Result;
			});
		}

		private TaskCompletionSource<ProcessBookResult> GateFor(string asin)
			=> gates.GetOrAdd(asin, _ => new TaskCompletionSource<ProcessBookResult>(TaskCreationOptions.RunContinuationsAsynchronously));

		public void Finish(string asin, ProcessBookResult result = ProcessBookResult.Success)
			=> GateFor(asin).TrySetResult(result);

		public void FinishAll(params string[] asins)
		{
			foreach (var asin in asins)
				Finish(asin);
		}

		/// <summary>Waits for <paramref name="count"/> books to have entered the handler.</summary>
		public async Task WaitForStarted(int count)
		{
			var deadline = DateTime.UtcNow + Patience;
			while (Started.Count < count)
			{
				if (DateTime.UtcNow > deadline)
					Assert.Fail($"Only {Started.Count} of {count} books started within {Patience.TotalSeconds}s.");
				await Task.Delay(15);
			}
		}
	}

	/// <param name="atOnce">
	/// The setting under test. Machine capability is pinned to the same number, because the loop
	/// clamps the setting by it: left to <see cref="Environment.ProcessorCount"/>, a test asking for
	/// three books at once would quietly start two on a small CI runner, wait out its patience and
	/// fail - having measured the runner rather than the loop.
	/// </param>
	private static (ProcessQueueViewModel Queue, FakeBooks Books) NewQueue(int atOnce)
	{
		var books = new FakeBooks();
		var queue = new ProcessQueueViewModel { MaxConcurrentDownloads = atOnce, MachineCeilingOverride = atOnce };
		queue.ProcessBookHandler = books.Handle;
		return (queue, books);
	}

	private static async Task RunToCompletion(ProcessQueueViewModel queue)
	{
		var runner = queue.QueueRunner;
		Assert.IsNotNull(runner, "The queue loop never started.");
		var finished = await Task.WhenAny(runner, Task.Delay(Patience));
		Assert.AreSame(runner, finished, $"The queue loop did not finish within {Patience.TotalSeconds}s.");
		await runner;
	}

	[TestMethod]
	public async Task the_loop_starts_no_more_books_than_the_concurrency_setting_allows()
	{
		var (queue, books) = NewQueue(atOnce: 2);

		queue.AddToQueue([Book("A"), Book("B"), Book("C"), Book("D")]);

		// Two start; C and D must wait for a slot rather than all four going at once.
		await books.WaitForStarted(2);
		await Task.Delay(100);
		Assert.AreEqual(2, books.Started.Count, "A third book started while the queue was at capacity.");

		books.FinishAll("A", "B");
		await books.WaitForStarted(4);
		books.FinishAll("C", "D");

		await RunToCompletion(queue);
		Assert.AreEqual(2, books.HighWaterMark, "More books ran at once than the setting allows.");
		Assert.AreEqual(4, queue.Queue.Completed.Count);
	}

	[TestMethod]
	public async Task lowering_the_setting_mid_run_does_not_start_more_books_until_the_extra_ones_finish()
	{
		var (queue, books) = NewQueue(atOnce: 3);

		queue.AddToQueue([Book("A"), Book("B"), Book("C"), Book("D")]);
		await books.WaitForStarted(3);

		queue.MaxConcurrentDownloads = 1;
		books.Finish("A");

		// Down to two running, which is still over the new cap, so D stays put.
		await Task.Delay(150);
		Assert.AreEqual(3, books.Started.Count, "A book started while the queue was still over its lowered cap.");

		books.FinishAll("B", "C");
		await books.WaitForStarted(4);
		books.Finish("D");

		await RunToCompletion(queue);
		Assert.AreEqual(4, queue.Queue.Completed.Count);
	}

	[TestMethod]
	public async Task books_queued_after_the_loop_starts_fill_the_free_slots_without_waiting_for_one_to_finish()
	{
		var (queue, books) = NewQueue(atOnce: 3);
		ProcessBookViewModel a = Book("A"), b = Book("B"), c = Book("C");

		// One book, so the loop ends up parked with two slots free and nothing queued. Without the
		// enqueue signal it would sit on the active task and pick the new books up one at a time as
		// that one finished, instead of waking on the arrival.
		queue.AddToQueue([a]);
		await books.WaitForStarted(1);

		queue.AddToQueue([b, c]);

		await books.WaitForStarted(3);
		Assert.AreEqual(3, books.HighWaterMark, "Newly queued books did not fill the free slots.");

		books.FinishAll("A", "B", "C");
		await RunToCompletion(queue);
		Assert.AreEqual(3, queue.Queue.Completed.Count);
	}

	[TestMethod]
	public async Task a_book_queued_while_the_loop_is_finishing_is_still_picked_up()
	{
		var (queue, books) = NewQueue(atOnce: 1);
		ProcessBookViewModel a = Book("A"), b = Book("B");

		queue.AddToQueue([a]);
		await books.WaitForStarted(1);

		// Racing the loop's exit: the wait captured before the queue is inspected is what stops this
		// book from being stranded by arriving in the gap.
		books.Finish("A");
		queue.AddToQueue([b]);

		await books.WaitForStarted(2);
		books.Finish("B");

		await RunToCompletion(queue);
		Assert.AreEqual(2, queue.Queue.Completed.Count, "A book queued as the loop wound down was stranded.");
	}

	[TestMethod]
	public async Task an_abort_clears_the_queue_and_the_loop_still_finishes_cleanly()
	{
		// One slot on purpose. With more, the aborting book and another finishing together let the
		// loop take a third book off the queue before the abort has cleared it - a real window, but a
		// tiny one, and not what this test is about.
		var (queue, books) = NewQueue(atOnce: 1);

		queue.AddToQueue([Book("A"), Book("B"), Book("C")]);
		await books.WaitForStarted(1);

		books.Finish("A", ProcessBookResult.FailedAbort);

		// The loop has to come back rather than dying inside the drain, and B and C must never start.
		await RunToCompletion(queue);

		Assert.AreEqual(1, books.Started.Count, "A queued book started after the abort.");
		// The queue itself, not the view model's QueuedCount mirror. That mirror arrives through the
		// posted path, and the bare SynchronizationContext installed in TestInitialize posts to the
		// thread pool - so reading it here raced delivery and failed most runs. A is the only book
		// left on the board: it completed, and the abort cleared B and C without starting them.
		Assert.AreEqual(1, queue.Queue.Count, "The abort left books on the queue.");
		Assert.IsFalse(queue.ProgressBarVisible, "The loop exited without clearing the progress bar.");
	}

	[TestMethod]
	public async Task only_one_book_reports_an_abort_and_the_rest_report_cancelled()
	{
		var (queue, books) = NewQueue(atOnce: 3);

		var a = Book("A");
		var b = Book("B");
		var c = Book("C");
		queue.AddToQueue([a, b, c]);
		await books.WaitForStarted(3);

		// No dialog was answered here - this is Bad Book set to Abort in settings, where every book
		// that fails aborts on its own account. Nobody is the one the user was asked about, so the
		// first book through tears the queue down and keeps the abort; the others were cancelled by it.
		Assert.IsNull(queue.BadBookSession.AbortOriginator);

		books.Finish("A", ProcessBookResult.FailedAbort);
		books.Finish("B", ProcessBookResult.FailedAbort);
		books.Finish("C", ProcessBookResult.FailedAbort);

		await RunToCompletion(queue);

		var aborted = new[] { a, b, c }.Count(x => x.Result is ProcessBookResult.FailedAbort);
		var cancelled = new[] { a, b, c }.Count(x => x.Result is ProcessBookResult.Cancelled);
		Assert.AreEqual(1, aborted, "More than one book claimed the abort.");
		Assert.AreEqual(2, cancelled, "Books that inherited the abort should report as cancelled.");
	}

	[TestMethod]
	public async Task the_book_the_user_aborted_reports_the_abort_whichever_book_tears_the_queue_down()
	{
		var (queue, books) = NewQueue(atOnce: 3);

		var a = Book("A");
		var b = Book("B");
		var c = Book("C");
		queue.AddToQueue([a, b, c]);
		await books.WaitForStarted(3);

		// C is the book the user was looking at when they answered Abort. A and B inherit that answer
		// through the session override, which is what puts all three here reporting FailedAbort.
		queue.BadBookSession.AbortOriginator = c;

		// A finishes first and so claims the teardown. Nothing below depends on it winning - that race
		// is what made the old status arbitrary - but this is the ordering that used to leave the row
		// the user actually aborted saying "Cancelled" while A's said "Error, Abort".
		books.Finish("A", ProcessBookResult.FailedAbort);
		await Task.Delay(50);
		books.Finish("B", ProcessBookResult.FailedAbort);
		books.Finish("C", ProcessBookResult.FailedAbort);

		await RunToCompletion(queue);

		Assert.AreEqual(ProcessBookResult.FailedAbort, c.Result, "The book the user aborted did not report the abort.");
		Assert.AreEqual(ProcessBookResult.Cancelled, a.Result, "A book that inherited the abort reported it as its own.");
		Assert.AreEqual(ProcessBookResult.Cancelled, b.Result, "A book that inherited the abort reported it as its own.");
	}

	[TestMethod]
	public async Task a_book_that_throws_is_logged_and_the_rest_of_the_queue_still_finishes()
	{
		var books = new FakeBooks();
		var queue = new ProcessQueueViewModel { MaxConcurrentDownloads = 2 };
		queue.ProcessBookHandler = book
			=> book.LibraryBook.Book.AudibleProductId.ToString() == "BOOM"
				? Task.FromException<ProcessBookResult>(new InvalidOperationException("Queue empty."))
				: books.Handle(book);

		queue.AddToQueue([Book("BOOM"), Book("A")]);
		await books.WaitForStarted(1);
		books.Finish("A");

		// The faulted task is observed on the way out. Before this, it took the loop out through its
		// outer catch and the remaining books ran on unsupervised.
		await RunToCompletion(queue);
		Assert.IsFalse(queue.ProgressBarVisible, "The loop died rather than finishing.");
	}

	[TestMethod]
	public async Task cancel_all_empties_the_queue_and_lets_the_loop_finish()
	{
		var (queue, books) = NewQueue(atOnce: 2);

		queue.AddToQueue([Book("A"), Book("B"), Book("C"), Book("D")]);
		await books.WaitForStarted(2);

		var cancelling = queue.CancelAllAsync();
		books.FinishAll("A", "B");
		await cancelling;

		await RunToCompletion(queue);
		// See the abort test: QueuedCount is the posted mirror and races delivery. A and B are the
		// only books left on the board; Cancel All took C and D off it before either could start.
		Assert.AreEqual(2, queue.Queue.Count, "Cancel All left books queued.");
		Assert.AreEqual(2, books.Started.Count, "Cancel All did not stop new books from starting.");
	}

	[TestMethod]
	public async Task a_machine_smaller_than_the_setting_holds_the_loop_down_without_changing_the_setting()
	{
		var books = new FakeBooks();
		var queue = new ProcessQueueViewModel { MaxConcurrentDownloads = 8, MachineCeilingOverride = 2 };
		queue.ProcessBookHandler = books.Handle;

		queue.AddToQueue([Book("A"), Book("B"), Book("C"), Book("D")]);

		await books.WaitForStarted(2);
		await Task.Delay(100);
		Assert.AreEqual(2, books.Started.Count, "The machine ceiling did not hold the loop down.");

		// The whole point of clamping here rather than on the way in: what the user asked for
		// survives being opened on a machine that cannot deliver it.
		Assert.AreEqual(8, queue.MaxConcurrentDownloads, "The stored setting was rewritten to what the machine could manage.");

		books.FinishAll("A", "B");
		await books.WaitForStarted(4);
		books.FinishAll("C", "D");

		await RunToCompletion(queue);
		Assert.AreEqual(2, books.HighWaterMark, "More books ran at once than the machine allows.");
	}

	[TestMethod]
	public async Task cancelling_a_book_with_nothing_running_still_records_the_cancellation()
	{
		var book = Book("A");
		Assert.IsFalse(book.CancellationRequested);

		// Nothing has started, so there is no step to cancel - and that is the case that matters. A
		// book held at the daily download limit is in exactly this state, and the gate reads this to
		// decide whether to resume it. Recording it on the book rather than on the queue is what
		// stops a later AddToQueue withdrawing the cancellation while the book is still parked.
		await book.CancelAsync();

		Assert.IsTrue(book.CancellationRequested);
	}

	[TestMethod]
	public void the_hint_says_what_the_machine_will_do_and_is_silent_when_it_can_keep_up()
	{
		var queue = new ProcessQueueViewModel { MaxConcurrentDownloads = 8, MachineCeilingOverride = 2 };
		Assert.AreEqual("(2 at a time)", queue.ConcurrencyHint);

		// Nothing to say once the machine can deliver what was asked for.
		queue.MachineCeilingOverride = 10;
		Assert.IsNull(queue.ConcurrencyHint);
	}
}
