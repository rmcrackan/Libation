using CsvHelper.Configuration.Attributes;
using DataLayer;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace ApplicationServices;

internal class ExportDto(LibraryBook libBook)
{
	[Name("Account")]
	public string Account { get; } = libBook.Account;

	[Name("Date Added to library")]
	public DateTime DateAdded { get; } = libBook.DateAdded;

	[Name("Is Audible Plus?")]
	public bool IsAudiblePlus { get; } = libBook.IsAudiblePlus;

	[Name("Absent from last scan?")]
	public bool AbsentFromLastScan { get; } = libBook.AbsentFromLastScan;

	[Name("Audible Product Id")]
	public string AudibleProductId { get; } = libBook.Book.AudibleProductId;

	[Name("Locale")]
	public string Locale { get; } = libBook.Book.Locale;

	[Name("Title")]
	public string Title { get; } = libBook.Book.Title;

	[Name("Subtitle")]
	public string Subtitle { get; } = libBook.Book.Subtitle;

	[Name("Authors")]
	public string AuthorNames { get; } = libBook.Book.AuthorNames;

	[Name("Narrators")]
	public string NarratorNames { get; } = libBook.Book.NarratorNames;

	[Name("Length In Minutes")]
	public int LengthInMinutes { get; } = libBook.Book.LengthInMinutes;

	[Name("Description")]
	public string Description { get; } = libBook.Book.Description;

	[Name("Publisher")]
	public string? Publisher { get; } = libBook.Book.Publisher;

	[Name("Has PDF")]
	public bool HasPdf { get; } = libBook.Book.HasPdf;

	[Name("Series Names")]
	public string SeriesNames { get; } = libBook.Book.SeriesNames();

	[Name("Series Order")]
	public string SeriesOrder { get; } = libBook.Book.SeriesLink?.Any() is true ? string.Join(", ", libBook.Book.SeriesLink.Select(sl => $"{sl.Order} : {sl.Series.Name}")) : "";

	[Name("Community Rating: Overall")]
	public float? CommunityRatingOverall { get; } = ZeroIsNull(libBook.Book.Rating?.OverallRating);

	[Name("Community Rating: Performance")]
	public float? CommunityRatingPerformance { get; } = ZeroIsNull(libBook.Book.Rating?.PerformanceRating);

	[Name("Community Rating: Story")]
	public float? CommunityRatingStory { get; } = ZeroIsNull(libBook.Book.Rating?.StoryRating);

	[Name("Cover Id")]
	public string? PictureId { get; } = libBook.Book.PictureId;

	[Name("Cover Id Large")]
	public string? PictureLarge { get; } = libBook.Book.PictureLarge;

	[Name("Is Abridged?")]
	public bool IsAbridged { get; } = libBook.Book.IsAbridged;

	[Name("Date Published")]
	public DateTime? DatePublished { get; } = libBook.Book.DatePublished;

	[Name("Categories")]
	public string CategoriesNames { get; } = string.Join("; ", libBook.Book.LowestCategoryNames());

	[Name("My Rating: Overall")]
	public float? MyRatingOverall { get; } = ZeroIsNull(libBook.Book.UserDefinedItem.Rating.OverallRating);

	[Name("My Rating: Performance")]
	public float? MyRatingPerformance { get; } = ZeroIsNull(libBook.Book.UserDefinedItem.Rating.PerformanceRating);

	[Name("My Rating: Story")]
	public float? MyRatingStory { get; } = ZeroIsNull(libBook.Book.UserDefinedItem.Rating.StoryRating);

	[Name("My Libation Tags")]
	public string MyLibationTags { get; } = libBook.Book.UserDefinedItem.Tags;

	[Name("Book Liberated Status")]
	public string BookStatus { get; } = libBook.Book.UserDefinedItem.BookStatus.ToString();

	[Name("PDF Liberated Status")]
	public string? PdfStatus { get; } = libBook.Book.UserDefinedItem.PdfStatus.ToString();

	[Name("Content Type")]
	public string ContentType { get; } = libBook.Book.ContentType.ToString();

	[Name("Language")]
	public string? Language { get; } = libBook.Book.Language;

	[Name("Last Downloaded")]
	public DateTime? LastDownloaded { get; } = libBook.Book.UserDefinedItem.LastDownloaded;

	[Name("Last Downloaded Version")]
	public string? LastDownloadedVersion { get; } = libBook.Book.UserDefinedItem.LastDownloadedVersion?.ToString();

	[Name("Is Finished?")]
	public bool IsFinished { get; } = libBook.Book.UserDefinedItem.IsFinished;

	[Name("Is Spatial?")]
	public bool IsSpatial { get; } = libBook.Book.IsSpatial;

	[Name("Included Until")]
	public DateTime? IncludedUntil { get; } = libBook.IncludedUntil;

	[Name("Last Downloaded File Version")]
	public string? LastDownloadedFileVersion { get; } = libBook.Book.UserDefinedItem.LastDownloadedFileVersion;

	[Ignore /* csv ignore */]
	public AudioFormat? LastDownloadedFormat { get; } = libBook.Book.UserDefinedItem.LastDownloadedFormat;

	[Name("Last Downloaded Codec"), JsonIgnore]
	public string CodecString => LastDownloadedFormat?.CodecString ?? "";

	[Name("Last Downloaded Sample rate"), JsonIgnore]
	public int? SampleRate => LastDownloadedFormat?.SampleRate;

	[Name("Last Downloaded Audio Channels"), JsonIgnore]
	public int? ChannelCount => LastDownloadedFormat?.ChannelCount;

	[Name("Last Downloaded Bitrate"), JsonIgnore]
	public int? BitRate => LastDownloadedFormat?.BitRate;

	private static float? ZeroIsNull(float? value) => value is 0 ? null : value;
}
