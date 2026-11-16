using LibationUiBase.Resources;

namespace LibationUiBase;

/// <summary>Describe logging evidence without treating a missing fallback filename as a failed write.</summary>
public static class CrashLogMessage
{
	public static string Describe(string? crashLogFile, bool submittedToLogger, string? currentLogFile)
	{
		if (crashLogFile is not null)
			return string.Format(Strings.CrashLogMessage_TheErrorWasWrittenToPlease, crashLogFile);

		if (submittedToLogger)
			return string.IsNullOrWhiteSpace(currentLogFile)
				? Strings.CrashLogMessage_TheErrorWasSubmittedToLibation
				: string.Format(Strings.CrashLogMessage_TheErrorWasSubmittedToLibation2, currentLogFile);

		return Strings.CrashLogMessage_LibationCouldNotConfirmThatThis;
	}
}
