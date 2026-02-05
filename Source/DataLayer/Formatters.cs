using System.Linq;

namespace DataLayer;

internal class Formatters
{
	private static string[] _sortPrefixIgnores { get; } = { "the", "a", "an" };

	public static string GetSortName(string unformattedName)
	{
		var sortName = unformattedName
			.Replace("|", "")
			.Replace(":", "")
			.ToLowerInvariant()
			.Trim();

		if (_sortPrefixIgnores.Any(prefix => sortName.StartsWith(prefix + " ")))
			sortName = sortName
				.Substring(sortName.IndexOf(" ") + 1)
				.TrimStart();

		return sortName;
	}
}
