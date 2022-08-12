using System;

namespace LibationFileManager
{
    public interface IInteropFunctions
    {
        void SetFolderIcon(string image, string directory);
        void DeleteFolderIcon(string directory);
    }
}
