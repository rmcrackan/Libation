using Dinah.Core;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer;

public class Contributor
{
	// Empty is a special case. use private ctor w/o validation
	public static Contributor GetEmpty() => new() { ContributorId = -1, Name = "" };

	// contributors search links are just name with url-encoding. space can be + or %20
	//   author search link:   /search?searchAuthor=Robert+Bevan
	//   narrator search link: /search?searchNarrator=Robert+Bevan
	// can also search multiples. concat with comma before url encode

	// id.s
	// ----
	// https://www.audible.com/author/Neil-Gaiman/B000AQ01G2 == https://www.audible.com/author/B000AQ01G2
	//     goes to summary page
	//     at bottom "See all titles by Neil Gaiman" goes to https://www.audible.com/search?searchAuthor=Neil+Gaiman
	// some authors have no id. simply goes to https://www.audible.com/search?searchAuthor=Rufus+Fears
	// all narrators have no id: https://www.audible.com/search?searchNarrator=Neil+Gaiman

	internal int ContributorId { get; private set; }
	public string Name { get; private set; }

	private readonly HashSet<BookContributor> _booksLink;
	public IEnumerable<BookContributor> BooksLink => _booksLink?.ToList() ?? [];

	public string? AudibleContributorId { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private Contributor() { }
#pragma warning restore CS8618
	public Contributor(string name, string? audibleContributorId = null)
	{
		Name = ArgumentValidator.EnsureNotNullOrWhiteSpace(name, nameof(name));

		_booksLink = new HashSet<BookContributor>();
		SetAudibleContributorId(audibleContributorId);
	}

	public override string ToString() => Name;
	public void SetAudibleContributorId(string? audibleContributorId)
	{
		// don't overwrite with null or whitespace but not an error
		if (!string.IsNullOrWhiteSpace(audibleContributorId))
			AudibleContributorId = audibleContributorId;
	}

	public bool IsEmpty => ContributorId == -1;
}
