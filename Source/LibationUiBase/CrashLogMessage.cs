namespace LibationUiBase;

/// <summary>Describe logging evidence without treating a missing fallback filename as a failed write.</summary>
public static class CrashLogMessage
{
	public static string Describe(string? crashLogFile, bool submittedToLogger, string? currentLogFile)
	{
		if (crashLogFile is not null)
			return $"""
				The error was written to:
				{crashLogFile}
				Please attach that file when reporting this issue.
				""";

		if (submittedToLogger)
			return string.IsNullOrWhiteSpace(currentLogFile)
				? """
					The error was submitted to Libation's logger.
					Please attach the current Libation log and include the text below.
					"""
				: $"""
					The error was submitted to Libation's logger.
					Please attach the current log:
					{currentLogFile}
					Please also include the text below.
					""";

		return """
			Libation could not confirm that this error was saved.
			Please include the text below when reporting this issue.
			""";
	}
}
