using Dinah.Core;
using LibationFileManager;
using SixLabors.ImageSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WindowsConfigApp;

internal class WinInterop : IInteropFunctions
{
	public WinInterop() { }
	public WinInterop(params object[] values) { }
	public void SetFolderIcon(string image, string directory)
	{
		using var img = Image.Load(image);
		var icon = img.ToIcon();
		new DirectoryInfo(directory)?.SetIcon(icon, "Music");
	}

	public void SetFolderIcon(byte[] imageJpegBytes, string directory)
	{
		using var img = Image.Load(new MemoryStream(imageJpegBytes, writable: false));
		var icon = img.ToIcon();
		new DirectoryInfo(directory)?.SetIcon(icon, "Music");
	}

	public void DeleteFolderIcon(string directory)
		=> new DirectoryInfo(directory)?.DeleteIcon();

	public bool CanUpgrade => true;

	public string ReleaseIdString => AppScaffolding.LibationScaffolding.ReleaseIdentifier.ToString();

	public async Task InstallUpgradeAsync(string upgradeBundle)
	{
		const string ExtractorExeName = "ZipExtractor.exe";
		var thisExe = Environment.ProcessPath;
		var thisDir = Path.GetDirectoryName(thisExe);
		if (!File.Exists(thisExe) || !Directory.Exists(thisDir))
			return;

		var zipExtractor = Path.Combine(Path.GetTempPath(), ExtractorExeName);

		File.Copy(Path.Combine(thisDir, ExtractorExeName), zipExtractor, overwrite: true);

		var args =
			$"--input {upgradeBundle.SurroundWithQuotes()} " +
			$"--output {thisDir.SurroundWithQuotes()} " +
			$"--executable {thisExe.SurroundWithQuotes()}";

		var proc = StartProcess(zipExtractor, args, elevate: !IsDirectoryWritable(thisDir));
		if (proc is null)
			throw new InvalidOperationException("Could not start the upgrade process.");
		await proc.WaitForExitAsync();
		if (proc.ExitCode != 0)
			throw new InvalidOperationException($"ZipExtractor exited with code {proc.ExitCode}.");

		TrySyncInstallMetadata();
	}

	public void TrySyncInstallMetadata() => WindowsUninstallRegistrySync.TrySync();

	public Process? RunAsRoot(string exe, string args) => StartProcess(exe, args, elevate: true);

	private static Process? StartProcess(string exe, string args, bool elevate)
	{
		var psi = new ProcessStartInfo
		{
			FileName = exe,
			UseShellExecute = true,
			WindowStyle = ProcessWindowStyle.Normal,
			CreateNoWindow = true,
			Arguments = args
		};

		if (elevate)
			psi.Verb = "runas";

		return Process.Start(psi);
	}

	private static bool IsDirectoryWritable(string directory)
	{
		try
		{
			var probe = Path.Combine(directory, $".libation-write-test-{Guid.NewGuid():N}");
			using (File.Create(probe, 1, FileOptions.DeleteOnClose)) { }
			return true;
		}
		catch (UnauthorizedAccessException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
	}
}
