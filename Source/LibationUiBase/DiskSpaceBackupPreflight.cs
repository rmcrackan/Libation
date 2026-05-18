using LibationFileManager;
using LibationUiBase.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationUiBase;

/// <summary>
/// Optional bulk-backup gate before titles are queued. Skipped for single-book backups.
/// Network paths that do not report free space pass through with no dialog; failures are handled at download time.
/// </summary>
public static class DiskSpaceBackupPreflight
{
	private const int BulkBackupBookThreshold = 2;

	public static async Task<bool> ConfirmBulkBackupAsync(int bookCount, Configuration config)
	{
		if (bookCount < BulkBackupBookThreshold)
			return true;

		var drives = DiskSpaceHelper.GetBackupDriveSpaces(config, bookCount);

		// All roots unknown, or all have enough reported space: queue without prompting.
		if (DiskSpaceHelper.HasSufficientSpaceForBulkBackup(drives))
			return true;

		// Known space below CriticalFreeBytes: do not offer Continue (avoids starting a huge queue on a full local disk).
		if (DiskSpaceHelper.AnyDriveCriticallyLow(drives))
		{
			await MessageBoxBase.Show(
				DiskFullUserMessage.BuildPreflightBlockedBody(drives, bookCount),
				DiskFullUserMessage.DialogCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
			return false;
		}

		// At least one root reported space below estimate but above critical (or mixed known/unknown with a shortfall).
		var result = await MessageBoxBase.Show(
			DiskFullUserMessage.BuildPreflightWarningBody(drives, bookCount),
			DiskFullUserMessage.DialogCaption,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning,
			MessageBoxDefaultButton.Button2);

		return result == DialogResult.Yes;
	}
}
