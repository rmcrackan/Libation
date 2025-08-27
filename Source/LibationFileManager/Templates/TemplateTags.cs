﻿using FileManager.NamingTemplate;

#nullable enable
namespace LibationFileManager.Templates
{
	public sealed class TemplateTags : ITemplateTag
	{
		public const string DEFAULT_DATE_FORMAT = "yyyy-MM-dd";
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

		public static TemplateTags ChCount { get; } = new TemplateTags("ch count", "Number of chapters");
		public static TemplateTags ChTitle { get; } = new TemplateTags("ch title", "Chapter title");
		public static TemplateTags ChNumber { get; } = new TemplateTags("ch#", "Chapter #");
		public static TemplateTags ChNumber0 { get; } = new TemplateTags("ch# 0", "Chapter # with leading zeros");

		public static TemplateTags Id { get; } = new TemplateTags("id", "Audible ID");
		public static TemplateTags Title { get; } = new TemplateTags("title", "Full title with subtitle");
		public static TemplateTags TitleShort { get; } = new TemplateTags("title short", "Title. Stop at first colon");
		public static TemplateTags AudibleTitle { get; } = new TemplateTags("audible title", "Audible's title (does not include subtitle)");
		public static TemplateTags AudibleSubtitle { get; } = new TemplateTags("audible subtitle", "Audible's subtitle");
		public static TemplateTags Author { get; } = new TemplateTags("author", "Author(s)");
		public static TemplateTags FirstAuthor { get; } = new TemplateTags("first author", "First author");
		public static TemplateTags Narrator { get; } = new TemplateTags("narrator", "Narrator(s)");
		public static TemplateTags FirstNarrator { get; } = new TemplateTags("first narrator", "First narrator");
		public static TemplateTags Series { get; } = new TemplateTags("series", "All series to which the book belongs (if any)");
		public static TemplateTags FirstSeries { get; } = new TemplateTags("first series", "First series");
		public static TemplateTags SeriesNumber { get; } = new TemplateTags("series#", "Number order in series (alias for <first series[{#}]>");
		public static TemplateTags Bitrate { get; } = new TemplateTags("bitrate", "Bitrate (kbps) of the last downloaded audiobook");
		public static TemplateTags SampleRate { get; } = new TemplateTags("samplerate", "Sample rate (Hz) of the last downloaded audiobook");
		public static TemplateTags Channels { get; } = new TemplateTags("channels", "Number of audio channels in the last downloaded audiobook");
		public static TemplateTags Codec { get; } = new TemplateTags("codec", "Audio codec of the last downloaded audiobook");
		public static TemplateTags FileVersion { get; } = new TemplateTags("file version", "Audible's file version number of the last downloaded audiobook");
		public static TemplateTags LibationVersion { get; } = new TemplateTags("libation version", "Libation version used during last download of the audiobook");
		public static TemplateTags Account { get; } = new TemplateTags("account", "Audible account of this book");
		public static TemplateTags AccountNickname { get; } = new TemplateTags("account nickname", "Audible account nickname of this book");
		public static TemplateTags Locale { get; } = new("locale", "Region/country");
		public static TemplateTags YearPublished { get; } = new("year", "Year published");
		public static TemplateTags Language { get; } = new("language", "Book's language");
		public static TemplateTags LanguageShort { get; } = new("language short", "Book's language abbreviated. Eg: ENG");

		public static TemplateTags FileDate { get; } = new TemplateTags("file date", "File date/time. e.g. yyyy-MM-dd HH-mm", $"<file date [{DEFAULT_DATE_FORMAT}]>", "<file date [...]>");
		public static TemplateTags DatePublished { get; } = new TemplateTags("pub date", "Publication date. e.g. yyyy-MM-dd", $"<pub date [{DEFAULT_DATE_FORMAT}]>", "<pub date [...]>");
		public static TemplateTags DateAdded { get; } = new TemplateTags("date added", "Date added to your Audible account. e.g. yyyy-MM-dd", $"<date added [{DEFAULT_DATE_FORMAT}]>", "<date added [...]>");
		public static TemplateTags IfSeries { get; } = new TemplateTags("if series", "Only include if part of a book series or podcast", "<if series-><-if series>", "<if series->...<-if series>");
		public static TemplateTags IfPodcast { get; } = new TemplateTags("if podcast", "Only include if part of a podcast", "<if podcast-><-if podcast>", "<if podcast->...<-if podcast>");
		public static TemplateTags IfPodcastParent { get; } = new TemplateTags("if podcastparent", "Only include if item is a podcast series parent", "<if podcastparent-><-if podcastparent>", "<if podcastparent->...<-if podcastparent>");
		public static TemplateTags IfBookseries { get; } = new TemplateTags("if bookseries", "Only include if part of a book series", "<if bookseries-><-if bookseries>", "<if bookseries->...<-if bookseries>");
		public static TemplateTags Has { get; } = new TemplateTags("has", "Only include if PROPERTY has a value (i.e. not null or empty)", "<has -><-has>", "<has PROPERTY->...<-has>");
		public static TemplateTags NotHas { get; } = new TemplateTags("not-has", "Only include if PROPERTY is missing (i.e. null or empty)", "<not-has -><-not-has>", "<not-has PROPERTY->...<-not-has>");
	}
}
