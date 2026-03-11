using NameParser;
using System;

namespace LibationFileManager.Templates;

public class ContributorDto(string name, string? audibleContributorId) : IFormattable
{
	public HumanName HumanName { get; } = new(RemoveSuffix(name), Prefer.FirstOverPrefix);
	private string? AudibleContributorId { get; } = audibleContributorId;

	public override string ToString()
		=> ToString("{T} {F} {M} {L} {S}", null);

	public string ToString(string? format, IFormatProvider? _)
	{
		if (string.IsNullOrWhiteSpace(format))
			return ToString();

		//Single-word names parse as first names. Use it as last name.
		var lastName = string.IsNullOrWhiteSpace(HumanName.Last) ? HumanName.First : HumanName.Last;
		//Because of the above, if the have only a first name, then we'd double the name as "FirstName FirstName", so clear the first name in that situation.
		var firstName = string.IsNullOrWhiteSpace(HumanName.Last) ? HumanName.Last : HumanName.First;

		return format
			.Replace("{T}", HumanName.Title)
			.Replace("{F}", firstName)
			.Replace("{M}", HumanName.Middle)
			.Replace("{L}", lastName)
			.Replace("{S}", HumanName.Suffix)
			.Replace("{ID}", AudibleContributorId)
			.Trim();
	}

	private static string RemoveSuffix(string namesString)
	{
		namesString = namesString.Replace('’', '\'').Replace(" - Ret.", ", Ret.");
		var dashIndex = namesString.IndexOf(" - ", StringComparison.Ordinal);
		return (dashIndex > 0 ? namesString[..dashIndex] : namesString).Trim();
	}
}
