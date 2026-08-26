using AssertionHelper;
using AudibleApi.Common;
using DataLayer;
using LibationFileManager.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mpeg4Lib;
using Mpeg4Lib.Boxes;
using System;
using System.IO;

namespace FileLiberator.Tests;

/// <summary>
/// Covers the two shapes of file Audible delivers. Its ADRM (.aaxc) downloads arrive with the
/// descriptive tags already written; its Widevine (DASH) downloads arrive with an empty tag list, so
/// everything has to come from the library instead. See issue #2002.
/// </summary>
[TestClass]
public class FillMissingTagsTests
{
	/// <summary>Audible's publisher_summary for the title in issue #2002, trimmed to two paragraphs.</summary>
	private const string PublisherSummaryHtml
		= "<p>When he was a little boy, there was a house behind Alex Lowry's house. "
		+ "Except he was the only one who could see it.</p> "
		+ "<p>Decades later, after the pandemic costs Alex his corporate job, he picks up work "
		+ "delivering specialty items &amp; homebound clientele.</p>";

	private const string PublisherSummaryFlattened
		= "When he was a little boy, there was a house behind Alex Lowry's house. "
		+ "Except he was the only one who could see it.\n"
		+ "Decades later, after the pandemic costs Alex his corporate job, he picks up work "
		+ "delivering specialty items & homebound clientele.";

	private const string CatalogCopyright = "\u00a92024 Bentley Little (P)2025 Journalstone";

	private static readonly DateTime ReleaseDate = new(2025, 11, 17);

	#region The Widevine / DASH file: nothing embedded, everything falls back to the library

	[TestMethod]
	public void EmptyTags_DescriptionIsFlattenedToPlainText()
	{
		var tags = EmptyTags();

		FillMissingTags(tags);

		tags.Comment.Should().Be(PublisherSummaryFlattened);
		tags.LongDescription.Should().Be(PublisherSummaryFlattened);
	}

	[TestMethod]
	public void EmptyTags_CopyrightComesFromTheCatalog()
	{
		var tags = EmptyTags();

		FillMissingTags(tags);

		tags.Copyright.Should().Be(CatalogCopyright);
	}

	[TestMethod]
	public void EmptyTags_ReleaseDateComesFromTheCatalog()
	{
		var tags = EmptyTags();

		FillMissingTags(tags);

		tags.ReleaseDate.Should().Be("17-Nov-2025");
		tags.Year.Should().Be("2025");
	}

	#endregion

	#region The AAXC file: Audible's own tags are left alone

	[TestMethod]
	public void EmbeddedTags_AudiblesOwnDescriptionAndCopyrightSurvive()
	{
		const string embeddedComment = "A short blurb Audible wrote.";
		const string embeddedLongDescription = "The full description Audible wrote.";
		const string embeddedCopyright = "\u00a92024 Someone Else (P)2025 Some Publisher";

		var tags = EmptyTags();
		tags.Comment = embeddedComment;
		tags.LongDescription = embeddedLongDescription;
		tags.Copyright = embeddedCopyright;

		FillMissingTags(tags);

		tags.Comment.Should().Be(embeddedComment);
		tags.LongDescription.Should().Be(embeddedLongDescription);
		tags.Copyright.Should().Be(embeddedCopyright);
	}

	[TestMethod]
	public void EmbeddedTags_PlaceholderReleaseDateIsReplaced()
	{
		var tags = EmptyTags();
		tags.ReleaseDate = "01-Jan-2000";
		tags.Year = "2000";

		FillMissingTags(tags);

		tags.ReleaseDate.Should().Be("17-Nov-2025");
		tags.Year.Should().Be("2025");
	}

	[TestMethod]
	public void EmbeddedTags_RealReleaseDateIsKept()
	{
		var tags = EmptyTags();
		tags.ReleaseDate = "13-Sep-2016";
		tags.Year = "2016";

		FillMissingTags(tags);

		tags.ReleaseDate.Should().Be("13-Sep-2016");
		tags.Year.Should().Be("2016");
	}

	[TestMethod]
	public void EmbeddedTags_PlaceholderIsKeptWhenTheCatalogAgreesWithIt()
	{
		var tags = EmptyTags();
		tags.ReleaseDate = "01-Jan-2000";
		tags.Year = "2000";

		FillMissingTags(tags, CatalogBook(new DateTime(2000, 1, 1)));

		tags.ReleaseDate.Should().Be("01-Jan-2000");
		tags.Year.Should().Be("2000");
	}

	[TestMethod]
	public void NoCatalogDate_LeavesThePlaceholderAlone()
	{
		var tags = EmptyTags();
		tags.ReleaseDate = "01-Jan-2000";
		tags.Year = "2000";

		FillMissingTags(tags, BookWithoutPublicationDate());

		tags.ReleaseDate.Should().Be("01-Jan-2000");
		tags.Year.Should().Be("2000");
	}

	#endregion

	[TestMethod]
	public void DrmTypeIsRecordedAsAFreeformTag()
	{
		var tags = EmptyTags();

		FillMissingTags(tags, drmType: DrmType.Widevine);

		tags.AppleListBox.GetFreeformTagString("org.libation", "AUDIBLE_DRM_TYPE").Should().Be("Widevine");
	}

	private static void FillMissingTags(MetadataItems tags, Book? book = null, DrmType drmType = DrmType.Adrm)
	{
		book ??= CatalogBook(ReleaseDate);
		DownloadDecryptBook.FillMissingTags(
			tags,
			book,
			new LibraryBookDto { AudibleProductId = book.AudibleProductId, TitleWithSubtitle = "Behind" },
			ContentReference(),
			drmType);
	}

	private static Book CatalogBook(DateTime datePublished)
		=> MockLibraryBook.CreateBook(
			title: "Behind",
			subtitle: "",
			description: PublisherSummaryHtml,
			datePublished: datePublished,
			copyright: CatalogCopyright)
			.Book;

	/// <summary>A title Audible's catalog has no publication or issue date for.</summary>
	private static Book BookWithoutPublicationDate()
		=> new(
			new AudibleProductId("B0G2N23FC4"),
			"Behind",
			"",
			PublisherSummaryHtml,
			646,
			DataLayer.ContentType.Product,
			[new Contributor("Bentley Little", "B001IR18G4")],
			[new Contributor("Nicholas Selker", "B0DEADBEEF")],
			"us");

	/// <summary>An <c>ilst</c> box with no tags in it, as a Widevine download delivers.</summary>
	private static MetadataItems EmptyTags()
	{
		byte[] emptyIlstBox = [0, 0, 0, 8, (byte)'i', (byte)'l', (byte)'s', (byte)'t'];
		var stream = new MemoryStream(emptyIlstBox);
		return new MetadataItems(new AppleListBox(stream, new BoxHeader(stream), parent: null));
	}

	private static ContentReference ContentReference()
		=> new()
		{
			Acr = "CR!ACR",
			Asin = "B0G2N23FC4",
			Codec = "mp4a",
			ContentFormat = "MPEG4_44_128",
			Marketplace = "AF2M0KC94RCEA",
			Sku = "BK_JOUR_000123",
			Tempo = "1.0",
			Version = "1"
		};
}
