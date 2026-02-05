using Dinah.Core;

namespace DataLayer;

public enum Role { Author = 1, Narrator = 2, Publisher = 3 }

public class BookContributor
{
	internal int BookId { get; private set; }
	internal int ContributorId { get; private set; }
	public Role Role { get; private set; }
	public byte Order { get; private set; }

	public Book Book { get; private set; }
	public Contributor Contributor { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private BookContributor() { }
#pragma warning restore CS8618
	internal BookContributor(Book book, Contributor contributor, Role role, byte order)
	{
		ArgumentValidator.EnsureNotNull(book, nameof(book));
		ArgumentValidator.EnsureNotNull(contributor, nameof(contributor));

		Book = book;
		Contributor = contributor;
		Role = role;
		Order = order;
	}

	public override string ToString() => $"{Book} {Contributor} {Role} {Order}";
}
