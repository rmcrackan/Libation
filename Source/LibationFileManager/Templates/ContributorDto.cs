using NameParser;
using System;
using System.Collections.Generic;
using FileManager.NamingTemplate;

namespace LibationFileManager.Templates; 

public class ContributorDto(string name, string? audibleContributorId) : IFormattable
{
	private HumanName HumanName { get; } = new(RemoveSuffix(name), Prefer.FirstOverPrefix);
	private string? AudibleContributorId { get; } = audibleContributorId;

	public static readonly Dictionary<string, Func<ContributorDto, object?>> FormatReplacements = new(StringComparer.OrdinalIgnoreCase)
	{
		// Single-word names parse as first names. Use it as last name.
		{ "L", dto => string.IsNullOrWhiteSpace(dto.HumanName.Last) ? dto.HumanName.First : dto.HumanName.Last },
		// Because of the above, if we have only a first name, then we'd double the name as "FirstName FirstName", so clear the first name in that situation.
		{ "F", dto => string.IsNullOrWhiteSpace(dto.HumanName.Last) ? dto.HumanName.Last : dto.HumanName.First },

		{ "T", dto => dto.HumanName.Title },
		{ "M", dto => dto.HumanName.Middle },
		{ "S", dto => dto.HumanName.Suffix },
		{ "ID", dto => dto.AudibleContributorId },
	};

	public override string ToString() => ToString("{T} {F} {M} {L} {S}", null);

	public string ToString(string? format, IFormatProvider? provider)
		=> string.IsNullOrWhiteSpace(format)
			? ToString()
			: CommonFormatters.TemplateStringFormatter(this, format, provider, FormatReplacements);

	private static string RemoveSuffix(string namesString)
	{
		namesString = namesString.Replace('’', '\'').Replace(" - Ret.", ", Ret.");
		var dashIndex = namesString.IndexOf(" - ", StringComparison.Ordinal);
		return (dashIndex > 0 ? namesString[..dashIndex] : namesString).Trim();
	}
}
