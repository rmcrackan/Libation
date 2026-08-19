using ApplicationServices;
using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core.ErrorHandling;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace FileLiberator.Tests;

/// <summary>
/// The whole chain from a thrown refusal to a title being left alone, against a real SQLite database. Recording
/// lives in <see cref="Processable"/> rather than in each host, so this is what proves the CLI and the GUI
/// queue both get it.
/// </summary>
[TestClass]
[DoNotParallelize]
public class AttemptFailureRecordingTests
{
	private string tempLibationFiles = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-attempt-recording-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();
	}

	[TestCleanup]
	public void Cleanup()
	{
		Configuration.RestoreSingletonInstance();
		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, null);

		try
		{
			Directory.Delete(tempLibationFiles, recursive: true);
		}
		catch (IOException)
		{
			// A leftover temp directory is not worth failing a test over.
		}
	}

	/// <summary>Stands in for the audiobook download: the step whose refusals are remembered.</summary>
	private sealed class FakeDownload : Processable
	{
		public override string Name => nameof(FakeDownload);
		public Exception? Throw { get; init; }
		protected override bool RecordsAttemptFailures => true;

		public override bool Validate(LibraryBook libraryBook) => true;

		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
			=> Throw is null ? Task.FromResult(new StatusHandler()) : throw Throw;
	}

	/// <summary>Stands in for the mp3 and Audiobookshelf steps, which never request a license.</summary>
	private sealed class FakeOtherStep : Processable
	{
		public override string Name => nameof(FakeOtherStep);
		public Exception? Throw { get; init; }

		public override bool Validate(LibraryBook libraryBook) => true;

		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
			=> Throw is null ? Task.FromResult(new StatusHandler()) : throw Throw;
	}

	private static ContentLicenseDeniedException Denied()
		=> new(
			new Uri("https://api.audible.com/1.0/content/B002V5B8OY/licenserequest"),
			new ContentLicense
			{
				Asin = "B002V5B8OY",
				StatusCode = "Denied",
				LicenseDenialReasons =
				[
					new LicenseDenialReason
					{
						ValidationType = "Ownership",
						RejectionReason = RejectionReason.RequesterEligibility,
						Message = "Ownership: No Ownership information returned by DAOQS"
					}
				]
			});

	private static LibraryBook Book(string title = "Refused Title")
		=> MockLibraryBook.CreateBook(title: title, bookStatus: LiberatedStatus.NotLiberated);

	private static DeferredDownload? Deferred(LibraryBook libraryBook)
		=> DownloadAttemptFailureStore.Find(libraryBook, DateTimeOffset.Now);

	[TestMethod]
	public async Task A_refused_download_is_remembered_and_the_title_left_alone()
	{
		var book = Book();
		var processable = new FakeDownload { Configuration = Configuration.Instance, Throw = Denied() };

		// The exception still reaches the caller: recording must not change how a failure is reported.
		await Assert.ThrowsExactlyAsync<ContentLicenseDeniedException>(() => processable.ProcessSingleAsync(book, validate: true));

		var deferred = Deferred(book);
		Assert.IsNotNull(deferred);
		Assert.AreEqual(DownloadFailureKind.LicenseDenied, deferred.Kind);
		Assert.AreEqual(1, deferred.ConsecutiveFailures);
		StringAssert.StartsWith(deferred.Reason, "Ownership: ");
	}

	[TestMethod]
	public async Task Each_further_refusal_pushes_the_next_attempt_further_out()
	{
		var book = Book();
		var processable = new FakeDownload { Configuration = Configuration.Instance, Throw = Denied() };

		for (var i = 0; i < 3; i++)
			await Assert.ThrowsExactlyAsync<ContentLicenseDeniedException>(() => processable.ProcessSingleAsync(book, validate: true));

		Assert.AreEqual(3, Deferred(book)!.ConsecutiveFailures);
	}

	[TestMethod]
	public async Task A_successful_download_forgets_the_refusal()
	{
		var book = Book();
		await Assert.ThrowsExactlyAsync<ContentLicenseDeniedException>(
			() => new FakeDownload { Configuration = Configuration.Instance, Throw = Denied() }.ProcessSingleAsync(book, validate: true));
		Assert.IsNotNull(Deferred(book));

		var status = await new FakeDownload { Configuration = Configuration.Instance }.ProcessSingleAsync(book, validate: true);

		Assert.IsTrue(status.IsSuccess);
		Assert.IsNull(Deferred(book));
	}

	[TestMethod]
	public async Task A_failure_that_is_nothing_to_do_with_Audible_is_not_remembered()
	{
		// Keeps the long-standing behaviour: retried on the next run, because nothing says it will fail again.
		var book = Book();
		var processable = new FakeDownload
		{
			Configuration = Configuration.Instance,
			Throw = new IOException("There is not enough space on the disk.")
		};

		await Assert.ThrowsExactlyAsync<IOException>(() => processable.ProcessSingleAsync(book, validate: true));

		Assert.IsNull(Deferred(book));
	}

	[TestMethod]
	public async Task A_step_that_asks_Audible_for_nothing_records_nothing()
	{
		// Converting to mp3 and uploading work on the files already on disk. There is no request for a record to
		// gate, so a failure there says nothing about whether Audible would refuse the title.
		var book = Book();
		var processable = new FakeOtherStep { Configuration = Configuration.Instance, Throw = Denied() };

		await Assert.ThrowsExactlyAsync<ContentLicenseDeniedException>(() => processable.ProcessSingleAsync(book, validate: true));

		Assert.IsNull(Deferred(book));
	}

	[TestMethod]
	public void The_supplement_download_remembers_a_refusal_as_the_audiobook_download_does()
	{
		// Both request the same content license, so a refusal of one is a refusal of the other. Until issue
		// #1973 the PDF step recorded nothing, and a scheduled run asked again every 15 minutes.
		Assert.IsTrue(RecordsAttemptFailures<DownloadPdf>());
		Assert.IsTrue(RecordsAttemptFailures<DownloadDecryptBook>());
		Assert.IsFalse(RecordsAttemptFailures<ConvertToMp3>());
		Assert.IsFalse(RecordsAttemptFailures<UploadToAudiobookshelf>());
	}

	private static bool RecordsAttemptFailures<T>() where T : Processable, IProcessable<T>
		=> (bool)typeof(Processable)
			.GetProperty(nameof(RecordsAttemptFailures), BindingFlags.NonPublic | BindingFlags.Instance)!
			.GetValue(T.Create(Configuration.Instance))!;

	[TestMethod]
	public async Task A_step_that_fails_validation_records_nothing()
	{
		var book = Book();
		var processable = new NeverValid { Configuration = Configuration.Instance };

		var status = await processable.ProcessSingleAsync(book, validate: true);

		Assert.IsFalse(status.IsSuccess);
		Assert.IsNull(Deferred(book));
	}

	private sealed class NeverValid : Processable
	{
		public override string Name => nameof(NeverValid);
		protected override bool RecordsAttemptFailures => true;
		public override bool Validate(LibraryBook libraryBook) => false;
		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook) => throw new InvalidOperationException("must not run");
	}
}
