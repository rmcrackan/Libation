using AssertionHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LibationCli.Tests;

[TestClass]
public class ConsoleProgressBarTests
{
	[TestMethod]
	public void WriteProgress_starts_with_carriage_return_to_overwrite_line()
	{
		using var writer = new StringWriter();
		var sut = new ConsoleProgressBar(writer, maxWidth: 40);

		sut.Progress = 50;

		var output = writer.ToString();
		Assert.IsTrue(output.StartsWith("\r", StringComparison.Ordinal));
		StringAssert.Contains(output, "[");
		StringAssert.Contains(output, "]");
		StringAssert.Contains(output, "ETA");
	}

	[TestMethod]
	public void Successive_updates_start_with_carriage_return()
	{
		using var writer = new StringWriter();
		var sut = new ConsoleProgressBar(writer, maxWidth: 40);

		sut.Progress = 25;
		sut.Progress = 50;

		var output = writer.ToString();
		var carriageReturns = output.Split('\r').Length - 1;
		carriageReturns.Should().Be(2);
	}

	[TestMethod]
	public void Clear_overwrites_line_with_spaces_and_resets_cursor()
	{
		using var writer = new StringWriter();
		var sut = new ConsoleProgressBar(writer, maxWidth: 40);

		sut.Progress = 50;
		sut.Clear();

		var output = writer.ToString();
		Assert.IsTrue(output.EndsWith("\r", StringComparison.Ordinal));
	}
}
