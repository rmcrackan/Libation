using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Dinah.Core;

namespace LibationFileManager
{
    public static class InteropFactory
    {
        public static Type InteropFunctionsType { get; }

        public static IInteropFunctions Create() => _create();

        //// examples of the pattern which could be useful later
        //public static IInteropFunctions Create(string str, int i) => _create(str, i);
        //public static IInteropFunctions Create(params object[] values) => _create(values);

        private static IInteropFunctions instance { get; set; }
        private static IInteropFunctions _create(params object[] values)
        {
            instance ??=
                InteropFunctionsType is null
                ? new NullInteropFunctions()
                //: values is null || values.Length == 0 ? Activator.CreateInstance(InteropFunctionsType) as IInteropFunctions
                : Activator.CreateInstance(InteropFunctionsType, values) as IInteropFunctions;
            return instance;
        }

        #region load types

        public static Func<string, bool> MatchesOS { get; }
            = Configuration.IsWindows ? a => Path.GetFileName(a).StartsWithInsensitive("win")
            : Configuration.IsLinux ? a => Path.GetFileName(a).StartsWithInsensitive("linux")
            : Configuration.IsMacOs ? a => Path.GetFileName(a).StartsWithInsensitive("mac") || Path.GetFileName(a).StartsWithInsensitive("osx")
            : _ => false;

        private const string CONFIG_APP_ENDING = "ConfigApp.dll";
        private static List<ProcessModule> ModuleList { get; } = new();
        static InteropFactory()
        {
            // searches file names for potential matches; doesn't run anything
            var configApp = getOSConfigApp();

            // nothing to load
            if (configApp is null)
            {
                Serilog.Log.Logger.Error($"Unable to locate *{CONFIG_APP_ENDING}");
                return;
            }

            /*
            * Commented code used to locate assemblies from the *ConfigApp.exe's module list.
            * Use this method to locate dependencies when they are not in Libation's program files directory.
#if DEBUG

           // runs the exe and gets the exe's loaded modules
           ModuleList = LoadModuleList(Path.GetFileNameWithoutExtension(configApp))
               .OrderBy(x => x.ModuleName)
               .ToList();
#endif
            */

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var configAppAssembly = Assembly.LoadFrom(configApp);
            var type = typeof(IInteropFunctions);
            InteropFunctionsType = configAppAssembly
                .GetTypes()
                .FirstOrDefault(t => type.IsAssignableFrom(t));
        }
        private static string getOSConfigApp()
        {
            var here = Path.GetDirectoryName(Environment.ProcessPath);

            // find '*ConfigApp.dll' files
            var appName =
                Directory.EnumerateFiles(here, $"*{CONFIG_APP_ENDING}", SearchOption.TopDirectoryOnly)
                // sanity check. shouldn't ever be true
                .Except(new[] { Environment.ProcessPath })
                .FirstOrDefault(exe => MatchesOS(exe));

            return appName;
        }

        /*
         * Use this method to locate dependencies when they are not in Libation's program files directory.
         * 
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
         */

        private static Dictionary<string, Assembly> lowEffortCache { get; } = new();
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // e.g. "System.Windows.Forms, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
            var asmName = args.Name.Split(',')[0] + ".dll";
            var here = Path.GetDirectoryName(Environment.ProcessPath);

            var key = $"{asmName}|{here}";

            if (lowEffortCache.TryGetValue(key, out var value))
                return value;

            var assembly = CurrentDomain_AssemblyResolve_internal(asmName: asmName, here: here);
            lowEffortCache[key] = assembly;

            //Let the runtime handle any dll not found exceptions.
            if (assembly is null)
                Serilog.Log.Logger.Error($"Unable to load module {args.Name}");

            return assembly;
        }

        private static Assembly CurrentDomain_AssemblyResolve_internal(string asmName, string here)
        {
            /*
             * Commented code used to locate assemblies from the *ConfigApp.exe's module list.
             * Use this method to locate dependencies when they are not in Libation's program files directory.
 #if DEBUG

            var modulePath = ModuleList.SingleOrDefault(m => m.ModuleName.EqualsInsensitive(asmName))?.FileName;
 #else
            */

            // find the requested assembly in the program files directory
            var modulePath =
                Directory.EnumerateFiles(here, asmName, SearchOption.TopDirectoryOnly)
                .SingleOrDefault();

//#endif
            if (modulePath is null)
                return null;

            return Assembly.LoadFrom(modulePath);
        }

        #endregion
    }
}
