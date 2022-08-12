using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dinah.Core;

namespace AppScaffolding.OSInterop
{
    public class OSInteropProxy : IInteropFunctions
    {
        public static bool IsWindows { get; } = OperatingSystem.IsWindows();
        public static bool IsLinux { get; } = OperatingSystem.IsLinux();
        public static bool IsMacOs { get; } = OperatingSystem.IsMacOS();

        public static Func<string, bool> MatchesOS { get; }
            = IsWindows ? a => Path.GetFileName(a).StartsWithInsensitive("win")
            : IsLinux ? a => Path.GetFileName(a).StartsWithInsensitive("linux")
            : IsMacOs ? a => Path.GetFileName(a).StartsWithInsensitive("mac") || a.StartsWithInsensitive("osx")
            : _ => false;

        public IInteropFunctions InteropFunctions { get; } = new NullInteropFunctions();

        #region Singleton Stuff

        private const string CONFIG_APP_ENDING = "ConfigApp.exe";
        private static List<ProcessModule> ModuleList { get; } = new();
        public static Type InteropFunctionsType { get; }
        static OSInteropProxy()
        {
            // searches file names for potential matches; doesn't run anything
            var configApp = getOSConfigApp();

            // nothing to load
            if (configApp is null)
                return;

            // runs the exe and gets the exe's loaded modules
            ModuleList = LoadModuleList(Path.GetFileNameWithoutExtension(configApp))
                .OrderBy(x => x.ModuleName)
                .ToList();

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var configAppAssembly = Assembly.LoadFrom(Path.ChangeExtension(configApp, "dll"));
            var type = typeof(IInteropFunctions);
            InteropFunctionsType = configAppAssembly
                .GetTypes()
                .FirstOrDefault(t => type.IsAssignableFrom(t));
        }
        private static string getOSConfigApp()
        {
            var here = Path.GetDirectoryName(Environment.ProcessPath);

            // find '*ConfigApp.exe' files
            var exes =
                Directory.EnumerateFiles(here, $"*{CONFIG_APP_ENDING}", SearchOption.TopDirectoryOnly)
                // sanity check. shouldn't ever be true
                .Except(new[] { Environment.ProcessPath })
                .Where(exe =>
                    // has a corresponding dll
                    File.Exists(Path.ChangeExtension(exe, "dll"))
                    && MatchesOS(exe)
                )
                .ToList();
            var exeName = exes.FirstOrDefault();
            return exeName;
        }

        private static List<ProcessModule> LoadModuleList(string exeName)
        {
            var proc = new Process
            {
                StartInfo = new()
                {
                    FileName = exeName,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false
                }
            };

            var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            proc.OutputDataReceived += (_, _) => waitHandle.Set();
            proc.Start();
            proc.BeginOutputReadLine();

            //Let the win process know we're ready to receive its standard output
            proc.StandardInput.WriteLine();

            if (!waitHandle.WaitOne(2000))
                throw new Exception("Failed to start program");

            //The win process has finished loading and is now waiting inside Main().
            //Copy it process module list.
            var modules = proc.Modules.Cast<ProcessModule>().ToList();

            //Let the win process know we're done reading its module list
            proc.StandardInput.WriteLine();

            return modules;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // e.g. "System.Windows.Forms, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            var asmName = args.Name.Split(',')[0];

            // `First` instead of `FirstOrDefault`. If it's not present we're going to fail anyway. May as well be here
            var module = ModuleList.First(m => m.ModuleName.StartsWith(asmName));

            return Assembly.LoadFrom(module.FileName);
        }

        #endregion

        public OSInteropProxy() : this(new object[0]) { }

        //// example of the pattern which could be useful later
        //public OSInteropProxy(string str, int i) : this(new object[] { str, i }) { }

        private OSInteropProxy(params object[] values)
        {
            if (InteropFunctionsType is null)
                return;

            InteropFunctions =
                values is null || values.Length == 0
                ? Activator.CreateInstance(InteropFunctionsType) as IInteropFunctions
                : Activator.CreateInstance(InteropFunctionsType, values) as IInteropFunctions;
        }

        // Interface Members
        /*
        // examples until the real interface is filled out
        public void CopyTextToClipboard(string text) => InteropFunctions.CopyTextToClipboard(text);
        public void ShowForm() => InteropFunctions.ShowForm();
        public string TransformInit1() => InteropFunctions.TransformInit1();
        public int TransformInit2() => InteropFunctions.TransformInit2();
        */
    }
}
