using LibationFileManager;
using LibationUiBase.Forms;
using System.Threading.Tasks;

namespace LibationUiBase;

/// <summary>
/// Optional bulk-backup gate before titles are queued. Skipped for single-book backups.
/// Network paths that do not report free space pass through with no dialog; failures are handled at download time.
/// </summary>
public static class DiskSpaceBackupPreflight
{
	private const int BulkBackupBookThreshold = 2;

	private static bool bulkPreflightConfirmedForQueueRun;

	/// <summary>Clears the per-queue-run preflight flag when the backup queue finishes.</summary>
	public static void ResetBulkPreflightForQueueRun() => bulkPreflightConfirmedForQueueRun = false;

	public static async Task<bool> ConfirmBulkBackupAsync(
		int bookCount,
		Configuration config,
		bool backupQueueAlreadyRunning = false)
	{
		if (bookCount < BulkBackupBookThreshold
			|| backupQueueAlreadyRunning
			|| bulkPreflightConfirmedForQueueRun)
			return true;

		var drives = DiskSpaceHelper.GetBackupDriveSpaces(config, bookCount);

		if (DiskSpaceHelper.HasSufficientSpaceForBulkBackup(drives))
		{
			bulkPreflightConfirmedForQueueRun = true;
			return true;
		}

		if (DiskSpaceHelper.AnyDriveCriticallyLow(drives))
		{
			await MessageBoxBase.Show(
				DiskFullUserMessage.BuildPreflightBlockedBody(drives, bookCount),
				DiskFullUserMessage.DialogCaption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
			return false;
		}

		var result = await MessageBoxBase.Show(
			DiskFullUserMessage.BuildPreflightWarningBody(drives, bookCount),
			DiskFullUserMessage.DialogCaption,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Warning,
			MessageBoxDefaultButton.Button2);

		if (result == DialogResult.Yes)
			bulkPreflightConfirmedForQueueRun = true;

		return result == DialogResult.Yes;
	}
}
