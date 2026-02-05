using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Dinah.Core;

namespace LibationFileManager;

public static class InteropFactory
{
	public static Type? InteropFunctionsType { get; }

	public static IInteropFunctions Create() => _create();

	//// examples of the pattern which could be useful later
	//public static IInteropFunctions Create(string str, int i) => _create(str, i);
	//public static IInteropFunctions Create(params object[] values) => _create(values);

	private static IInteropFunctions? instance { get; set; }
	private static IInteropFunctions _create(params object[] values)
	{
		instance ??=
			InteropFunctionsType is null
			? new NullInteropFunctions()
			: Activator.CreateInstance(InteropFunctionsType, values) as IInteropFunctions;

		if (instance is null)
			throw new TypeLoadException();

		return instance;
	}

	#region load types

	private const string CONFIG_APP_ENDING = "ConfigApp.dll";

	public static Func<string, bool> MatchesOS { get; }
	= Configuration.IsWindows ? a => Path.GetFileName(a).StartsWithInsensitive("win")
	: Configuration.IsLinux ? a => Path.GetFileName(a).StartsWithInsensitive("linux")
	: Configuration.IsMacOs ? a => Path.GetFileName(a).StartsWithInsensitive("mac") || Path.GetFileName(a).StartsWithInsensitive("osx")
	: _ => false;

	private static readonly EnumerationOptions enumerationOptions = new()
	{
		MatchType = MatchType.Simple,
		MatchCasing = MatchCasing.CaseInsensitive,
		IgnoreInaccessible = true,
		RecurseSubdirectories = false,
		ReturnSpecialDirectories = false
	};

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

		AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

		try
		{
			var configAppAssembly = Assembly.LoadFrom(configApp);
			var type = typeof(IInteropFunctions);
			InteropFunctionsType = configAppAssembly
				.GetTypes()
				.FirstOrDefault(type.IsAssignableFrom);
		}
		catch (Exception e)
		{
			//None of the interop functions are strictly necessary for Libation to run.
			Serilog.Log.Logger.Error(e, "Unable to load types from assembly {configApp}", configApp);
		}
	}
	private static string? getOSConfigApp()
	{
		// find '*ConfigApp.dll' files
		var appName =
			Directory.EnumerateFiles(Configuration.ProcessDirectory, $"*{CONFIG_APP_ENDING}", enumerationOptions)
			.FirstOrDefault(exe => MatchesOS(exe));

		return appName;
	}

	private static Dictionary<string, Assembly?> lowEffortCache { get; } = new();
	private static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
	{
		var asmName = new AssemblyName(args.Name);
		var here = Configuration.ProcessDirectory;

		var key = $"{asmName}|{here}";

		if (lowEffortCache.TryGetValue(key, out var value))
			return value;

		var assembly = CurrentDomain_AssemblyResolve_internal(asmName, here: here);
		lowEffortCache[key] = assembly;

		// Let the runtime handle any dll not found exceptions
		if (assembly is null)
			Serilog.Log.Logger.Warning($"Unable to load module {args.Name}");

		return assembly;
	}

	private static Assembly? CurrentDomain_AssemblyResolve_internal(AssemblyName asmName, string here)
	{
		/*
		 * Find the requested assembly in the program files directory.
		 * Assumes that all assemblies are in this application's directory.
		 * If they're not (e.g. the app is not self-contained), you will need
		 * to located them. The original way of doing this was to execute the
		 * config app, wait for the runtime to load all dependencies, and
		 * then seach the Process.Modules for the assembly name. Code for
		 * this approach is still in the _Demos projects.
		 */
		var modulePath =
			Directory.EnumerateFiles(here, $"{asmName.Name}.dll", enumerationOptions)
			.SingleOrDefault();

		return modulePath is null ? null : Assembly.LoadFrom(modulePath);
	}

	#endregion
}
