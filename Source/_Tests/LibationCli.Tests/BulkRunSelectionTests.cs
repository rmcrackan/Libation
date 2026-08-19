using ApplicationServices;
using AssertionHelper;
using AudibleApi;
using AudibleApi.Common;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileLiberator;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli.Tests;

/// <summary>
/// Which titles a bulk <c>liberate</c> run attempts, driving the real run loop against a real library database.
/// Reported in issue #1973: a run every 15 minutes attempted the same 59 titles every time - 54 of them absent
/// from the last scan, the rest refused - and none of it was ever remembered.
/// </summary>
[TestClass]
[DoNotParallelize]
public class BulkRunSelectionTests
{
	private string tempLibationFiles = string.Empty;
	private TextWriter originalOut = Console.Out;
	private StringWriter capturedOut = new();

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-bulk-selection-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		Environment.SetEnvironmentVariable(LibationFiles.LIBATION_FILES_DIR, tempLibationFiles);
		Configuration.CreateMockInstance();

		originalOut = Console.Out;
		capturedOut = new StringWriter();
		Console.SetOut(capturedOut);
	}

	[TestCleanup]
	public void Cleanup()
	{
		Console.SetOut(originalOut);
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

	private string Output => capturedOut.ToString();

	private const string Account = "test-account";

	private static void Seed(string productId, bool absentFromLastScan = false, bool hasSupplement = false, LiberatedStatus bookStatus = LiberatedStatus.NotLiberated)
	{
		var book = new Book(
			new AudibleProductId(productId),
			$"Title {productId}",
			"",
			"",
			600,
			DataLayer.ContentType.Product,
			[new Contributor("Test Author")],
			[new Contributor("Test Narrator")],
			"us");

		book.UserDefinedItem.BookStatus = bookStatus;

		if (hasSupplement)
			book.SetSupplementDownloadUrl($"https://example.com/{productId}.pdf");

		using var context = DbContexts.GetContext();
		context.LibraryBooks.Add(new LibraryBook(book, new DateTime(2026, 1, 1), Account) { AbsentFromLastScan = absentFromLastScan });
		context.SaveChanges();
	}

	/// <summary>Records that Audible refused this title, as a real attempt would have.</summary>
	private static void Refuse(string productId)
		=> DownloadAttemptFailureStore.Record(
			DbContexts.GetLibraryBook_Flat_NoTracking(productId)!,
			DownloadFailureKind.LicenseDenied,
			"Ownership: Customer does not have Ownership rights");

	[TestMethod]
	public async Task A_title_absent_from_the_last_scan_is_left_alone()
	{
		Seed("B0PRESENT01");
		Seed("B0ABSENT001", absentFromLastScan: true);
		var step = NewStep();

		await new BulkRun().Go(step);

		step.Attempted.Should().BeEquivalentTo(["B0PRESENT01"]);
		StringAssert.Contains(Output, "Skipped 1 title absent from your last library scan");
	}

	[TestMethod]
	public async Task An_absent_title_no_pass_wanted_is_not_counted_as_skipped()
	{
		// Most of a large library's absent titles need nothing at all. Counting those would report thousands of
		// skipped titles for a run that was only ever going to attempt a handful.
		Seed("B0ABSENTDONE", absentFromLastScan: true, bookStatus: LiberatedStatus.Liberated);
		Seed("B0ABSENTTODO", absentFromLastScan: true);

		await new BulkRun().Go(NewStep(lb => !lb.Book.AudioExists));

		StringAssert.Contains(Output, "Skipped 1 title absent from your last library scan");
	}

	[TestMethod]
	public async Task The_follow_up_pass_leaves_them_alone_too()
	{
		// The PDF back-fill pass never looked at this, which is where 54 of the 59 titles in the report came in:
		// their audio was already downloaded, so only the second pass ever selected them.
		Seed("B0ABSENTPDF", absentFromLastScan: true, hasSupplement: true, bookStatus: LiberatedStatus.Liberated);
		var firstPass = NewStep(lb => !lb.Book.AudioExists);
		var followUp = NewStep();

		await new BulkRun().Go(firstPass, followUp);

		firstPass.Attempted.Should().HaveCount(0);
		followUp.Attempted.Should().HaveCount(0);
	}

	[TestMethod]
	public async Task Force_attempts_them_anyway()
	{
		Seed("B0ABSENT001", absentFromLastScan: true);
		var step = NewStep();

		await new BulkRun { SkipsAbsent = false }.Go(step);

		step.Attempted.Should().BeEquivalentTo(["B0ABSENT001"]);
		Output.Contains("absent from your last library scan").Should().BeFalse();
	}

	[TestMethod]
	public async Task A_refused_title_is_waited_on_by_the_follow_up_pass()
	{
		// A PDF comes from the same license request as the audiobook, so a title being waited on would be
		// refused for its PDF too. Only the first pass consulted the record.
		Seed("B0REFUSED01", hasSupplement: true, bookStatus: LiberatedStatus.Liberated);
		Refuse("B0REFUSED01");
		var firstPass = NewStep(lb => !lb.Book.AudioExists);
		var followUp = NewStep();

		await new BulkRun { HonorsWaits = true }.Go(firstPass, followUp);

		followUp.Attempted.Should().HaveCount(0);
		StringAssert.Contains(Output, "Skipped 1 title that recently failed to download");
	}

	[TestMethod]
	public async Task A_title_the_first_pass_waited_on_is_only_reported_once()
	{
		Seed("B0REFUSED01");
		Refuse("B0REFUSED01");
		var firstPass = NewStep();
		var followUp = NewStep();

		await new BulkRun { HonorsWaits = true }.Go(firstPass, followUp);

		firstPass.Attempted.Should().HaveCount(0);
		followUp.Attempted.Should().HaveCount(0);
		StringAssert.Contains(Output, "Skipped 1 title that recently failed to download");
	}

	[TestMethod]
	public async Task A_run_with_nothing_to_skip_says_nothing_about_skipping()
	{
		Seed("B0PRESENT01");
		var step = NewStep();

		await new BulkRun().Go(step);

		step.Attempted.Should().BeEquivalentTo(["B0PRESENT01"]);
		Output.Contains("Skipped").Should().BeFalse();
		StringAssert.Contains(Output, "Done. All books have been processed");
	}

	/// <param name="applies">
	/// Which titles this pass has anything to do, standing in for a step's own Validate. The default is every
	/// title; the audiobook step's "only what is not downloaded" is what leaves a title to the follow-up pass.
	/// </param>
	private static AttemptedTitles NewStep(Func<LibraryBook, bool>? applies = null)
		=> new() { Configuration = Configuration.Instance, Applies = applies ?? (_ => true) };

	/// <summary>Records which titles a pass reached, without downloading anything.</summary>
	private sealed class AttemptedTitles : Processable
	{
		public List<string> Attempted { get; } = [];
		public required Func<LibraryBook, bool> Applies { get; init; }

		public override string Name => nameof(AttemptedTitles);
		public override bool Validate(LibraryBook libraryBook) => Applies(libraryBook);

		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
		{
			Attempted.Add(libraryBook.Book.AudibleProductId);
			return Task.FromResult(new StatusHandler());
		}
	}

	/// <summary>Exposes the run loop, which is otherwise reachable only through a verb.</summary>
	private sealed class BulkRun : ProcessableOptionsBase
	{
		public bool SkipsAbsent { get; init; } = true;
		public bool HonorsWaits { get; init; }

		internal override bool SkipsTitlesAbsentFromLastScan => SkipsAbsent;
		internal override bool HonorsDeferredRetries => HonorsWaits;

		protected override Task ProcessAsync() => Task.CompletedTask;

		public Task Go(Processable processable, Processable? bulkFollowUp = null)
			=> RunAsync(processable, bulkFollowUp: bulkFollowUp);
	}
}
