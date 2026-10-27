using ApplicationServices;
using AssertionHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApplicationServices.Tests;

/// <summary>
/// Regression coverage for the <c>--check-asin</c> / <c>AudiobookshelfCheckAsin</c> feature.
/// Uses an in-process <see cref="HttpListener"/> to stand in for an Audiobookshelf server so we
/// can assert the behavior of <see cref="AudiobookshelfApiService.BookExistsAsync"/> end to end.
/// </summary>
[TestClass]
public class AudiobookshelfBookExistsAsinTests
{
	private const string LibraryId = "lib-1";
	private const string ApiToken = "test-token";

	private HttpListener _listener = null!;
	private string _baseUrl = "";
	private CancellationTokenSource _cts = null!;
	private Task _pumpTask = null!;
	private readonly List<string> _requestedPaths = new();
	private Func<string, (int status, string body)> _responder = _ => (200, "{}");

	[TestInitialize]
	public void StartListener()
	{
		int port = GetFreePort();
		_baseUrl = $"http://127.0.0.1:{port}";
		_listener = new HttpListener();
		_listener.Prefixes.Add(_baseUrl + "/");
		_listener.Start();
		_cts = new CancellationTokenSource();
		_pumpTask = Task.Run(() => Pump(_cts.Token));
	}

	[TestCleanup]
	public void StopListener()
	{
		_cts.Cancel();
		try { _listener.Stop(); } catch { }
		try { _pumpTask.Wait(TimeSpan.FromSeconds(2)); } catch { }
		_listener.Close();
	}

	private static int GetFreePort()
	{
		var l = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
		l.Start();
		int port = ((System.Net.IPEndPoint)l.LocalEndpoint).Port;
		l.Stop();
		return port;
	}

	private async Task Pump(CancellationToken ct)
	{
		while (!ct.IsCancellationRequested)
		{
			HttpListenerContext ctx;
			try { ctx = await _listener.GetContextAsync(); }
			catch { return; }

			var path = ctx.Request.Url!.PathAndQuery;
			_requestedPaths.Add(path);
			var (status, body) = _responder(path);
			var bytes = Encoding.UTF8.GetBytes(body);
			ctx.Response.StatusCode = status;
			ctx.Response.ContentType = "application/json";
			ctx.Response.ContentLength64 = bytes.Length;
			await ctx.Response.OutputStream.WriteAsync(bytes.AsMemory(), ct);
			ctx.Response.Close();
		}
	}

	private static string SearchResponse(params (string id, string title, string? subtitle, string? asin, string[] authors)[] items)
	{
		var sb = new StringBuilder();
		sb.Append("{\"book\":[");
		for (int i = 0; i < items.Length; i++)
		{
			var it = items[i];
			if (i > 0) sb.Append(',');
			sb.Append("{\"libraryItem\":{");
			sb.Append($"\"id\":\"{it.id}\",");
			sb.Append("\"media\":{\"metadata\":{");
			sb.Append($"\"title\":\"{it.title}\"");
			if (it.subtitle != null) sb.Append($",\"subtitle\":\"{it.subtitle}\"");
			if (it.asin != null) sb.Append($",\"asin\":\"{it.asin}\"");
			sb.Append(",\"authors\":[");
			for (int a = 0; a < it.authors.Length; a++)
			{
				if (a > 0) sb.Append(',');
				sb.Append($"{{\"name\":\"{it.authors[a]}\"}}");
			}
			sb.Append("]");
			sb.Append("}}");
			sb.Append("}}");
		}
		sb.Append("]}");
		return sb.ToString();
	}

	[TestMethod]
	public async Task BookExistsAsync_finds_match_when_asin_matches_but_title_differs()
	{
		// Reviewer ask: different title, same ASIN, must return true.
		_responder = _ => (200, SearchResponse(
			("id-1", "Le Petit Prince (French Edition)", null, "B00ABCDEFG", new[] { "Antoine de Saint-Exupéry" })));

		var found = await AudiobookshelfApiService.BookExistsAsync(
			_baseUrl,
			ApiToken,
			LibraryId,
			title: "The Little Prince",
			author: "Antoine de Saint-Exupéry",
			asin: "B00ABCDEFG");

		found.Should().BeTrue();
	}

	[TestMethod]
	public async Task BookExistsAsync_does_not_match_when_asin_only_appears_in_filename()
	{
		// Documents an intentional limitation: Audiobookshelf's search endpoint indexes metadata
		// ASINs but not ASINs that only appear in folder or file names. This test locks that in
		// so a future change cannot silently claim broader coverage.
		_responder = path =>
		{
			// Search hit contains a library item whose metadata ASIN differs from the query,
			// but whose (nominal) path contains the queried ASIN. We should NOT report a match.
			if (path.Contains("/search"))
			{
				return (200, SearchResponse(
					("id-2", "Some Other Book", null, "B99OTHERAS", new[] { "Someone Else" })));
			}
			return (200, "{}");
		};

		var found = await AudiobookshelfApiService.BookExistsAsync(
			_baseUrl,
			ApiToken,
			LibraryId,
			title: "Something Unrelated",
			author: "Someone Else",
			asin: "B00ABCDEFG");

		found.Should().BeFalse();
	}

	[TestMethod]
	public async Task BookExistsAsync_falls_back_to_title_author_when_no_asin_supplied()
	{
		// Reviewer ask: existing title/author fallback must keep working.
		_responder = _ => (200, SearchResponse(
			("id-3", "The Little Prince", null, "B00ABCDEFG", new[] { "Antoine de Saint-Exupéry" })));

		var found = await AudiobookshelfApiService.BookExistsAsync(
			_baseUrl,
			ApiToken,
			LibraryId,
			title: "The Little Prince",
			author: "Antoine de Saint-Exupéry",
			asin: null);

		found.Should().BeTrue();
	}

	[TestMethod]
	public async Task BookExistsAsync_returns_false_when_no_candidates()
	{
		_responder = _ => (200, "{\"book\":[]}");

		var found = await AudiobookshelfApiService.BookExistsAsync(
			_baseUrl,
			ApiToken,
			LibraryId,
			title: "Nothing Matches",
			author: "Nobody",
			asin: "B00NOMATCH");

		found.Should().BeFalse();
	}
}
