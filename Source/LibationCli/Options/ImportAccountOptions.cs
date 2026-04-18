using AudibleUtilities;
using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("import-account", HelpText = "Import an Audible account from an mkb79/audible-cli JSON export file.")]
internal class ImportAccountOptions : OptionsBase
{
	[Value(0, MetaName = "path", Required = true, HelpText = "Path to the exported account JSON file.")]
	public string? JsonFilePath { get; set; }

	protected override async Task ProcessAsync()
	{
		var path = JsonFilePath?.Trim();
		if (string.IsNullOrEmpty(path))
		{
			PrintVerbUsage("ERROR", "=====", "Path to JSON file is required.");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		if (!File.Exists(path))
		{
			PrintVerbUsage("ERROR", "=====", $"File not found: {path}");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		string jsonText;
		try
		{
			jsonText = await File.ReadAllTextAsync(path);
		}
		catch (Exception ex)
		{
			PrintVerbUsage("ERROR", "=====", $"Could not read file: {ex.Message}");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		Mkb79ImportResult result;
		try
		{
			result = await Mkb79AuthImporter.ImportFromJsonTextAsync(jsonText);
		}
		catch (Exception ex)
		{
			PrintVerbUsage("ERROR", "=====", ex.Message, "", ex.StackTrace);
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return;
		}

		switch (result.Outcome)
		{
			case Mkb79ImportOutcome.InvalidFile:
				Console.Error.WriteLine(result.Message ?? "Invalid import file.");
				Environment.ExitCode = (int)ExitCode.RunTimeError;
				return;
			case Mkb79ImportOutcome.DuplicateAccount when result.Account is { } dup:
				Console.Error.WriteLine(
					$"An account with that account id and country already exists.{Environment.NewLine}"
					+ $"Account ID: {dup.AccountId}{Environment.NewLine}Country: {dup.Locale?.Name}");
				Environment.ExitCode = (int)ExitCode.RunTimeError;
				return;
			case Mkb79ImportOutcome.Success when result.Account is { } account:
				Console.WriteLine(
					$"Imported account: {account.AccountName} ({account.AccountId}, {account.Locale?.Name})");
				return;
			default:
				Console.Error.WriteLine("Unexpected import result.");
				Environment.ExitCode = (int)ExitCode.RunTimeError;
				return;
		}
	}
}
