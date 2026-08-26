using AudibleApi;
using Dinah.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AudibleUtilities;

public enum MarketplaceProbeOutcome
{
	/// <summary>Already scanned: this account's own marketplace, or one it has already been given.</summary>
	AlreadyScanned,
	/// <summary>Scanned by a different account row for the same login - the pre-existing way to hold two marketplaces.</summary>
	ScannedByAnotherAccount,
	/// <summary>The marketplace answered, and holds titles.</summary>
	TitlesFound,
	/// <summary>The marketplace answered, and holds nothing.</summary>
	Empty,
	/// <summary>The marketplace could not be asked. Says nothing either way about what is there.</summary>
	Failed
}

/// <param name="TitleCount">Titles the marketplace reports, or null when it was not asked or did not answer.</param>
/// <param name="ClaimedBy">For <see cref="MarketplaceProbeOutcome.ScannedByAnotherAccount"/>, the account already scanning it.</param>
public record MarketplaceProbeResult(
	Locale Locale,
	MarketplaceProbeOutcome Outcome,
	int? TitleCount = null,
	string? ClaimedBy = null,
	string? Error = null)
{
	/// <summary>Whether adding this marketplace to the account is something the user can choose to do.</summary>
	public bool CanAdd => Outcome is MarketplaceProbeOutcome.TitlesFound or MarketplaceProbeOutcome.Empty;
}

/// <summary>
/// <para>
/// Asks each Audible marketplace whether this login holds anything there.
/// </para>
/// <para>
/// A title bought while an Amazon address was briefly set to another country stays in that country's library for
/// good, and a scan of the account's own marketplace will never see it - no error, no warning, the titles are
/// simply absent. One device registration is honored by every marketplace, so the only thing standing between
/// those titles and a scan is knowing which marketplace to look in. That is what this answers.
/// </para>
/// <para>
/// One request per marketplace, in sequence: enough to get a count, and no more traffic or concurrency than a
/// user who clicked a button should generate. Nothing here runs on its own - the accounts dialog asks for it.
/// </para>
/// </summary>
public static class MarketplaceProbe
{
	/// <summary>Space between requests. A deliberate trickle rather than a fan-out.</summary>
	public static TimeSpan RequestSpacing { get; set; } = TimeSpan.FromMilliseconds(250);

	/// <summary>
	/// The marketplaces worth asking about for this account. Pre-Amazon locales and modern ones are separate
	/// worlds with their own logins, so only the account's own kind is probed; asking about the rest would double
	/// the traffic to learn nothing.
	/// </summary>
	public static IReadOnlyList<Locale> CandidateLocales(Account account)
	{
		var withUsername = account?.Locale?.WithUsername ?? false;

		return Localization.Locales
			.Where(l => l.WithUsername == withUsername)
			.OrderBy(l => l.Name)
			.ToList();
	}

	/// <summary>
	/// Probe every candidate marketplace, yielding each result as it arrives so a dialog can fill in a row at a
	/// time. Never throws for a single marketplace: one that cannot be reached is reported as such and the rest
	/// carry on.
	/// </summary>
	public static async IAsyncEnumerable<MarketplaceProbeResult> ProbeAsync(
		Account account,
		AccountsSettings accountsSettings,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		ArgumentValidator.EnsureNotNull(account, nameof(account));
		ArgumentValidator.EnsureNotNull(accountsSettings, nameof(accountsSettings));

		var first = true;

		foreach (var locale in CandidateLocales(account))
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (account.HasMarketplace(locale.Name))
			{
				yield return new MarketplaceProbeResult(locale, MarketplaceProbeOutcome.AlreadyScanned);
				continue;
			}

			if (accountsSettings.GetAccountClaimingMarketplace(account.AccountId, locale.Name, excluding: account) is { } other)
			{
				yield return new MarketplaceProbeResult(
					locale,
					MarketplaceProbeOutcome.ScannedByAnotherAccount,
					ClaimedBy: AccountCredentialStatus.FormatAccountLabel(other));
				continue;
			}

			if (!first)
				await Task.Delay(RequestSpacing, cancellationToken);
			first = false;

			yield return await probeOneAsync(account, locale);
		}
	}

	private static async Task<MarketplaceProbeResult> probeOneAsync(Account account, Locale locale)
	{
		try
		{
			// no interactive login: the whole premise is that this account's existing tokens already work here
			var apiExtended = await ApiExtended.CreateAsync(account, allowInteractiveLogin: false, storeLocale: locale);

			var count = await apiExtended.Api.GetItemsCountAsync(
				new LibraryOptions { PurchasedAfter = new DateTime(1970, 1, 1) });

			Serilog.Log.Logger.Information(
				"Marketplace probe: {LocaleName} reported {TitleCount} titles. {@DebugInfo}",
				locale.Name,
				count,
				new { Account = account.MaskedLogEntry });

			// -1 means Audible answered without the count header. it answered, so the marketplace is reachable;
			// treat it as reachable-but-unknown rather than claiming there is nothing there
			return count switch
			{
				> 0 => new MarketplaceProbeResult(locale, MarketplaceProbeOutcome.TitlesFound, count),
				0 => new MarketplaceProbeResult(locale, MarketplaceProbeOutcome.Empty, 0),
				_ => new MarketplaceProbeResult(locale, MarketplaceProbeOutcome.Empty)
			};
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Information(
				ex,
				"Marketplace probe: {LocaleName} could not be checked. {@DebugInfo}",
				locale.Name,
				new { Account = account.MaskedLogEntry });

			return new MarketplaceProbeResult(locale, MarketplaceProbeOutcome.Failed, Error: ex.Message);
		}
	}
}
