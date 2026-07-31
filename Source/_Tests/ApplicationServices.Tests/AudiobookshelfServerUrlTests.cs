using ApplicationServices;
using AssertionHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace ApplicationServices.Tests;

[TestClass]
public class AudiobookshelfServerUrlTests
{
	[TestMethod]
	[DataRow("http://localhost:13378", "http://localhost:13378")]
	[DataRow("http://localhost:13378/", "http://localhost:13378")]
	[DataRow("https://abs.example.com", "https://abs.example.com")]
	[DataRow("https://abs.example.com/", "https://abs.example.com")]
	[DataRow("localhost:13378", "http://localhost:13378")]
	[DataRow("http://localhost:13378/audiobookshelf", "http://localhost:13378/audiobookshelf")]
	[DataRow("http://localhost:13378/audiobookshelf/", "http://localhost:13378/audiobookshelf")]
	public void Normalize_keeps_valid_base_urls(string input, string expected)
	{
		AudiobookshelfApiService.NormalizeServerUrl(input).Should().Be(expected);
	}

	[TestMethod]
	[DataRow("http://localhost:13378/library/abc-123", "http://localhost:13378")]
	[DataRow("http://localhost:13378/library/abc-123/bookshelf", "http://localhost:13378")]
	[DataRow("http://localhost:13378/audiobookshelf/library/abc-123", "http://localhost:13378/audiobookshelf")]
	[DataRow("http://localhost:13378/audiobookshelf/library/abc-123/bookshelf", "http://localhost:13378/audiobookshelf")]
	[DataRow("https://abs.example.com/item/book-id", "https://abs.example.com")]
	[DataRow("https://abs.example.com/collection/col-id", "https://abs.example.com")]
	[DataRow("https://abs.example.com/playlist/pl-id", "https://abs.example.com")]
	[DataRow("https://abs.example.com/author/au-id", "https://abs.example.com")]
	[DataRow("https://abs.example.com/series/se-id", "https://abs.example.com")]
	[DataRow("https://abs.example.com/config", "https://abs.example.com")]
	[DataRow("https://abs.example.com/login", "https://abs.example.com")]
	[DataRow("https://abs.example.com/account", "https://abs.example.com")]
	public void Normalize_strips_browser_client_routes(string input, string expected)
	{
		AudiobookshelfApiService.NormalizeServerUrl(input).Should().Be(expected);
	}

	[TestMethod]
	[DataRow("http://localhost:13378/api", "http://localhost:13378")]
	[DataRow("http://localhost:13378/api/", "http://localhost:13378")]
	[DataRow("http://localhost:13378/api/libraries", "http://localhost:13378")]
	[DataRow("http://localhost:13378/audiobookshelf/api/libraries", "http://localhost:13378/audiobookshelf")]
	public void Normalize_strips_api_path(string input, string expected)
	{
		AudiobookshelfApiService.NormalizeServerUrl(input).Should().Be(expected);
	}

	[TestMethod]
	[DataRow("http://localhost:13378/library/abc?foo=bar", "http://localhost:13378")]
	[DataRow("http://localhost:13378/library/abc#section", "http://localhost:13378")]
	[DataRow("  http://localhost:13378/library/abc  ", "http://localhost:13378")]
	public void Normalize_strips_query_fragment_and_whitespace(string input, string expected)
	{
		AudiobookshelfApiService.NormalizeServerUrl(input).Should().Be(expected);
	}

	[TestMethod]
	[DataRow(null)]
	[DataRow("")]
	[DataRow("   ")]
	[DataRow("ftp://localhost:13378")]
	public void Normalize_rejects_invalid_urls(string? input)
	{
		Assert.ThrowsExactly<ArgumentException>(() => AudiobookshelfApiService.NormalizeServerUrl(input));
	}
}
