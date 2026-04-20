using AppScaffolding;
using LibationFileManager;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LinuxConfigApp;

internal class LinuxInterop : IInteropFunctions
{
	//Different terminal apps possibly installed on a linux system
	// [0] console executable
	// [1] argument to set the concole's title
	// [2] argument to pass a command to be executed to the terminal
	static readonly string[][] consoleCommands =
	{
		new[] {"konsole", "--title", "-e"},
		new[] {"gnome-terminal", "--title", "--"},
		new[] {"mate-terminal", "--title", "-x"},
		new[] {"xterm", "-T", "-e"},
	};

	public LinuxInterop() { }
	public LinuxInterop(params object[] values) { }

	public void SetFolderIcon(string image, string directory) => throw new PlatformNotSupportedException();
	public void SetFolderIcon(byte[] imageJpegBytes, string directory) => throw new PlatformNotSupportedException();
	public void DeleteFolderIcon(string directory) => throw new PlatformNotSupportedException();

	public string ReleaseIdString => LibationScaffolding.ReleaseIdentifier.ToString() + (File.Exists("/usr/bin/apt") || File.Exists("/bin/apt") ? "_DEB" : "_RPM");

	//only run the auto upgrader if the current app was installed from the
	//.deb or .rpm package. Try to detect this by checking if the symlink exists.
	public bool CanUpgrade => File.Exists("/usr/bin/libation") || File.Exists("/bin/libation");

	public async Task InstallUpgradeAsync(string upgradeBundle)
	{
		if (string.IsNullOrWhiteSpace(upgradeBundle) || !File.Exists(upgradeBundle))
			throw new FileNotFoundException("Upgrade bundle not found.", upgradeBundle);

		if (!TryResolvePackageManager(upgradeBundle, out var pkgExe, out var pkgArgs))
			throw new PlatformNotSupportedException("Could not find apt, dnf, yum, or dnf5 to install the upgrade.");

		if (FindPkexec(out var pkexec))
		{
			var psi = new ProcessStartInfo
			{
				FileName = pkexec,
				UseShellExecute = false,
			};
			// pkexec requires an absolute path to the program on modern polkit.
			foreach (var a in new[] { pkgExe }.Concat(pkgArgs))
				psi.ArgumentList.Add(a);

			if (string.Equals(Path.GetFileName(pkgExe), "apt", StringComparison.OrdinalIgnoreCase))
				psi.Environment["DEBIAN_FRONTEND"] = "noninteractive";

			var proc = Process.Start(psi);
			if (proc is null)
				throw new InvalidOperationException("Failed to start pkexec.");

			await proc.WaitForExitAsync();
			if (proc.ExitCode != 0)
				throw new InvalidOperationException($"Package manager exited with code {proc.ExitCode}.");
			return;
		}

		// Terminal + runasroot.sh: completion cannot be tracked reliably; user must watch the window.
		var legacyArgs = string.Join(" ", pkgArgs);
		Serilog.Log.Logger.Warning("pkexec not found; launching install in a terminal. Completion cannot be verified automatically.");
		RunAsRoot(pkgExe, legacyArgs);
	}

	private static bool TryResolvePackageManager(string upgradeBundle, out string pkgExe, out string[] pkgArgs)
	{
		if (TryFirstExisting(out pkgExe, "/usr/bin/dnf5", "/bin/dnf5"))
		{
			pkgArgs = new[] { "install", "-y", upgradeBundle };
			return true;
		}
		if (TryFirstExisting(out pkgExe, "/usr/bin/dnf", "/bin/dnf"))
		{
			pkgArgs = new[] { "install", "-y", upgradeBundle };
			return true;
		}
		if (TryFirstExisting(out pkgExe, "/usr/bin/yum", "/bin/yum"))
		{
			pkgArgs = new[] { "install", "-y", upgradeBundle };
			return true;
		}
		if (TryFirstExisting(out pkgExe, "/usr/bin/apt", "/bin/apt"))
		{
			pkgArgs = new[] { "install", "-y", "-o", "Dpkg::Options::=--force-confdef", "-o", "Dpkg::Options::=--force-confold", upgradeBundle };
			return true;
		}
		pkgExe = "";
		pkgArgs = Array.Empty<string>();
		return false;
	}

	private static bool TryFirstExisting(out string path, params string[] candidates)
	{
		foreach (var c in candidates)
		{
			if (File.Exists(c))
			{
				path = c;
				return true;
			}
		}
		path = "";
		return false;
	}

	private bool FindPkexec(out string? exePath)
	{
		if (File.Exists("/usr/bin/pkexec"))
		{
			exePath = "/usr/bin/pkexec";
			return true;
		}
		if (File.Exists("/bin/pkexec"))
		{
			exePath = "/bin/pkexec";
			return true;
		}
		exePath = null;
		return false;
	}

	public Process? RunAsRoot(string exe, string args)
	{
		//cribbed this script from VirtualBox's guest additions installer.
		//It's designed to launch the system's gui superuser password
		//prompt across multiple distributions and desktop environments.
		const string runasroot = "runasroot.sh";

		string command = $"{exe ?? ""} {args ?? ""}".Trim();

		foreach (var console in consoleCommands)
		{
			ProcessStartInfo psi = new()
			{
				FileName = console[0],
				UseShellExecute = false,
				ArgumentList =
				{
					console[1],
					$"Running '{exe}' as root", // console title
					console[2],
					"/bin/sh",
					Path.Combine(Configuration.ProcessDirectory, runasroot), //script file
					"Installing libation.deb", //command title
					command, // command to execute vis /bin/sh
					$"Please run '{command}' manually" // error message to display in the terminal
				}
			};

			try
			{
				return Process.Start(psi);
			}
			catch { }
		}
		throw new PlatformNotSupportedException($"Could not start any of the supported terminals: {string.Join(", ", consoleCommands.Select(c => c[0]))}");
	}
}
