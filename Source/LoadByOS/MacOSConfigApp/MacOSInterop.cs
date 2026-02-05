using Dinah.Core;
using LibationFileManager;
using System.Diagnostics;

namespace MacOSConfigApp
{
    internal class MacOSInterop : IInteropFunctions
	{
		private const string AppPath = "/Applications/Libation.app";
		public MacOSInterop() { }
        public MacOSInterop(params object[] values) { }

		public void SetFolderIcon(string image, string directory)
		{
			Process.Start("fileicon", $"set {directory.SurroundWithQuotes()} {image.SurroundWithQuotes()}").WaitForExit();
		}
		public void DeleteFolderIcon(string directory)
		{
			Process.Start("fileicon", $"rm {directory.SurroundWithQuotes()}").WaitForExit();
		}

		//I haven't figured out how to find the app bundle's directory from within
		//the running process, so don't upgrade unless it's "installed" in /Applications
		public bool CanUpgrade => Directory.Exists(AppPath);

		public string ReleaseIdString => AppScaffolding.LibationScaffolding.ReleaseIdentifier.ToString();

		public void InstallUpgrade(string upgradeBundle)
		{
			Serilog.Log.Information($"Extracting upgrade bundle to {AppPath}");

			//Upgrade bundle is a DMG
			Process.Start("open", upgradeBundle.SurroundWithQuotes())?.WaitForExit();
		}

		//Using osascript -e '[script]' works from the terminal, but I haven't figured
		//out the syntax for it to work from create_process, so write to stdin instead.
		public Process? RunAsRoot(string _, string command)
		{
			const string osascript = "osascript";
			var fullCommand = $"do shell script \"{command}\" with administrator privileges";

			var psi = new ProcessStartInfo()
			{
				FileName = osascript,
				UseShellExecute = false,
				Arguments = "-",
				RedirectStandardError= true,
				RedirectStandardOutput= true,
				RedirectStandardInput= true,
			};

			Serilog.Log.Logger.Information($"running {osascript} as root: {{script}}", fullCommand);

			if (Process.Start(psi) is not { } proc)
				return null;
			proc.ErrorDataReceived += Proc_ErrorDataReceived;
			proc.OutputDataReceived += Proc_OutputDataReceived;
			proc.BeginErrorReadLine();
			proc.BeginOutputReadLine();
			proc.StandardInput.WriteLine(fullCommand);
			proc.StandardInput.Close();

			return proc;
		}

		private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data != null)
				Serilog.Log.Logger.Information("stderr: {data}", e.Data);
		}

		private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (e.Data!= null)
				Serilog.Log.Logger.Information("stderr: {data}", e.Data);
		}
	}
}
