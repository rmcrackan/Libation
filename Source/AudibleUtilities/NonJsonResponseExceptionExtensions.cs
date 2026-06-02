using AudibleApi;
using System;
using System.Collections.Generic;

namespace AudibleUtilities;

/// <summary>
/// Libation-facing helpers when Audible returns HTML instead of JSON (library scan, catalog, etc.).
/// </summary>
public static class NonJsonResponseExceptionExtensions
{
	public const string LibraryScanFailedCaption = "Library scan failed";

	private static readonly string[] ThingsToTryBullets =
	[
		"Scan again after a few minutes.",
		"Sign in to Audible in a browser on the same network.",
		"Disable VPN or proxy and scan again.",
		"Remove and re-add the account in Libation.",
		"If it still fails, check the log for Full body: near the error and attach it to a bug report.",
	];

	public static IEnumerable<string> GetExplainerLines(this NonJsonResponseException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);
		yield return getIntro(ex);
		yield return string.Empty;
		yield return "Things to try:";
		foreach (var bullet in ThingsToTryBullets)
			yield return "• " + bullet;
	}

	public static string GetExplainerBody(this NonJsonResponseException ex)
		=> string.Join("\r\n", ex.GetExplainerLines());

	public static bool TryFindInTree(Exception ex, out NonJsonResponseException? match)
	{
		ArgumentNullException.ThrowIfNull(ex);
		NonJsonResponseException? found = null;
		walk(ex);
		match = found;
		return found is not null;

		void walk(Exception? e)
		{
			if (e is null || found is not null)
				return;

			if (e is NonJsonResponseException nonJson)
				found = nonJson;

			if (found is not null)
				return;

			if (e is AggregateException agg)
			{
				foreach (var inner in agg.InnerExceptions)
					walk(inner);
			}

			walk(e.InnerException);
		}
	}

	private static string getIntro(NonJsonResponseException ex)
		=> ex.HtmlTitle is null
			? "Audible returned an HTML page instead of the expected library data."
			: $"Audible returned an HTML page ({ex.HtmlTitle}) instead of the expected library data.";
}
