using System;
using System.Runtime.InteropServices;
using System.Text;

namespace LibationFileManager;

/// <summary>Whether a path is inside a cloud sync root, and which provider owns it if Windows says.</summary>
public readonly record struct CloudSyncStatus(bool IsSynced, string? ProviderName)
{
	public static readonly CloudSyncStatus NotSynced = new(false, null);

	/// <summary>The provider's own name where Windows reported one, otherwise a generic description.</summary>
	public string Description => ProviderName is { Length: > 0 } name ? name : "a cloud sync folder";
}

/// <summary>
/// Asks Windows whether a path sits under a registered cloud sync root. Every sync engine that
/// serves files on demand registers one, so this covers OneDrive, Dropbox, Google Drive, iCloud and
/// the rest without naming any of them, and without inferring anything from folder names.
///
/// It matters because sync clients dehydrate files into placeholders, restore old copies, and leave
/// conflict copies behind. Under an install folder that the upgrader rewrites in place, that undoes
/// part of an upgrade.
/// </summary>
public static class CloudSyncedFolders
{
	// cfapi.h. BASIC returns only a file id, which is all that is needed to answer "is this in a
	// sync root at all", and is a plain 8-byte value with no marshalling to get wrong.
	private const int CF_SYNC_ROOT_INFO_BASIC = 0;
	private const int CF_SYNC_ROOT_INFO_PROVIDER = 2;

	private const int CF_MAX_PROVIDER_NAME_LENGTH = 255;
	private const int CF_MAX_PROVIDER_VERSION_LENGTH = 255;

	// CF_SYNC_ROOT_BASIC_INFO is a single LARGE_INTEGER.
	private const int BasicInfoLength = 8;

	// CF_SYNC_ROOT_PROVIDER_INFO is a 4-byte status enum, then two fixed WCHAR arrays.
	private const int ProviderStatusLength = 4;
	private const int ProviderNameLengthInBytes = CF_MAX_PROVIDER_NAME_LENGTH * 2;
	private const int ProviderInfoLength = ProviderStatusLength + ProviderNameLengthInBytes + (CF_MAX_PROVIDER_VERSION_LENGTH * 2);

	public static CloudSyncStatus GetSyncStatus(string? path)
	{
		if (string.IsNullOrWhiteSpace(path) || !OperatingSystem.IsWindows())
			return CloudSyncStatus.NotSynced;

		try
		{
			return ReadWindowsSyncStatus(path);
		}
		catch (Exception ex)
		{
			// cldapi.dll is absent before Windows 10 1709, where there are no sync roots to find.
			Serilog.Log.Logger.Debug(ex, "Could not read cloud sync root information for {Path}", path);
			return CloudSyncStatus.NotSynced;
		}
	}

	private static CloudSyncStatus ReadWindowsSyncStatus(string path)
	{
		var basicInfo = new byte[BasicInfoLength];
		if (CfGetSyncRootInfoByPath(path, CF_SYNC_ROOT_INFO_BASIC, basicInfo, basicInfo.Length, out _) < 0)
			return CloudSyncStatus.NotSynced;

		return new CloudSyncStatus(true, TryReadProviderName(path));
	}

	/// <summary>The provider's name, or null when only the fact of syncing could be established.</summary>
	private static string? TryReadProviderName(string path)
	{
		var providerInfo = new byte[ProviderInfoLength];
		if (CfGetSyncRootInfoByPath(path, CF_SYNC_ROOT_INFO_PROVIDER, providerInfo, providerInfo.Length, out var returnedLength) < 0
			|| returnedLength < ProviderStatusLength + 2)
			return null;

		var available = Math.Min(ProviderNameLengthInBytes, returnedLength - ProviderStatusLength);
		var name = Encoding.Unicode.GetString(providerInfo, ProviderStatusLength, available);

		var terminator = name.IndexOf('\0');
		if (terminator >= 0)
			name = name[..terminator];

		return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
	}

	[DllImport("cldapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
	private static extern int CfGetSyncRootInfoByPath(
		string filePath,
		int infoClass,
		[Out] byte[] infoBuffer,
		int infoBufferLength,
		out int returnedLength);
}
