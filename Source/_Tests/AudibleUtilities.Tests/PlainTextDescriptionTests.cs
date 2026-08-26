using AssertionHelper;
using AudibleApi.Common;
using AudibleUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AudibleUtilities.Tests;

/// <summary>
/// Audible sends a book's summary as HTML. It is flattened here, at the API boundary, so nothing
/// downstream has to know that. Reported in issue #2002, where the markup reached a file's metadata
/// tags because the only stripping Libation did was for the library grid.
/// </summary>
[TestClass]
public class PlainTextDescriptionTests
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
		=> HtmlText.ToPlainText(html).Should().Be(expected);

	[TestMethod]
	public void SeparatorIsConfigurable()
		=> HtmlText.ToPlainText("<p>First.</p> <p>Second.</p>", paragraphSeparator: "\r\n\r\n")
			.Should().Be("First.\r\n\r\nSecond.");

	/// <summary>
	/// Unmarked summaries skip the parser, so a stray angle bracket cannot swallow the sentence after
	/// it. Entities are still decoded, since Audible writes those into plain text too.
	/// </summary>
	[TestMethod]
	public void TextWithoutTagsIsNotParsed()
	{
		HtmlText.ToPlainText("5 > 3 and 2 < 4").Should().Be("5 > 3 and 2 < 4");
		HtmlText.ToPlainText("Tom &amp; Jerry").Should().Be("Tom & Jerry");
	}

	[TestMethod]
	public void ReadsTheItemsPublisherSummary()
	{
		var item = new Item { PublisherSummary = "<p>Hello.</p> <p>World.</p>" };

		item.PlainTextDescription().Should().Be("Hello.\nWorld.");
	}

	[TestMethod]
	public void AnItemWithNoSummaryFlattensToEmpty()
		=> new Item().PlainTextDescription().Should().Be("");
}
