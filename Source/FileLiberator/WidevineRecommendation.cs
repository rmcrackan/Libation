using AudibleApi;
using LibationFileManager;
using System;

namespace FileLiberator;

/// <summary>
/// Detects a rare Audible API failure mode where ADRM licenserequest returns Sable error 000307
/// with acr:null. Some titles (e.g. B08XDZCH78 in the US marketplace) only deliver via Widevine.
/// Keep all detection here so this edge case does not spread through download or UI code.
/// </summary>
public static class WidevineRecommendation
{
	/// <summary>
	/// Audible error code seen when Sable cannot resolve asset details for ADRM licenserequest.
	/// Not diagnostic on its own — also returned for unrelated API issues (e.g. empty response_groups).
	/// </summary>
	public const string SableAssetErrorCode = "000307";

	/// <summary>
	/// Substring in the Sable error message when no ADRM content reference exists for the title.
	/// Together with <see cref="SableAssetErrorCode"/> on licenserequest, this is the best signal we have.
	/// </summary>
	public const string SableAcrNullMarker = "acr:null";

	/// <summary>
	/// True when <paramref name="ex"/> matches the rare ADRM licenserequest / Sable acr:null pattern.
	/// </summary>
	public static bool IsAdrmLicenseUnavailableError(ApiErrorException ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		if (ex.RequestUri is not { } requestUri
			|| !requestUri.Contains("/licenserequest", StringComparison.OrdinalIgnoreCase))
			return false;

		if (ex.JsonMessage is not { } json)
			return false;

		return json.Contains(SableAssetErrorCode, StringComparison.Ordinal)
			&& json.Contains(SableAcrNullMarker, StringComparison.Ordinal);
	}

	/// <summary>
	/// True when Libation should suggest enabling Widevine for this failure.
	/// </summary>
	public static bool ShouldRecommendWidevine(ApiErrorException ex, Configuration config)
	{
		ArgumentNullException.ThrowIfNull(ex);
		ArgumentNullException.ThrowIfNull(config);

		return !config.UseWidevine && IsAdrmLicenseUnavailableError(ex);
	}

	/// <summary>One line for logs and CLI when <see cref="ShouldRecommendWidevine"/> applies.</summary>
	public static string BuildLogSummary(string bookTitleWithSubtitle)
		=> $"{bookTitleWithSubtitle}: Audible returned no ADRM license (Sable acr:null). "
			+ "Some rare titles only download with Widevine — try Settings > Audio File Options > Use Widevine DRM.";
}
