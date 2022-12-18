using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Dinah.Core;

namespace LibationFileManager
{
    public partial class Configuration
    {
        public static string AppDir_Relative => $@".{Path.PathSeparator}{LIBATION_FILES_KEY}";
        public static string AppDir_Absolute => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Exe.FileLocationOnDisk), LIBATION_FILES_KEY));
        public static string MyDocs => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Libation"));
        public static string WinTemp => Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Libation"));
        public static string UserProfile => Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Libation"));

        public enum KnownDirectories
        {
            None = 0,

            [Description("My Users folder")]
            UserProfile = 1,

            [Description("The same folder that Libation is running from")]
            AppDir = 2,

            [Description("System temporary folder")]
            WinTemp = 3,

            [Description("My Documents")]
            MyDocs = 4,

            [Description("Your settings folder (aka: Libation Files)")]
            LibationFiles = 5
        }
        // use func calls so we always get the latest value of LibationFiles
        private static List<(KnownDirectories directory, Func<string> getPathFunc)> directoryOptionsPaths { get; } = new()
        {
            (KnownDirectories.None, () => null),
            (KnownDirectories.UserProfile, () => UserProfile),
            (KnownDirectories.AppDir, () => AppDir_Relative),
            (KnownDirectories.WinTemp, () => WinTemp),
            (KnownDirectories.MyDocs, () => MyDocs),
			// this is important to not let very early calls try to accidentally load LibationFiles too early.
			// also, keep this at bottom of this list
			(KnownDirectories.LibationFiles, () => libationFilesPathCache)
        };
        public static string GetKnownDirectoryPath(KnownDirectories directory)
        {
            var dirFunc = directoryOptionsPaths.SingleOrDefault(dirFunc => dirFunc.directory == directory);
            return dirFunc == default ? null : dirFunc.getPathFunc();
        }
        public static KnownDirectories GetKnownDirectory(string directory)
        {
            // especially important so a very early call doesn't match null => LibationFiles
            if (string.IsNullOrWhiteSpace(directory))
                return KnownDirectories.None;

            // 'First' instead of 'Single' because LibationFiles could match other directories. eg: default value of LibationFiles == UserProfile.
            // since it's a list, order matters and non-LibationFiles will be returned first
            var dirFunc = directoryOptionsPaths.FirstOrDefault(dirFunc => dirFunc.getPathFunc() == directory);
            return dirFunc == default ? KnownDirectories.None : dirFunc.directory;
        }
    }
}
