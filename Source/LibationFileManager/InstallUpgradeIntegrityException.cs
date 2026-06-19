using System;

namespace LibationFileManager;

/// <summary>Thrown when an in-app upgrade overlay did not update all required install files.</summary>
public sealed class InstallUpgradeIntegrityException : Exception
{
	public InstallUpgradeIntegrityException(string message) : base(message) { }

	public InstallUpgradeIntegrityException(string message, Exception innerException) : base(message, innerException) { }
}
