using FileManager.NamingTemplate;

namespace LibationFileManager.Templates;

public sealed class TemplateTags : ITemplateTag
{
	public string TagName { get; }
	public string DefaultValue { get; }
	public string Description { get; }
	public string Display { get; }

	private TemplateTags(string tagName, string description, string? defaultValue = null, string? display = null)
	{
		TagName = tagName;
		Description = description;
		DefaultValue = defaultValue ?? $"<{tagName}>";
		Display = display ?? $"<{tagName}>";
	}

	public static TemplateTags ChCount { get; } = new("ch count", "Number of chapters");
	public static TemplateTags ChTitle { get; } = new("ch title", "Chapter title");
	public static TemplateTags ChNumber { get; } = new("ch#", "Chapter #");
	public static TemplateTags ChNumber0 { get; } = new("ch# 0", "Chapter # with leading zeros");

	public static TemplateTags Id { get; } = new("id", "Audible ID");
	public static TemplateTags Title { get; } = new("title", "Full title with subtitle");
	public static TemplateTags TitleShort { get; } = new("title short", "Title. Stop at first colon");
	public static TemplateTags AudibleTitle { get; } = new("audible title", "Audible's title (does not include subtitle)");
	public static TemplateTags AudibleSubtitle { get; } = new("audible subtitle", "Audible's subtitle");
	public static TemplateTags Author { get; } = new("author", "Author(s)");
	public static TemplateTags FirstAuthor { get; } = new("first author", "First author");
	public static TemplateTags Narrator { get; } = new("narrator", "Narrator(s)");
	public static TemplateTags FirstNarrator { get; } = new("first narrator", "First narrator");
	public static TemplateTags Series { get; } = new("series", "All series to which the book belongs (if any)");
	public static TemplateTags FirstSeries { get; } = new("first series", "First series");
	public static TemplateTags SeriesNumber { get; } = new("series#", "Number order in series (alias for <first series[{#}]>");
	public static TemplateTags Minutes { get; } = new("minutes", "Length in minutes");
	public static TemplateTags Bitrate { get; } = new("bitrate", "Bitrate (kbps) of the last downloaded audiobook");
	public static TemplateTags SampleRate { get; } = new("samplerate", "Sample rate (Hz) of the last downloaded audiobook");
	public static TemplateTags Channels { get; } = new("channels", "Number of audio channels in the last downloaded audiobook");
	public static TemplateTags Codec { get; } = new("codec", "Audio codec of the last downloaded audiobook");
	public static TemplateTags FileVersion { get; } = new("file version", "Audible's file version number of the last downloaded audiobook");
	public static TemplateTags LibationVersion { get; } = new("libation version", "Libation version used during last download of the audiobook");
	public static TemplateTags Account { get; } = new("account", "Audible account of this book");
	public static TemplateTags AccountNickname { get; } = new("account nickname", "Audible account nickname of this book");
	public static TemplateTags Tag { get; } = new("tag", "Tag(s)");
	public static TemplateTags FirstTag { get; } = new("first tag", "First tag");
	public static TemplateTags Locale { get; } = new("locale", "Region/country");
	public static TemplateTags YearPublished { get; } = new("year", "Year published");
	public static TemplateTags Language { get; } = new("language", "Book's language");
	public static TemplateTags LanguageShort { get; } = new("language short", "Book's language abbreviated. Eg: ENG");
	public static TemplateTags UI { get; } = new("ui", "UI language");
	public static TemplateTags OS { get; } = new("os", "OS language");

	public static TemplateTags FileDate { get; } = new("file date", "File date/time. e.g. yyyy-MM-dd HH-mm", $"<file date [{CommonFormatters.DefaultDateFormat}]>", "<file date [...]>");
	public static TemplateTags DatePublished { get; } = new("pub date", "Publication date. e.g. yyyy-MM-dd", $"<pub date [{CommonFormatters.DefaultDateFormat}]>", "<pub date [...]>");

	public static TemplateTags DateAdded { get; } =
		new("date added", "Date added to your Audible account. e.g. yyyy-MM-dd", $"<date added [{CommonFormatters.DefaultDateFormat}]>", "<date added [...]>");

	public static TemplateTags IfSeries { get; } = new("if series", "Only include if part of a book series or podcast", "<if series-><-if series>", "<if series->...<-if series>");
	public static TemplateTags IfPodcast { get; } = new("if podcast", "Only include if part of a podcast", "<if podcast-><-if podcast>", "<if podcast->...<-if podcast>");

	public static TemplateTags IfPodcastParent { get; } = new("if podcastparent", "Only include if item is a podcast series parent", "<if podcastparent-><-if podcastparent>",
		"<if podcastparent->...<-if podcastparent>");

	public static TemplateTags IfBookseries { get; } = new("if bookseries", "Only include if part of a book series", "<if bookseries-><-if bookseries>", "<if bookseries->...<-if bookseries>");
	public static TemplateTags IfAbridged { get; } = new("if abridged", "Only include if abridged", "<if abridged-><-if abridged>", "<if abridged->...<-if abridged>");
	public static TemplateTags Has { get; } = new("has", "Only include if PROPERTY has a value (i.e. not null or empty)", "<has -><-has>", "<has PROPERTY->...<-has>");
	public static TemplateTags Is { get; } = new("is", "Only include if PROPERTY has a value satisfying the check (i.e. string comparison)", "<is -><-is>", "<is PROPERTY->...<-is>");
}
