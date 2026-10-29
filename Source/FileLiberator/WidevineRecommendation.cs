using AudibleApi;
using LibationFileManager;
using System;

namespace FileLiberator;

/// <summary>
/// Detects a rare Audible API failure mode where ADRM licenserequest returns Sable error 000307
/// with acr:null (or where the subsequent AAX workaround fails with missing codecs).
/// Some titles (e.g. B08XDZCH78 in the US marketplace, or non-US titles where legacy AAX is disabled)
/// only deliver via Widevine.
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
	/// Fallback error message substring thrown by AudibleApi when the AAX workaround
	/// cannot find codecs for a title whose primary ADRM licenserequest failed.
	/// </summary>
	public const string NoCodecSpecifiedMarker = "Book has no codec specified";

	/// <summary>
	/// Alternate fallback error message substring thrown by AudibleApi when the AAX workaround
	/// finds zero available codecs.
	/// </summary>
	public const string NoAvailableCodecsMarker = "Book has no available codecs";

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
	/// True when <paramref name="ex"/> indicates the AAX download workaround failed
	/// because the title has no legacy AAX audio codecs available.
	/// </summary>
	public static bool IsAaxFallbackMissingCodecError(Exception ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		if (ex.Message.Contains(NoCodecSpecifiedMarker, StringComparison.OrdinalIgnoreCase)
			|| ex.Message.Contains(NoAvailableCodecsMarker, StringComparison.OrdinalIgnoreCase))
			return true;

		return ex.InnerException is not null && IsAaxFallbackMissingCodecError(ex.InnerException);
	}

	/// <summary>
	/// True when <paramref name="ex"/> matches either the primary Sable acr:null pattern
	/// or the secondary AAX fallback missing-codec pattern.
	/// </summary>
	public static bool IsAdrmOrAaxFallbackUnavailable(Exception ex)
	{
		ArgumentNullException.ThrowIfNull(ex);

		if (ex is ApiErrorException apiEx && IsAdrmLicenseUnavailableError(apiEx))
			return true;

		return IsAaxFallbackMissingCodecError(ex);
	}

	/// <summary>
	/// True when Libation should suggest enabling Widevine for this failure.
	/// </summary>
	public static bool ShouldRecommendWidevine(Exception ex, Configuration config)
	{
		ArgumentNullException.ThrowIfNull(ex);
		ArgumentNullException.ThrowIfNull(config);

		return !config.UseWidevine && IsAdrmOrAaxFallbackUnavailable(ex);
	}

	/// <summary>Overload preserving binary / source compatibility for ApiErrorException callers.</summary>
	public static bool ShouldRecommendWidevine(ApiErrorException ex, Configuration config)
		=> ShouldRecommendWidevine((Exception)ex, config);

	/// <summary>One line for logs and CLI when <see cref="ShouldRecommendWidevine(Exception, Configuration)"/> applies.</summary>
	public static string BuildLogSummary(string bookTitleWithSubtitle, string? accountId = null)
	{
		var accountSuffix = string.IsNullOrWhiteSpace(accountId) ? "" : $" for account '{accountId}'";
		return $"{bookTitleWithSubtitle}: Standard (ADRM) license unavailable. "
			+ $"Libation enabled Widevine DRM and marked account{accountSuffix} for re-registration. Sign in to complete setup.";
	}
}
