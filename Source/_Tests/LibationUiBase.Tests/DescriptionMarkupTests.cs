using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LibationUiBase.Tests;

[TestClass]
public class DescriptionMarkupTests
{
	private static DescriptionRun Plain(string t) => new(t, false, false);
	private static DescriptionRun B(string t) => new(t, true, false);
	private static DescriptionRun BI(string t) => new(t, true, true);

	private static void AssertParagraphs(string? html, params DescriptionRun[][] expected)
	{
		var actual = DescriptionMarkup.Parse(html);
		Assert.HasCount(expected.Length, actual);
		for (var i = 0; i < expected.Length; i++)
			CollectionAssert.AreEqual(expected[i], actual[i].ToArray(), $"paragraph {i}");
	}

	[TestMethod]
	public void null_and_blank_are_empty()
	{
		AssertParagraphs(null);
		AssertParagraphs("");
		AssertParagraphs("  \n ");
	}

	[TestMethod]
	public void plain_text_splits_on_newlines()
		=> AssertParagraphs("one\ntwo\r\n\nthree", [Plain("one")], [Plain("two")], [Plain("three")]);

	[TestMethod]
	public void paragraph_tags_make_paragraphs()
		=> AssertParagraphs("<p>a</p> <p>b</p>", [Plain("a")], [Plain("b")]);

	[TestMethod]
	public void audible_sample()
		=> AssertParagraphs(
			"<p><b>Blindsided by betrayal in pre-WWII England, a woman charts a daring new course by the best-selling author of </b><b><i>The Rose Arbor</i></b><b> and </b><b><i>The Venice Sketchbook</i></b><b>.</b></p> <p>Surrey, England, 1938. After 30 devoted years&#8212;</p>",
			[
				B("Blindsided by betrayal in pre-WWII England, a woman charts a daring new course by the best-selling author of "),
				BI("The Rose Arbor"),
				B(" and "),
				BI("The Venice Sketchbook"),
				B("."),
			],
			[Plain("Surrey, England, 1938. After 30 devoted years—")]);

	[TestMethod]
	public void strong_and_em_style_runs()
		=> AssertParagraphs("a <strong>b</strong> <em>c</em>", [Plain("a "), B("b "), new("c", false, true)]);

	[TestMethod]
	public void br_splits_and_style_carries_over()
		=> AssertParagraphs("<b>a<br/>b</b>", [B("a")], [B("b")]);

	[TestMethod]
	public void unknown_tags_keep_text()
		=> AssertParagraphs("<span class=\"x\">kept</span>", [Plain("kept")]);

	[TestMethod]
	public void entities_are_decoded()
		=> AssertParagraphs("<p>Tom &amp; Jerry</p>", [Plain("Tom & Jerry")]);

	[TestMethod]
	public void stray_closer_is_ignored()
		=> AssertParagraphs("a</b> b", [Plain("a b")]);

	[TestMethod]
	public void unclosed_tag_stays_active_to_end()
		=> AssertParagraphs("<b>a</i> b", [B("a b")]);

	[TestMethod]
	public void whitespace_is_collapsed_and_trimmed()
		=> AssertParagraphs("<p>  a \t  b  </p>", [Plain("a b")]);

	[TestMethod]
	public void adjacent_same_style_runs_merge()
		=> AssertParagraphs("<b>a</b><b>b</b>", [B("ab")]);

	[TestMethod]
	public void malformed_input_does_not_throw()
	{
		var parsed = DescriptionMarkup.Parse("5 < 3 and <b");
		StringAssert.Contains(parsed[0][0].Text, "5");
		Assert.IsNotNull(DescriptionMarkup.Parse("a > b"));
	}
}
