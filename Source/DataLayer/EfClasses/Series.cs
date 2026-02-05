using Dinah.Core;
using System.Collections.Generic;
using System.Linq;

namespace DataLayer;

public class AudibleSeriesId
{
	public string Id { get; }
	public AudibleSeriesId(string id)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
		Id = id;
	}
}
public class Series
{
	internal int SeriesId { get; private set; }
	public string AudibleSeriesId { get; private set; }

	/// <summary>optional</summary>
	public string? Name { get; private set; }

	private readonly HashSet<SeriesBook> _booksLink;
	public IEnumerable<SeriesBook> BooksLink
		=> _booksLink?
			.OrderBy(sb => sb.Index)
			.ToList() ?? [];

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	private Series() { }
#pragma warning restore CS8618
	/// <summary>special id class b/c it's too easy to get string order mixed up</summary>
	public Series(AudibleSeriesId audibleSeriesId, string? name = null)
	{
		ArgumentValidator.EnsureNotNull(audibleSeriesId, nameof(audibleSeriesId));
		var id = audibleSeriesId.Id;
		ArgumentValidator.EnsureNotNullOrWhiteSpace(id, nameof(id));
		AudibleSeriesId = id;
		_booksLink = new HashSet<SeriesBook>();
		UpdateName(name);
	}

	public void UpdateName(string? name)
	{
		// don't overwrite with null or whitespace but not an error
		if (!string.IsNullOrWhiteSpace(name))
			Name = name;
	}

	public override string? ToString() => Name;
}
