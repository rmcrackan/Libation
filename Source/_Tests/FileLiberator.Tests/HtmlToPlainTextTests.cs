using AssertionHelper;
using DataLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileLiberator.Tests;

[TestClass]
public class HtmlToPlainTextTests
{
	[TestMethod]
	[DataRow(null, "")]
	[DataRow("", "")]
	[DataRow("   ", "")]
	[DataRow("Already plain text.", "Already plain text.")]
	[DataRow("  Padded plain text.  ", "Padded plain text.")]
	[DataRow("<p>One paragraph.</p>", "One paragraph.")]
	[DataRow("<p>First.</p> <p>Second.</p>", "First.\nSecond.")]
	[DataRow("<p>First.</p><p>Second.</p>", "First.\nSecond.")]
	[DataRow("<p><b>Bold</b> and <i>italic</i>.</p>", "Bold and italic.")]
	[DataRow("Line one.<br>Line two.", "Line one.\nLine two.")]
	[DataRow("Line one.<br />Line two.", "Line one.\nLine two.")]
	[DataRow("<ul><li>One</li><li>Two</li></ul>", "One\nTwo")]
	[DataRow("<p>Tom &amp; Jerry</p>", "Tom & Jerry")]
	[DataRow("<p>&#169;2024 Someone</p>", "\u00a92024 Someone")]
	[DataRow("<p>Blank</p><p></p><p>lines dropped</p>", "Blank\nlines dropped")]
	public void Flattens(string? html, string expected)
		=> EntityExtensions.HtmlToPlainText(html).Should().Be(expected);

	[TestMethod]
	public void SeparatorIsConfigurable()
		=> EntityExtensions.HtmlToPlainText("<p>First.</p> <p>Second.</p>", paragraphSeparator: "\r\n\r\n")
			.Should().Be("First.\r\n\r\nSecond.");

	/// <summary>
	/// Unmarked descriptions skip the parser, so a stray angle bracket cannot swallow the sentence
	/// after it. Entities are still decoded, since Audible writes those into plain text too.
	/// </summary>
	[TestMethod]
	public void TextWithoutTagsIsNotParsed()
	{
		EntityExtensions.HtmlToPlainText("5 > 3 and 2 < 4").Should().Be("5 > 3 and 2 < 4");
		EntityExtensions.HtmlToPlainText("Tom &amp; Jerry").Should().Be("Tom & Jerry");
	}

	[TestMethod]
	public void ReadsTheBooksDescription()
	{
		var libraryBook = MockLibraryBook.CreateBook(description: "<p>Hello.</p> <p>World.</p>");

		libraryBook.Book.DescriptionAsPlainText().Should().Be("Hello.\nWorld.");
	}
}
