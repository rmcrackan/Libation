using ApplicationServices;
using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileLiberator;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LibationCli.Tests;

/// <summary>
/// What a scheduled run prints when Audible refuses a title. The user in issue #1947 runs from cron, so the
/// console output is the only place they find out that Libation has decided to wait rather than forgotten the
/// title.
/// </summary>
[TestClass]
[DoNotParallelize]
public class DeferredRetryReportingTests
{
	private string tempLibationFiles = string.Empty;

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-cli-deferred-tests-{Guid.NewGuid():N}");
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

	/// <summary>Exposes the shared per-book failure handling without running a real download.</summary>
	private sealed class TestableRun : ProcessableOptionsBase
	{
		protected override Task ProcessAsync() => throw new NotSupportedException();

		public Task ReportAsync(Processable processable, LibraryBook libraryBook)
			=> ProcessOneAsync(processable, libraryBook, validate: false);
	}

	private sealed class RefusedDownload : Processable
	{
		public override string Name => nameof(RefusedDownload);
		public required Exception Throw { get; init; }
		protected override bool RecordsAttemptFailures => true;
		public override bool Validate(LibraryBook libraryBook) => true;
		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook) => throw Throw;
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

	private async Task<string> RunAndCaptureStdErrAsync(Exception thrown, LibraryBook libraryBook)
	{
		var original = Console.Error;
		using var captured = new StringWriter();
		Console.SetError(captured);
		try
		{
			await new TestableRun().ReportAsync(
				new RefusedDownload { Configuration = Configuration.Instance, Throw = thrown },
				libraryBook);
		}
		finally
		{
			Console.SetError(original);
		}
		return captured.ToString();
	}

	private static LibraryBook Book(string title = "Refused Title")
		=> MockLibraryBook.CreateBook(title: title, bookStatus: LiberatedStatus.NotLiberated);

	[TestMethod]
	public async Task A_refusal_says_when_the_title_will_be_attempted_again()
	{
		var book = Book();

		var output = await RunAndCaptureStdErrAsync(Denied(), book);

		// The existing per-title detail is still printed the once...
		StringAssert.Contains(output, "Audible denied a content license");
		StringAssert.Contains(output, "Ownership: No Ownership information returned by DAOQS");
		// ...followed by the reason the next several runs will say nothing about this title.
		StringAssert.Contains(output, "Not attempting this title again in about 1 day");
		StringAssert.Contains(output, $"libationcli liberate {book.Book.AudibleProductId}");
	}

	[TestMethod]
	public async Task A_failure_nothing_to_do_with_Audible_still_promises_the_next_run()
	{
		var output = await RunAndCaptureStdErrAsync(new IOException("Connection reset"), Book());

		StringAssert.Contains(output, "This book will be tried again on next attempt.");
		Assert.IsFalse(output.Contains("Not attempting this title again"), output);
	}

	[TestMethod]
	public async Task A_refusal_no_longer_claims_the_next_run_will_try_again()
	{
		// The old message said "will be tried again on next attempt" for every failure, which was untrue for
		// the ones now waited on.
		var output = await RunAndCaptureStdErrAsync(Denied(), Book());

		Assert.IsFalse(output.Contains("tried again on next attempt"), output);
	}
}
