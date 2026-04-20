using System.Diagnostics;
using System.Threading.Tasks;

namespace LibationFileManager;

public interface IInteropFunctions
{
	void SetFolderIcon(string image, string directory);
	void SetFolderIcon(byte[] imageJpegBytes, string directory);
	void DeleteFolderIcon(string directory);
	Process? RunAsRoot(string exe, string args);
	/// <summary>Waits for the privileged installer where possible and throws if it fails.</summary>
	Task InstallUpgradeAsync(string upgradeBundle);
	bool CanUpgrade { get; }
	string ReleaseIdString { get; }
}
