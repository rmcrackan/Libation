using Dinah.Core;

namespace ApplicationServices;

/// <summary>
/// What to tell the user about titles a run left alone because the last library scan did not find them.
/// <para>
/// Audible will not license a title it no longer lists for the account - a returned title, or one that has
/// left the Plus catalog - so attempting one collects a refusal every run and downloads nothing. Both hosts
/// say it in these words so that the same skip does not read as two different things.
/// </para>
/// </summary>
public static class AbsentFromLastScanUserMessage
{
	/// <summary>The reason, short enough to head a list of skip reasons.</summary>
	public const string Label = "Absent from your last library scan";

	/// <summary>What to do about it.</summary>
	public const string Advice = "run Scan, or `libationcli scan`, then try again";

	/// <summary>The line a CLI run prints in place of attempting them.</summary>
	public static string BuildCliSkippedSummary(int count)
		=> $"Skipped {"title".PluralizeWithCount(count)} absent from your last library scan. "
		+ $"Audible will not license a title it no longer lists, so {Advice}. To attempt them anyway: libationcli liberate --force.";
}
