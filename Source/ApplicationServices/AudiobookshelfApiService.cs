using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationServices;

public static class AudiobookshelfApiService
{
	public record Library(string Id, string Name, string MediaType, List<Folder> Folders);
	public record Folder(string Id, string FullPath);
	public enum UploadResult { Success, AlreadyExists, Failed }

	private static HttpClient CreateClient(string serverUrl)
	{
		var client = new HttpClient();
		client.BaseAddress = new Uri(serverUrl.TrimEnd('/') + "/");
		client.Timeout = TimeSpan.FromSeconds(30);
		return client;
	}

	public static async Task<List<Library>> GetLibrariesAsync(string serverUrl, string apiToken)
	{
		apiToken = AudiobookshelfTokenStorage.DecryptToken(apiToken) ?? "";
		using var client = CreateClient(serverUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

		var response = await client.GetAsync("api/libraries");
		if (!response.IsSuccessStatusCode)
		{
			var errorBody = await response.Content.ReadAsStringAsync();
			throw new HttpRequestException($"Audiobookshelf API returned {(int)response.StatusCode} ({response.StatusCode}) when fetching libraries. Response: {errorBody}");
		}

		var json = await response.Content.ReadAsStringAsync();
		var obj = JObject.Parse(json);
		var libraries = obj["libraries"] as JArray ?? new JArray();

		var allLibraries = libraries.Select(l => new Library(
			l["id"]?.Value<string>() ?? "",
			l["name"]?.Value<string>() ?? "",
			l["mediaType"]?.Value<string>() ?? "book",
			(l["folders"] as JArray ?? new JArray())
				.Select(f => new Folder(
					f["id"]?.Value<string>() ?? "",
					f["fullPath"]?.Value<string>() ?? ""))
				.ToList()))
			.ToList();

		// Only return libraries with book media type
		return allLibraries.Where(l => string.Equals(l.MediaType, "book", StringComparison.OrdinalIgnoreCase)).ToList();
	}

	public static async Task<bool> BookExistsAsync(string serverUrl, string apiToken, string libraryId, string title)
	{
		apiToken = AudiobookshelfTokenStorage.DecryptToken(apiToken) ?? "";
		using var client = CreateClient(serverUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
		client.Timeout = TimeSpan.FromMinutes(2);

		var normalizedTitle = NormalizeTitle(title);
		var baseTitle = GetBaseTitle(normalizedTitle);

		int page = 0;
		const int limit = 500;
		const int maxPages = 200;

		Serilog.Log.Logger.Information("Audiobookshelf duplicate check: looking for '{Title}' (base: '{BaseTitle}') in library {LibraryId}", normalizedTitle, baseTitle, libraryId);

		while (page < maxPages)
		{
			var url = $"api/libraries/{Uri.EscapeDataString(libraryId)}/items?minified=1&limit={limit}&page={page}";
			var response = await client.GetAsync(url);
			if (!response.IsSuccessStatusCode)
			{
				var errorBody = await response.Content.ReadAsStringAsync();
				throw new HttpRequestException($"Audiobookshelf API returned {(int)response.StatusCode} when checking for existing books. Body: {errorBody}");
			}

			var json = await response.Content.ReadAsStringAsync();
			var obj = JObject.Parse(json);
			var results = obj["results"] as JArray ?? new JArray();

			Serilog.Log.Logger.Information("Audiobookshelf duplicate check page {Page}: received {Count} items (total={Total})", page, results.Count, obj["total"]?.Value<int>() ?? -1);

			if (results.Count == 0)
				break;

			foreach (var item in results)
			{
				var itemTitle = item["media"]?["metadata"]?["title"]?.Value<string>()?.Replace("\u00A0", " ").Trim();
				var itemSubtitle = item["media"]?["metadata"]?["subtitle"]?.Value<string>()?.Replace("\u00A0", " ").Trim();

				var itemFullTitle = string.IsNullOrWhiteSpace(itemSubtitle)
					? itemTitle
					: $"{itemTitle}: {itemSubtitle}";

				var normalizedItemTitle = NormalizeTitle(itemTitle);
				var normalizedItemFullTitle = NormalizeTitle(itemFullTitle);

				Serilog.Log.Logger.Information("Audiobookshelf duplicate check: comparing search='{SearchTitle}'/'{BaseTitle}' against ABS '{ItemTitle}'/'{ItemFullTitle}' (normalized: '{NormItemTitle}'/'{NormItemFull}')", normalizedTitle, baseTitle, itemTitle, itemFullTitle, normalizedItemTitle, normalizedItemFullTitle);

				if (!string.IsNullOrWhiteSpace(normalizedItemTitle))
				{
					// 1) Exact match against normalized full title
					if (string.Equals(normalizedItemTitle, normalizedTitle, StringComparison.OrdinalIgnoreCase))
						return true;

					// 2) Exact match against base title (without subtitle)
					if (string.Equals(normalizedItemTitle, baseTitle, StringComparison.OrdinalIgnoreCase))
						return true;

					// 3) Match against combined title:subtitle
					if (!string.IsNullOrWhiteSpace(normalizedItemFullTitle)
						&& string.Equals(normalizedItemFullTitle, normalizedTitle, StringComparison.OrdinalIgnoreCase))
						return true;

					// 4) Substring fallback: ABS title contains our base title (handles "The Hobbit" vs "The Hobbit: 75th Anniversary Edition")
					if (!string.IsNullOrWhiteSpace(baseTitle)
						&& baseTitle.Length > 5
						&& normalizedItemTitle.Contains(baseTitle, StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}

			// CRITICAL FIX: use results.Count < limit as primary end-of-pagination indicator.
			// Do NOT fall back to results.Count for total — that caused breaking after page 1
			// when the first page had exactly 'limit' items.
			if (results.Count < limit)
				break;

			// Secondary: if API provides total, use it to avoid a blank-page round-trip
			if (obj["total"]?.Value<int>() is int total && (page + 1) * limit >= total)
				break;

			page++;
		}

		Serilog.Log.Logger.Information("Audiobookshelf duplicate check: '{SearchTitle}' not found in library {LibraryId}", normalizedTitle, libraryId);
		return false;
	}

	private static string NormalizeTitle(string? title)
	{
		if (string.IsNullOrWhiteSpace(title))
			return title ?? "";

		var normalized = title
			.Replace("\u00A0", " ")
			.Replace("(Unabridged)", "", StringComparison.OrdinalIgnoreCase)
			.Replace("(Abridged)", "", StringComparison.OrdinalIgnoreCase)
			.Trim();

		// Collapse multiple spaces to single space
		while (normalized.Contains("  ", StringComparison.Ordinal))
			normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);

		return normalized;
	}

	private static string GetBaseTitle(string fullTitle)
	{
		if (string.IsNullOrWhiteSpace(fullTitle))
			return fullTitle;

		var colonIndex = fullTitle.IndexOf(": ", StringComparison.Ordinal);
		return colonIndex > 0 ? fullTitle[..colonIndex].Trim() : fullTitle;
	}

	public static async Task<UploadResult> UploadBookAsync(
		string serverUrl,
		string apiToken,
		string libraryId,
		string folderId,
		string title,
		string? author,
		string? series,
		IEnumerable<string> filePaths)
	{
		apiToken = AudiobookshelfTokenStorage.DecryptToken(apiToken) ?? "";

		// Pre-check for existing item
		try
		{
			if (await BookExistsAsync(serverUrl, apiToken, libraryId, title))
			{
				Serilog.Log.Logger.Information("Skipping Audiobookshelf upload: book '{Title}' already exists in library {LibraryId}", title, libraryId);
				return UploadResult.AlreadyExists;
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Pre-check for existing book on Audiobookshelf failed; aborting upload");
			return UploadResult.Failed;
		}

		using var client = CreateClient(serverUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
		client.Timeout = TimeSpan.FromMinutes(30);

		using var form = new MultipartFormDataContent();
		form.Add(new StringContent(title), "title");

		if (!string.IsNullOrWhiteSpace(author))
			form.Add(new StringContent(author), "author");
		if (!string.IsNullOrWhiteSpace(series))
			form.Add(new StringContent(series), "series");

		form.Add(new StringContent(libraryId), "library");
		form.Add(new StringContent(folderId), "folder");

		int fileIndex = 0;
		var streams = new List<Stream>();
		try
		{
			foreach (var path in filePaths.Where(File.Exists))
			{
				var stream = File.OpenRead(path);
				streams.Add(stream);
				var fileContent = new StreamContent(stream);
				fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
				form.Add(fileContent, fileIndex.ToString(), Path.GetFileName(path));
				fileIndex++;
			}

			if (fileIndex == 0)
			{
				Serilog.Log.Logger.Warning("No audio files found to upload to Audiobookshelf for '{Title}'", title);
				return UploadResult.Failed;
			}

			var response = await client.PostAsync("api/upload", form);

			if (response.IsSuccessStatusCode)
				return UploadResult.Success;

			var responseBody = await response.Content.ReadAsStringAsync();
			Serilog.Log.Logger.Error("Audiobookshelf upload failed for '{Title}' with status {(int)response.StatusCode} ({StatusCode}). Response body: {ResponseBody}",
				title, (int)response.StatusCode, response.StatusCode, responseBody);

			// Treat "already exists" 500 errors as skip/success
			if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError
				&& responseBody.Contains("already exists", StringComparison.OrdinalIgnoreCase))
			{
				return UploadResult.AlreadyExists;
			}

			return UploadResult.Failed;
		}
		finally
		{
			foreach (var stream in streams)
			{
				try { stream.Dispose(); } catch { /* ignored */ }
			}
		}
	}
}
