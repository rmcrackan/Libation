using DataLayer;
using LibationFileManager;
using LibationUiBase.Forms;
using LibationUiBase.ProcessQueue;

namespace LibationUiBase.Tests;

/// <summary>
/// Covers what a multi-book backup request does when it cannot queue anything. These tests never queue a
/// book: doing so starts the queue loop, which downloads for real.
/// </summary>
[TestClass]
[DoNotParallelize]
public class QueueDownloadDecryptTests
{
	private string tempDir = "";
	private List<(string Message, string Caption)> Dialogs { get; } = [];

	[TestInitialize]
	public void Initialize()
	{
		//ProcessQueueViewModel raises property changes through the current SynchronizationContext
		SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

		tempDir = Path.Combine(Path.GetTempPath(), "LibationQueueDownloadDecryptTests", Path.GetRandomFileName());

		var config = Configuration.CreateMockInstance();
		config.Books = Path.Combine(tempDir, "Books");
		config.InProgress = Path.Combine(tempDir, "InProgress");

		MessageBoxBase.ShowAsyncImpl = (_, message, caption, _, _, _, _) =>
		{
			Dialogs.Add((message, caption));
			return Task.FromResult(DialogResult.OK);
		};
	}

	[TestCleanup]
	public void Cleanup()
	{
		//null restores MessageBoxBase's own no-op implementation
		MessageBoxBase.ShowAsyncImpl = null!;
		Configuration.RestoreSingletonInstance();

		if (Directory.Exists(tempDir))
			Directory.Delete(tempDir, recursive: true);
	}

	private static LibraryBook Liberated(string asin)
	{
		var contributor = Contributor.GetEmpty();
		var book = new Book(new AudibleProductId(asin), asin, null, null, 1, ContentType.Product, [contributor], [contributor], "us");
		book.UserDefinedItem.BookStatus = LiberatedStatus.Liberated;
		return new LibraryBook(book, new DateTime(2026, 8, 10), "account");
	}

	[TestMethod]
	public async Task selecting_several_downloaded_books_says_why_nothing_was_queued()
	{
		var queue = new ProcessQueueViewModel();

		var queued = await queue.QueueDownloadDecryptAsync([Liberated("DONE1"), Liberated("DONE2")]);

		Assert.IsFalse(queued);
		Assert.AreEqual(0, queue.Queue.Count);
		Assert.AreEqual(1, Dialogs.Count);
		Assert.AreEqual("Download not queued", Dialogs[0].Caption);
		StringAssert.Contains(Dialogs[0].Message, "Already downloaded: 2");
	}

	[TestMethod]
	public async Task a_request_the_caller_already_filtered_to_nothing_is_still_acknowledged()
	{
		var queue = new ProcessQueueViewModel();

		var queued = await queue.QueueDownloadDecryptAsync([]);

		Assert.IsFalse(queued);
		Assert.AreEqual(1, Dialogs.Count);
		StringAssert.Contains(Dialogs[0].Message, "no titles that need downloading");
	}

	[TestMethod]
	public async Task automated_callers_are_not_interrupted_by_a_dialog()
	{
		var queue = new ProcessQueueViewModel();

		var queued = await queue.QueueDownloadDecryptAsync([Liberated("DONE1"), Liberated("DONE2")], notifyIfNothingQueued: false);

		Assert.IsFalse(queued);
		Assert.AreEqual(0, Dialogs.Count);
	}
}
