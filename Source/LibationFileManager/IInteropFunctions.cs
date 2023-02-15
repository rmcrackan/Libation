using System;
using System.Diagnostics;

namespace LibationFileManager
{
    public interface IInteropFunctions
    {
        void SetFolderIcon(string image, string directory);
        void DeleteFolderIcon(string directory);
        Process RunAsRoot(string exe, string args);
        void InstallUpdate(string updateBundle);
        bool CanUpdate { get; }
    }
}
