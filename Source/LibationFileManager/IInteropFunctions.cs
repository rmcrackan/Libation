using System;
using System.Diagnostics;
using System.Threading.Tasks;

#nullable enable
namespace LibationFileManager
{
	public interface IInteropFunctions
    {
        void SetFolderIcon(string image, string directory);
        void DeleteFolderIcon(string directory);
        Process RunAsRoot(string exe, string args);
        void InstallUpgrade(string upgradeBundle);
        bool CanUpgrade { get; }
		string ReleaseIdString { get; }
    }

}
