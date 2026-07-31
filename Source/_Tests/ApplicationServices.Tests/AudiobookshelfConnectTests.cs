using ApplicationServices;
using AssertionHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ApplicationServices.Tests;

[TestClass]
public class AudiobookshelfConnectTests
{
	[TestMethod]
	[DataRow(null, null)]
	[DataRow("", "token")]
	[DataRow("http://localhost:13378", "")]
	[DataRow("   ", "   ")]
	public async Task ConnectAsync_blank_url_or_token_returns_validation_message(string? url, string? token)
	{
		var result = await AudiobookshelfApiService.ConnectAsync(url, token);

		result.Success.Should().BeFalse();
		result.StatusMessage.Should().Be("Please enter both server URL and API token.");
		result.NormalizedServerUrl.Should().BeNull();
		result.ServerUrlAdjusted.Should().BeFalse();
		result.Libraries.Should().HaveCount(0);
	}

	[TestMethod]
	public async Task ConnectAsync_invalid_url_returns_connection_failed_without_throwing()
	{
		var result = await AudiobookshelfApiService.ConnectAsync("ftp://localhost:13378", "token");

		result.Success.Should().BeFalse();
		Assert.IsTrue(result.StatusMessage.StartsWith("Connection failed:", StringComparison.Ordinal));
		result.Libraries.Should().HaveCount(0);
	}

	[TestMethod]
	[DataRow(1, false, "Connected. Found 1 library.")]
	[DataRow(2, false, "Connected. Found 2 libraries.")]
	[DataRow(1, true, "Connected. Found 1 library. Server URL adjusted to the API base address.")]
	[DataRow(3, true, "Connected. Found 3 libraries. Server URL adjusted to the API base address.")]
	public void FormatConnectedStatus_pluralizes_and_notes_url_adjustment(int count, bool urlAdjusted, string expected)
	{
		AudiobookshelfApiService.FormatConnectedStatus(count, urlAdjusted).Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null, "")]
	[DataRow("", "")]
	[DataRow("   ", "")]
	[DataRow("http://localhost:13378/library/abc", "http://localhost:13378")]
	[DataRow("localhost:13378", "http://localhost:13378")]
	[DataRow("ftp://localhost:13378", "ftp://localhost:13378")]
	[DataRow("  http://localhost:13378/  ", "http://localhost:13378")]
	public void TryNormalizeServerUrlForSave_normalizes_or_keeps_trimmed(string? input, string expected)
	{
		AudiobookshelfApiService.TryNormalizeServerUrlForSave(input).Should().Be(expected);
	}
}
