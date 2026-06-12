using System.Diagnostics;
using System.Text;

namespace HangoverBase;

public static class HangoverDbMutation
{
	public const string ConfirmTitle = "Modify Database";

	public static string RemoveDuplicateAsinsDescription { get; }
		= "This one-time cleanup will back up the database, merge useful data into the kept row, and permanently delete duplicate Book + LibraryBook pairs.";

	public static string SqlMutatingDescription { get; }
		= "This will run a SQL command that modifies the database.";

	public static string RestoreDeletedBooksDescription { get; }
		= "This will restore the selected book(s) to your Libation library.";

	public static string PermanentlyDeleteBooksDescription { get; }
		= "This will permanently delete the selected book(s) from Libation.";

	public static bool IsLibationRunning()
	{
		try
		{
			var currentPid = Environment.ProcessId;
			foreach (var process in Process.GetProcessesByName("Libation"))
			{
				try
				{
					if (process.Id != currentPid && !process.HasExited)
						return true;
				}
				finally
				{
					process.Dispose();
				}
			}
		}
		catch
		{
			// Process enumeration can fail on some platforms; treat as unknown.
		}

		return false;
	}

	public static bool IsMutatingSql(string sql)
	{
		if (string.IsNullOrWhiteSpace(sql))
			return false;

		var trimmed = sql.Trim();
		while (trimmed.StartsWith("--", StringComparison.Ordinal))
		{
			var lineEnd = trimmed.IndexOf('\n');
			if (lineEnd < 0)
				return false;
			trimmed = trimmed[(lineEnd + 1)..].TrimStart();
		}

		var lower = trimmed.ToLowerInvariant();
		return lower.StartsWith("update")
			|| lower.StartsWith("insert")
			|| lower.StartsWith("delete");
	}

	public static string BuildConfirmMessage(string actionDescription)
	{
		var builder = new StringBuilder();
		builder.AppendLine(actionDescription);
		builder.AppendLine();

		if (IsLibationRunning())
			builder.AppendLine("Libation is currently running.");

		builder.AppendLine("Close Libation before continuing to avoid database conflicts and a stale library display.");
		builder.AppendLine();
		builder.Append("Proceed?");
		return builder.ToString().TrimEnd();
	}
}
