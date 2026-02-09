using Dinah.Core;

namespace DataLayer;

public class SeriesBook
{
	internal int SeriesId { get; private set; }
	internal int BookId { get; private set; }

	public string? Order { get; private set; }
	public float Index => StringLib.ExtractFirstNumber(Order);

	public Series Series { get; private set; }
	public Book Book { get; private set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private SeriesBook() { }
#pragma warning restore CS8618
	internal SeriesBook(Series series, Book book, string? order)
	{
		ArgumentValidator.EnsureNotNull(series, nameof(series));
		ArgumentValidator.EnsureNotNull(book, nameof(book));

		Series = series;
		Book = book;
		Order = order;
	}

	public void UpdateOrder(string? order)
	{
		if (!string.IsNullOrWhiteSpace(order))
			Order = order;
	}

	public override string ToString() => $"Series={Series} Book={Book}";
}
