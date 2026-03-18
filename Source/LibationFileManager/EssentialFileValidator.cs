using Serilog;
using System;
using System.IO;
using System.Threading;

namespace LibationFileManager;

/// <summary>
/// Validates that essential files were created correctly after creation, with retries to allow for OS delay.
/// Callers should use <see cref="ReportValidationFailure"/> to log errors and show the user when the log cannot be written.
/// </summary>
public static class EssentialFileValidator
{
	/// <summary>
	/// Called when an essential file validation fails and the error could not be written to the log.
	/// Set by the host (e.g. LibationAvalonia, LibationWinForms) to display the message to the user.
	/// </summary>
	public static Action<string>? ShowUserWhenLogUnavailable { get; set; }

	/// <summary>
	/// Default retry total duration (ms) when checking that a file is available after creation.
	/// </summary>
	public const int DefaultMaxRetriesMs = 1000;

	/// <summary>
	/// Default delay (ms) between retries.
	/// </summary>
	public const int DefaultDelayMs = 50;

	/// <summary>
	/// Validates that the file at <paramref name="path"/> exists and is readable and writable,
	/// with retries to allow for OS delay between create and availability.
	/// Error messages use the file name portion of <paramref name="path"/>.
	/// </summary>
	/// <param name="path">Full path to the file.</param>
	/// <param name="maxRetriesMs">Total time to retry (ms).</param>
	/// <param name="delayMs">Delay between retries (ms).</param>
	/// <returns>(true, null) if valid; (false, errorMessage) if validation failed.</returns>
	public static (bool success, string? errorMessage) ValidateCreated(
		string path,
		int maxRetriesMs = DefaultMaxRetriesMs,
		int delayMs = DefaultDelayMs)
	{
		if (string.IsNullOrWhiteSpace(path))
			return (false, "(unknown file): path is null or empty.");

		var displayName = Path.GetFileName(path);
		if (string.IsNullOrWhiteSpace(displayName))
			displayName = path;

		var stopAt = DateTime.UtcNow.AddMilliseconds(maxRetriesMs);
		Exception? lastEx = null;

		while (DateTime.UtcNow < stopAt)
		{
			try
			{
				if (!File.Exists(path))
				{
					lastEx = new FileNotFoundException($"File not found after creation: {path}");
					Thread.Sleep(delayMs);
					continue;
				}

				using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read))
				{
					// ensure we can open for read and write
				}

				Log.Logger.Debug("Essential file validated: {DisplayName} at \"{Path}\"", displayName, path);
				return (true, null);
			}
			catch (Exception ex)
			{
				lastEx = ex;
				Thread.Sleep(delayMs);
			}
		}

		var msg = lastEx is not null
			? $"{displayName} could not be validated at \"{path}\": {lastEx.Message}"
			: $"{displayName} could not be validated at \"{path}\" (file not found or not accessible).";
		return (false, msg);
	}

	/// <summary>
	/// Validates that the file was created correctly and, if validation fails, reports the failure (log and optionally user).
	/// Equivalent to calling <see cref="ValidateCreated"/> then <see cref="ReportValidationFailure"/> when the result is not valid.
	/// </summary>
	/// <returns>True if the file is valid; false if validation failed (and failure has been reported).</returns>
	public static bool ValidateCreatedAndReport(
		string path,
		int maxRetriesMs = DefaultMaxRetriesMs,
		int delayMs = DefaultDelayMs)
	{
		var (success, errorMessage) = ValidateCreated(path, maxRetriesMs, delayMs);
		if (!success && errorMessage is not null)
			ReportValidationFailure(errorMessage);
		return success;
	}

	/// <summary>
	/// Reports a validation failure: tries to log the error; if logging fails, invokes <see cref="ShowUserWhenLogUnavailable"/>.
	/// The message is prefixed with a strongly worded error notice for both log and user display.
	/// </summary>
	/// <param name="errorMessage">Message to log and optionally show to the user.</param>
	public static void ReportValidationFailure(string errorMessage)
	{
		var fullMessage = $"Critical error! Essential file validation failed: {errorMessage}";
		try
		{
			Log.Logger.Error("Critical error! Essential file validation failed: {ErrorMessage}. Call stack: {StackTrace}",
				errorMessage, Environment.StackTrace);
		}
		catch
		{
			ShowUserWhenLogUnavailable?.Invoke(fullMessage);
		}
	}
}
