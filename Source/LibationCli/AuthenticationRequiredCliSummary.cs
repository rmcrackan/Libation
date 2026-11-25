using AudibleUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationCli;

/// <summary>
/// Formats CLI authentication failure summaries without dumping stack traces to the console or leaking
/// unmasked account emails into stdout (which is teed into Serilog).
/// </summary>
internal static class AuthenticationRequiredCliSummary
{
	public static void WriteSummary(
		IReadOnlyList<AuthenticationRequiredException> authFailures,
		TextWriter stdout,
		TextWriter stderr)
	{
		if (authFailures.Count == 0)
			return;

		var distinct = authFailures
			.Select(f => f.AccountInfo)
			.Where(info => info is not null)
			.Select(info => info!)
			.DistinctBy(info => info.RevealOwnerFacingLabel(), StringComparer.OrdinalIgnoreCase)
			.ToList();

		if (distinct.Count == 0)
		{
			stdout.WriteLine("One or more accounts were skipped because they need Audible sign-in.");
			stderr.WriteLine("Sign in with the Libation GUI (Import > Scan Library) or run 'libationcli login-external --account <email> --locale <locale>'.");
			return;
		}

		stdout.WriteLine(distinct.Count == 1
			? "1 account was skipped because it needs Audible sign-in:"
			: $"{distinct.Count} accounts were skipped because they need Audible sign-in:");

		foreach (var info in distinct)
			stdout.WriteLine($"  - {info.FormatLogSafeLabel()}");

		var hints = distinct
			.Select(info => info.RevealLoginExternalHint())
			.Where(hint => !string.IsNullOrWhiteSpace(hint))
			.Select(hint => hint!)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToList();

		if (hints.Count > 0)
		{
			stderr.WriteLine("To sign in again (CLI):");
			foreach (var hint in hints)
				stderr.WriteLine($"  {hint}");
		}

		stderr.WriteLine("Or sign in with the Libation GUI (Import > Scan Library).");
	}
}
