using AudibleUtilities;
using CommandLine;
using System;
using System.IO;
using System.Threading.Tasks;

namespace LibationCli;

[Verb("export-master-key", HelpText = "Export Libation's OS-bound encryption master key to a file for portable use (e.g. Docker). The file unlocks encrypted AccountsSettings.json.")]
internal class ExportMasterKeyOptions : OptionsBase
{
	[Value(0, MetaName = "path", Required = false, HelpText = "Destination path for the raw master key file (e.g. libation-master.key).")]
	public string? KeyFilePath { get; set; }

	[Option('p', "path", HelpText = "Destination path for the raw master key file. Alternative to the positional path argument.")]
	public string? PathOption { get; set; }

	protected override Task ProcessAsync()
	{
		var path = (PathOption ?? KeyFilePath)?.Trim();
		if (string.IsNullOrEmpty(path))
		{
			PrintVerbUsage("ERROR", "=====", "Path to the master key file is required.");
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return Task.CompletedTask;
		}

		try
		{
			MasterKeyExport.ExportToFile(path);
		}
		catch (Exception ex)
		{
			PrintVerbUsage("ERROR", "=====", ex.Message);
			Environment.ExitCode = (int)ExitCode.RunTimeError;
			return Task.CompletedTask;
		}

		Console.WriteLine($"Wrote master key to: {Path.GetFullPath(path)}");
		Console.WriteLine("Treat this file like a password: anyone with it can decrypt AccountsSettings.json tokens.");
		Console.WriteLine("For Docker, copy it next to AccountsSettings.json as libation-master.key, or set LIBATION_MASTER_KEY_FILE.");
		return Task.CompletedTask;
	}
}
