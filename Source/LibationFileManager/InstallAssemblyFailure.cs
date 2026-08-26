using System;

namespace LibationFileManager;

/// <summary>
/// An assembly the runtime could not bind to a usable file in Libation's install folder, with the version
/// the build was compiled against and the version actually on disk (null when the file is not there).
/// </summary>
public sealed record InstallAssemblyFailure(
	string AssemblyName,
	Version RequestedVersion,
	Version? InstalledVersion,
	string ExpectedPath)
{
	public string FileName => System.IO.Path.GetFileName(ExpectedPath);
}
