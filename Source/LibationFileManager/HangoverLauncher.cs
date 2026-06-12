using System;
using System.Diagnostics;
using System.IO;

namespace LibationFileManager;

public static class HangoverLauncher
{
	public static string GetHangoverExecutablePath()
	{
		var fileName = Configuration.IsWindows ? "Hangover.exe" : "Hangover";
		return Path.Combine(Configuration.ProcessDirectory, fileName);
	}

	public static void Launch()
	{
		var path = GetHangoverExecutablePath();
		if (!File.Exists(path))
			throw new FileNotFoundException(
				$"Hangover was not found in Libation's folder.{Environment.NewLine}{Environment.NewLine}Expected:{Environment.NewLine}{path}",
				path);

		Process.Start(new ProcessStartInfo
		{
			FileName = path,
			WorkingDirectory = Configuration.ProcessDirectory,
			UseShellExecute = true,
		});
	}

	public static string GetLaunchFailureMessage(Exception ex)
		=> ex is FileNotFoundException fileNotFound
		? fileNotFound.Message
		: $"Could not start Hangover.{Environment.NewLine}{Environment.NewLine}{ex.Message}";
}
