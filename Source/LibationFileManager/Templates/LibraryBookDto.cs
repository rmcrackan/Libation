using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace LibationFileManager.Templates;

public class BookDto
{
	public string? AudibleProductId { get; set; }
	public string? Title { get; set; }
	public string? Subtitle { get; set; }
	public string? TitleWithSubtitle { get; set; }
	public string? Locale { get; set; }
	public int? YearPublished { get; set; }

	public IEnumerable<ContributorDto>? Authors { get; set; }
	public ContributorDto? FirstAuthor => Authors?.FirstOrDefault();

	public IEnumerable<ContributorDto>? Narrators { get; set; }
	public ContributorDto? FirstNarrator => Narrators?.FirstOrDefault();

	public IEnumerable<SeriesDto>? Series { get; set; }
	public SeriesDto? FirstSeries => Series?.FirstOrDefault();

	public bool IsSeries => Series is not null;
	public bool IsPodcastParent { get; set; }
	public bool IsPodcast { get; set; }

	public int BitRate { get; set; }
	public int SampleRate { get; set; }
	public int Channels { get; set; }
	public DateTime FileDate { get; set; } = DateTime.Now;
	public DateTime? DatePublished { get; set; }
	public string? Language { get; set; }
}

public class LibraryBookDto : BookDto
{
	public DateTime? DateAdded { get; set; }
	public string? Account { get; set; }
	public string? AccountNickname { get; set; }
}
