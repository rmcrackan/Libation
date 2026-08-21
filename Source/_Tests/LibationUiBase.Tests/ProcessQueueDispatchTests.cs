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
			Started.Enqueue(asin);

			lock (countLock)
			{
				running++;
				if (running > HighWaterMark)
					HighWaterMark = running;
			}

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

	private static (ProcessQueueViewModel Queue, FakeBooks Books) NewQueue(int atOnce)
	{
		var books = new FakeBooks();
		var queue = new ProcessQueueViewModel { MaxConcurrentDownloads = atOnce };
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
		Assert.AreEqual(0, queue.QueuedCount, "The abort left books on the queue.");
		Assert.IsFalse(queue.ProgressBarVisible, "The loop exited without clearing the progress bar.");
	}

	[TestMethod]
	public async Task only_the_book_that_aborted_reports_an_abort_and_the_rest_report_cancelled()
	{
		var (queue, books) = NewQueue(atOnce: 3);

		var a = Book("A");
		var b = Book("B");
		var c = Book("C");
		queue.AddToQueue([a, b, c]);
		await books.WaitForStarted(3);

		// All three inherit the abort, which is what a session-wide Abort answer produces. Only the
		// first through gets to tear the queue down; the others were cancelled by it.
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
		Assert.AreEqual(0, queue.QueuedCount, "Cancel All left books queued.");
		Assert.AreEqual(2, books.Started.Count, "Cancel All did not stop new books from starting.");
	}
}
