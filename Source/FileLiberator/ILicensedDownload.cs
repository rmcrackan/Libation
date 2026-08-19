namespace FileLiberator;

/// <summary>
/// A step that downloads something Audible delivers through a content license: the audiobook, and the
/// supplement whose link the same license carries.
/// <para>
/// One license serves every such step for a title. The request is identical - <c>response_groups</c> asks for
/// <c>pdf_url</c> alongside the content reference - so a run holding a license must hand it on rather than ask
/// for another, which would be a second refusal to record for a title already refused once.
/// </para>
/// </summary>
public interface ILicensedDownload
{
	/// <summary>
	/// Optional override to supply license info directly instead of querying the api based on Configuration options
	/// </summary>
	DownloadOptions.LicenseInfo? LicenseInfo { get; set; }

	/// <summary>
	/// The license this step ended up using, whether supplied or requested, so the next step for the same title
	/// can reuse it. Null until a license has been obtained, and reset at the start of every attempt: a
	/// Processable instance is reused across books, so a license left behind belongs to a different title.
	/// </summary>
	DownloadOptions.LicenseInfo? ObtainedLicense { get; }
}
