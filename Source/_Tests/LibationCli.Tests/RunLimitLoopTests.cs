using ApplicationServices;
using AssertionHelper;
using DataLayer;
using Dinah.Core.ErrorHandling;
using FileLiberator;
using LibationFileManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LibationCli.Tests;

/// <summary>
/// Drives the real <c>liberate</c> run loop against a real library database and a processable that records
/// downloads the way <see cref="DownloadDecryptBook"/> does, which is as close to a limited run as is possible
/// without an Audible account.
/// </summary>
[TestClass]
[DoNotParallelize]
public class RunLimitLoopTests
{
	private string tempLibationFiles = string.Empty;
	private TextWriter originalOut = Console.Out;
	private StringWriter capturedOut = new();

	[TestInitialize]
	public void Initialize()
	{
		tempLibationFiles = Path.Combine(Path.GetTempPath(), $"libation-run-limit-tests-{Guid.NewGuid():N}");
		Directory.CreateDirectory(tempLibationFiles);

		// A fresh Configuration resolves LibationFiles from this variable, so the database lands in the temp dir.
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

	[TestMethod]
	public async Task A_book_limit_leaves_the_rest_of_the_library_for_the_next_run()
	{
		SeedLibrary(5);
		var processable = NewProcessable();

		await new LimitedRun(new(Configuration.DailyLimitUnit.Books, 2)).Go(processable);

		processable.Downloaded.Should().HaveCount(2);
		StringAssert.Contains(
			Output,
			"Reached this run's limit of 2 book(s). Downloaded 2 title(s); stopping. "
			+ "Remaining titles are still un-liberated and will be tried on the next run.");
		StringAssert.Contains(Output, "Done. Stopped early: this run's download limit was reached.");
	}

	[TestMethod]
	public async Task A_size_limit_stops_once_another_book_would_not_fit()
	{
		SeedLibrary(10);
		var processable = NewProcessable(bytesPerBook: 300L * 1024 * 1024);

		await new LimitedRun(new(Configuration.DailyLimitUnit.GB, 1)).Go(processable);

		// A fourth 300 MB title would be assumed to need another 400 MB, past the 1 GB limit.
		processable.Downloaded.Should().HaveCount(3);
		StringAssert.Contains(
			Output,
			"Reached this run's limit of 1 GB. Downloaded about 900 MB across 3 title(s); stopping. "
			+ "Remaining titles are still un-liberated and will be tried on the next run.");
	}

	[TestMethod]
	public async Task A_run_that_fits_inside_its_limit_says_nothing_about_it()
	{
		SeedLibrary(3);
		var processable = NewProcessable();

		await new LimitedRun(new(Configuration.DailyLimitUnit.Books, 10)).Go(processable);

		processable.Downloaded.Should().HaveCount(3);
		Output.Contains("Reached this run's limit").Should().BeFalse();
		StringAssert.Contains(Output, "Done. All books have been processed");
	}

	[TestMethod]
	public async Task A_run_ending_exactly_at_its_limit_says_nothing_either()
	{
		// Nothing was cut short, so reporting a limit reached would be misleading.
		SeedLibrary(2);
		var processable = NewProcessable();

		await new LimitedRun(new(Configuration.DailyLimitUnit.Books, 2)).Go(processable);

		processable.Downloaded.Should().HaveCount(2);
		Output.Contains("Reached this run's limit").Should().BeFalse();
		StringAssert.Contains(Output, "Done. All books have been processed");
	}

	[TestMethod]
	public async Task Without_a_limit_the_whole_library_is_processed()
	{
		SeedLibrary(6);
		var processable = NewProcessable();

		await new LimitedRun(null).Go(processable);

		processable.Downloaded.Should().HaveCount(6);
		StringAssert.Contains(Output, "Done. All books have been processed");
	}

	[TestMethod]
	public async Task Downloads_from_an_earlier_run_do_not_count_against_this_one()
	{
		SeedLibrary(4);
		var processable = NewProcessable();

		// The same titles, downloaded moments ago by a different run: inside the daily window, outside this run.
		for (var i = 0; i < 4; i++)
			DownloadHistoryStore.Record(ProductId(i), isAudiblePlus: true, bytes: 1, completedAt: DateTimeOffset.Now.AddHours(-1));

		await new LimitedRun(new(Configuration.DailyLimitUnit.Books, 3)).Go(processable);

		processable.Downloaded.Should().HaveCount(3);
	}

	private string Output => capturedOut.ToString();

	private static string ProductId(int index) => $"B0RUNLIMIT{index:0000}";

	private static RecordingProcessable NewProcessable(long bytesPerBook = 1024 * 1024)
		=> new() { Configuration = Configuration.Instance, BytesPerBook = bytesPerBook };

	private static void SeedLibrary(int count)
	{
		// Shared instances: a contributor is one row no matter how many books credit it.
		var author = new Contributor("Test Author");
		var narrator = new Contributor("Test Narrator");

		using var context = DbContexts.GetContext();

		for (var i = 0; i < count; i++)
		{
			var book = new Book(
				new AudibleProductId(ProductId(i)),
				$"Test Title {i}",
				"",
				"",
				600,
				ContentType.Product,
				[author],
				[narrator],
				"us");

			context.LibraryBooks.Add(new LibraryBook(book, new DateTime(2026, 1, 1).AddMinutes(i), "test-account"));
		}

		context.SaveChanges();
	}

	/// <summary>Stands in for <see cref="DownloadDecryptBook"/>: succeeds, and records what it "downloaded".</summary>
	private sealed class RecordingProcessable : Processable
	{
		public required long BytesPerBook { get; init; }
		public List<string> Downloaded { get; } = [];

		public override string Name => nameof(RecordingProcessable);

		public override bool Validate(LibraryBook libraryBook) => true;

		public override Task<StatusHandler> ProcessAsync(LibraryBook libraryBook)
		{
			Downloaded.Add(libraryBook.Book.AudibleProductId);
			DownloadHistoryStore.Record(libraryBook.Book.AudibleProductId, libraryBook.IsAudiblePlus, BytesPerBook);
			return Task.FromResult(new StatusHandler());
		}
	}

	/// <summary>Exposes the run loop, which is otherwise reachable only through a verb.</summary>
	private sealed class LimitedRun(RunDownloadLimit? limit) : ProcessableOptionsBase
	{
		protected override RunDownloadLimit? RunLimit => limit;

		protected override Task ProcessAsync() => Task.CompletedTask;

		public Task Go(Processable processable) => RunAsync(processable);
	}
}
