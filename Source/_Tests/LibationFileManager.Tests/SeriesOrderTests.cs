using AssertionHelper;
using LibationFileManager.Templates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace SeriesOrderTests;

/// <summary>
/// Unformatted series numbers must keep the original digits. Parsing them as float used to
/// print 2147483647 as 2.1474836E+09 and collide different values (issue #2024).
/// </summary>
[TestClass]
public class Parse
{
	[TestMethod]
	[DataRow("1", "1")]
	[DataRow("406", "406")]
	[DataRow("1-6", "1-6")]
	[DataRow("2147483647", "2147483647")]
	[DataRow(" 1 6 ", "1 6")]
	public void unformatted_keeps_the_original_digits(string order, string expected)
		=> SeriesOrder.Parse(order).ToString().Should().Be(expected);

	[TestMethod]
	public void a_numeric_format_still_applies_to_each_number_part()
		=> SeriesOrder.Parse("1-6").ToString("F2", CultureInfo.InvariantCulture).Should().Be("1.00-6.00");

	[TestMethod]
	public void a_numeric_format_does_not_round_a_large_integer()
		=> SeriesOrder.Parse("2147483647").ToString("F0", CultureInfo.InvariantCulture).Should().Be("2147483647");
}
