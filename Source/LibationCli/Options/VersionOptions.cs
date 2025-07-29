using AppScaffolding;
using CommandLine;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationCli.Options;

[Verb("version", HelpText = "Display version information.")]
internal class VersionOptions : OptionsBase
{
	[Option('c', "check", Required = false, HelpText = "Check if an upgrade is available")]
	public bool CheckForUpgrade { get; set; }

	protected override Task ProcessAsync()
	{
		const string checkingForUpgrade = "Checking for upgrade...";
		Console.WriteLine($"Libation {LibationScaffolding.Variety} v{LibationScaffolding.BuildVersion.ToVersionString()}");

		if (CheckForUpgrade)
		{
			Console.Write(checkingForUpgrade);

			var origColor = Console.ForegroundColor;
			try
			{
				var upgradeProperties = LibationScaffolding.GetLatestRelease();

				if (upgradeProperties is null)
				{
					Console.ForegroundColor = ConsoleColor.Green;
					ReplaceConsoleText(Console.Out, checkingForUpgrade.Length, "No available upgrade");
					Console.WriteLine();
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					ReplaceConsoleText(Console.Out, checkingForUpgrade.Length, $"Upgrade Available: v{upgradeProperties.LatestRelease.ToVersionString()}");
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine(upgradeProperties.ZipUrl);
					Console.WriteLine();
					Console.WriteLine("Release Notes");
					Console.WriteLine("=============");
					Console.WriteLine(upgradeProperties.Notes);
				}
			}
			catch
			{
				Console.Error.WriteLine("ERROR CHECKING FOR UPGRADE");
			}
			finally
			{
				Console.ForegroundColor = origColor;
			}
		}

		return Task.CompletedTask;
	}
}
