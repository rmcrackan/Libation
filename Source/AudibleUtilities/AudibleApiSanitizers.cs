using AudibleApi.Common;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace AudibleUtilities;

public interface ISanitizer
{
	void Sanitize(IEnumerable<Item> items);

	public static ISanitizer[] GetAllSanitizers() => [
		new ContributorSanitizer()
	];
}

public class ContributorSanitizer : ISanitizer
{
	public void Sanitize(IEnumerable<Item> items)
	{
		foreach (var item in items)
		{
			item.Authors = SanitizePersonArray(item.Authors);
			item.Narrators = SanitizePersonArray(item.Narrators);
		}
	}

	private static Person[]? SanitizePersonArray(Person?[]? contributors)
		=> contributors
		?.OfType<Person>()
		.Where(c => !string.IsNullOrWhiteSpace(c.Name))
		.ToArray();
}
