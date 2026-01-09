using System;
using System.Linq;
using System.Reflection;
using System.Text;

#nullable enable
namespace DataLayer;
public class MockLibraryBook : LibraryBook
{
	protected MockLibraryBook(Book book, DateTime dateAdded, string account, DateTime? includedUntil, bool isAudiblePlus)
		: base(book, dateAdded, account)
	{
		SetIncludedUntil(includedUntil);
		SetIsAudiblePlus(isAudiblePlus);
	}

	public MockLibraryBook AddSeries(string seriesName, int order)
	{
		var series = new Series(new AudibleSeriesId(CalculateAsin(seriesName)), seriesName);
		Book.UpsertSeries(series, order.ToString());
		return this;
	}

	public MockLibraryBook AddCategoryLadder(params string[] ladder)
	{
		var newLadder = new CategoryLadder(ladder.Select(c => new Category(new AudibleCategoryId(CalculateAsin(c)), c)).ToList());
		Book.SetCategoryLadders(Book.Categories.Select(c => c.CategoryLadder).Append(newLadder));
		return this;
	}

	public MockLibraryBook AddNarrator(string name)
	{
		var newNarrator = new Contributor(name, CalculateAsin(name));
		Book.ReplaceNarrators(Book.Narrators.Append(newNarrator));
		return this;
	}

	public MockLibraryBook AddAuthor(string name)
	{
		var newAuthor = new Contributor(name, CalculateAsin(name));
		Book.ReplaceAuthors(Book.Authors.Append(newAuthor));
		return this;
	}

	public MockLibraryBook WithBookStatus(LiberatedStatus liberatedStatus)
	{
		//Set the backing field directly to preserve LiberatedStatus.PartialDownload
		typeof(UserDefinedItem)
			.GetField("_bookStatus", BindingFlags.NonPublic | BindingFlags.Instance)
			?.SetValue(Book.UserDefinedItem, liberatedStatus);
		return this;
	}

	public MockLibraryBook WithPdfStatus(LiberatedStatus liberatedStatus)
	{
		Book.UserDefinedItem.PdfStatus = liberatedStatus;
		return this;
	}

	public MockLibraryBook WithLastDownloaded(Version? lastVersion = null, AudioFormat? format = null, string audioVersion = "1")
	{
		lastVersion ??= new Version(10, 0, 0, 0);
		format ??= AudioFormat.Default;
		Book.UserDefinedItem.SetLastDownloaded(lastVersion, format, audioVersion);
		return this;
	}

	public MockLibraryBook WithMyRating(float overallRating = 4, float performanceRating = 4.5f, float storyRating = 5)
	{
		Book.UserDefinedItem.UpdateRating(overallRating, performanceRating, storyRating);
		return this;
	}

	public static MockLibraryBook CreateBook(
		string account = "someone@email.co",
		bool absetFromLastScan = false,
		DateTime? dateAdded = null,
		DateTime? datePublished = null,
		DateTime? includedUntil = null,
		bool isAudiblePlus = false,
		string title = "Mock Book Title",
		string subtitle = "Mock Book Subtitle",
		string description = "This is a mock book description.",
		int lengthInMinutes = 1400,
		ContentType contentType = ContentType.Product,
		string firstAuthor = "Author One",
		string firstNarrator = "Narrator One",
		string localeName = "us",
		bool isAbridged = false,
		bool isSpatial = false,
		string language = "English",
		LiberatedStatus bookStatus = LiberatedStatus.Liberated,
		LiberatedStatus? pdfStatus = null,
		AudioFormat? lastDlFormat = null,
		Version? lastDlVersion = null)
	{
		var book = new Book(
			new AudibleProductId(CalculateAsin(title + subtitle)),
			title,
			subtitle,
			description,
			lengthInMinutes,
			contentType,
			[new Contributor(firstAuthor, CalculateAsin(firstAuthor))],
			[new Contributor(firstNarrator, CalculateAsin(firstNarrator))],
			localeName);

		lastDlFormat ??= new AudioFormat(Codec.AAC_LC, 128, 44100, 2);
		lastDlVersion ??= new Version(13, 0);
		book.UserDefinedItem.SetLastDownloaded(lastDlVersion, lastDlFormat, "1");
		book.UserDefinedItem.PdfStatus = pdfStatus;
		book.UserDefinedItem.BookStatus = bookStatus;

		book.UpdateBookDetails(isAbridged, isSpatial, datePublished ?? DateTime.Now, language);

		return new MockLibraryBook(
			book,
			dateAdded ?? DateTime.Now,
			account,
			includedUntil,
			isAudiblePlus)
		{
			AbsentFromLastScan = absetFromLastScan
		};
	}

	private static string CalculateAsin(string name) 
		=> Convert.ToHexString(System.Security.Cryptography.MD5.HashData(Encoding.UTF8.GetBytes(name))).Substring(0, 10);
}
