using Dinah.Core;
using LibationFileManager;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using System.IO;

namespace WindowsConfigApp;

internal class WinInterop : IInteropFunctions
{
	public WinInterop() { }
	public WinInterop(params object[] values) { }
	public void SetFolderIcon(string image, string directory)
	{
		var icon = Image.Load(image).ToIcon();
		new DirectoryInfo(directory)?.SetIcon(icon, "Music");
	}

	public void DeleteFolderIcon(string directory)
		=> new DirectoryInfo(directory)?.DeleteIcon();

	public bool CanUpgrade => true;

	public string ReleaseIdString => AppScaffolding.LibationScaffolding.ReleaseIdentifier.ToString();

	public void InstallUpgrade(string upgradeBundle)
	{
		const string ExtractorExeName = "ZipExtractor.exe";
		var thisExe = Environment.ProcessPath;
		var thisDir = Path.GetDirectoryName(thisExe);
		if (!File.Exists(thisExe) || !Directory.Exists(thisDir))
			return;

		var zipExtractor = Path.Combine(Path.GetTempPath(), ExtractorExeName);

		File.Copy(Path.Combine(thisDir, ExtractorExeName), zipExtractor, overwrite: true);

		RunAsRoot(zipExtractor,
				$"--input {upgradeBundle.SurroundWithQuotes()} " +
				$"--output {thisDir.SurroundWithQuotes()} " +
				$"--executable {thisExe.SurroundWithQuotes()}");
	}

	public Process? RunAsRoot(string exe, string args)
	{
		var psi = new ProcessStartInfo()
		{
			FileName = exe,
			UseShellExecute = true,
			Verb = "runas",
			WindowStyle = ProcessWindowStyle.Normal,
			CreateNoWindow = true,
			Arguments = args
		};

		return Process.Start(psi);
	}
}
