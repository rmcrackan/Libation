using CommandLine;
using FileLiberator;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("upload", HelpText = "Upload already-liberated books to Audiobookshelf. Default: all liberated titles whose audio is on disk.\n"
	+ "Optional: specify product id(s) via --id or positional ASIN(s) to upload those book(s).\n"
	+ "Books are never re-downloaded and local files are never deleted.")]
public class UploadOptions : ProcessableOptionsBase
{
	protected override async Task ProcessAsync()
	{
		if (AudibleFileStorage.BooksDirectory is null)
		{
			Console.Error.WriteLine("Error: Books directory is not set. Please configure the 'Books' setting in Settings.json.");
			return;
		}

		var configErrors = GetAudiobookshelfConfigurationErrors();
		if (configErrors.Count > 0)
		{
			Console.Error.WriteLine("Audiobookshelf is not fully configured:");
			foreach (var error in configErrors)
				Console.Error.WriteLine("  " + error);
			Console.Error.WriteLine();
			Console.Error.WriteLine("Set these under Settings -> Audiobookshelf in the main Libation app, then retry.");
			return;
		}

		var outcomes = new Dictionary<UploadToAudiobookshelf.UploadOutcome, int>();

		var uploader = CreateProcessable<UploadToAudiobookshelf>();

		// Failures arrive here rather than through StatusHandler: an upload problem must never mark
		// the book as a bad book. See UploadToAudiobookshelf.OutcomeDetermined.
		uploader.OutcomeDetermined += (_, e) =>
		{
			outcomes[e.Outcome] = outcomes.GetValueOrDefault(e.Outcome) + 1;

			if (e.Outcome is UploadToAudiobookshelf.UploadOutcome.Failed or UploadToAudiobookshelf.UploadOutcome.NoFilesFound)
				Console.Error.WriteLine(e.Message);
		};

		// RunAsync invokes this per candidate book, before processing. Books rejected by Validate on a
		// targeted run never reach ProcessAsync and so raise no outcome; counting here lets the summary
		// account for them instead of silently omitting them.
		var candidates = 0;
		await RunAsync(uploader, _ => candidates++);

		PrintSummary(outcomes, candidates);
	}

	/// <summary>
	/// Without this check every book fails <see cref="UploadToAudiobookshelf.Validate"/> and the
	/// run looks like a silent no-op.
	/// </summary>
	private static List<string> GetAudiobookshelfConfigurationErrors()
	{
		var config = Configuration.Instance;
		var errors = new List<string>();

		if (!config.AudiobookshelfEnabled)
			errors.Add("Audiobookshelf upload is turned off.");
		if (string.IsNullOrWhiteSpace(config.AudiobookshelfServerUrl))
			errors.Add("Server URL is not set.");
		if (string.IsNullOrWhiteSpace(config.AudiobookshelfApiToken))
			errors.Add("API token is not set.");
		if (string.IsNullOrWhiteSpace(config.AudiobookshelfLibraryId))
			errors.Add("No library is selected.");
		if (string.IsNullOrWhiteSpace(config.AudiobookshelfFolderId))
			errors.Add("No folder is selected.");

		return errors;
	}

	private static void PrintSummary(Dictionary<UploadToAudiobookshelf.UploadOutcome, int> outcomes, int candidates)
	{
		int count(UploadToAudiobookshelf.UploadOutcome outcome) => outcomes.GetValueOrDefault(outcome);

		// Anything counted as a candidate but never processed was rejected before ProcessAsync ran.
		var skipped = int.Max(0, candidates - outcomes.Values.Sum());

		Console.WriteLine();
		Console.WriteLine("Audiobookshelf upload summary:");
		Console.WriteLine($"  uploaded:          {count(UploadToAudiobookshelf.UploadOutcome.Uploaded)}");
		Console.WriteLine($"  already on server: {count(UploadToAudiobookshelf.UploadOutcome.AlreadyExists)}");
		Console.WriteLine($"  no files found:    {count(UploadToAudiobookshelf.UploadOutcome.NoFilesFound)}");
		Console.WriteLine($"  failed:            {count(UploadToAudiobookshelf.UploadOutcome.Failed)}");
		Console.WriteLine($"  skipped:           {skipped}");
	}
}
