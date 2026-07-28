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

	public static async Task<bool> BookExistsAsync(string serverUrl, string apiToken, string libraryId, string title, string? author = null)
	{
		apiToken = AudiobookshelfTokenStorage.DecryptToken(apiToken) ?? "";
		using var client = CreateClient(serverUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
		client.Timeout = TimeSpan.FromSeconds(30);

		var normalizedTitle = NormalizeTitle(title);
		var baseTitle = GetBaseTitle(normalizedTitle);

		Serilog.Log.Logger.Debug("Audiobookshelf duplicate check: looking for '{Title}' (base: '{BaseTitle}') in library {LibraryId}", normalizedTitle, baseTitle, libraryId);

		try
		{
			// Search with base title first (shorter = broader fuzzy matches, better recall)
			var candidates = await SearchLibraryAsync(client, libraryId, baseTitle, 20);

			// Also search with full title to catch cases where ABS stores the longer/subtitled version
			if (!string.Equals(normalizedTitle, baseTitle, StringComparison.Ordinal))
			{
				var fullCandidates = await SearchLibraryAsync(client, libraryId, normalizedTitle, 10);
				foreach (var c in fullCandidates)
				{
					if (!candidates.Any(existing => string.Equals(existing.Id, c.Id, StringComparison.Ordinal)))
						candidates.Add(c);
				}
			}

			if (candidates.Count == 0)
			{
				Serilog.Log.Logger.Debug("Audiobookshelf duplicate check: no candidates found for '{Title}' in library {LibraryId}", normalizedTitle, libraryId);
				return false;
			}

			Serilog.Log.Logger.Debug("Audiobookshelf duplicate check: found {Count} candidate(s) for '{Title}' in library {LibraryId}", candidates.Count, normalizedTitle, libraryId);

			foreach (var candidate in candidates)
			{
				if (string.IsNullOrWhiteSpace(candidate.Title))
					continue;

				bool titleMatch = false;

				// 1) Exact match against normalized title
				if (string.Equals(candidate.Title, normalizedTitle, StringComparison.OrdinalIgnoreCase))
					titleMatch = true;

				// 2) Exact match against base title (without subtitle)
				if (!titleMatch && string.Equals(candidate.Title, baseTitle, StringComparison.OrdinalIgnoreCase))
					titleMatch = true;

				// 3) Exact match against combined title:subtitle
				if (!titleMatch && !string.IsNullOrWhiteSpace(candidate.FullTitle)
					&& string.Equals(candidate.FullTitle, normalizedTitle, StringComparison.OrdinalIgnoreCase))
					titleMatch = true;

				if (titleMatch && AuthorsCompatible(author, candidate.Authors))
				{
					Serilog.Log.Logger.Information("Audiobookshelf duplicate check: found match for '{Title}' (id={ItemId})", title, candidate.Id);
					return true;
				}
			}

			Serilog.Log.Logger.Debug("Audiobookshelf duplicate check: '{Title}' not found in library {LibraryId}", normalizedTitle, libraryId);
			return false;
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Debug(ex, "Audiobookshelf duplicate check failed for '{Title}' in library {LibraryId}", title, libraryId);
			throw; // Let caller decide whether to proceed with upload
		}
	}

	private record SearchCandidate(string Id, string Title, string FullTitle, List<string> Authors);

	private static async Task<List<SearchCandidate>> SearchLibraryAsync(HttpClient client, string libraryId, string query, int limit)
	{
		var url = $"api/libraries/{Uri.EscapeDataString(libraryId)}/search?q={Uri.EscapeDataString(query)}&limit={limit}";
		var response = await client.GetAsync(url);
		if (!response.IsSuccessStatusCode)
		{
			var errorBody = await response.Content.ReadAsStringAsync();
			throw new HttpRequestException($"Audiobookshelf search returned {(int)response.StatusCode} for query '{query}'. Body: {errorBody}");
		}

		var json = await response.Content.ReadAsStringAsync();
		var obj = JObject.Parse(json);
		var books = obj["book"] as JArray ?? new JArray();

		var results = new List<SearchCandidate>();
		foreach (var entry in books)
		{
			var item = entry["libraryItem"] ?? entry;
			var id = item["id"]?.Value<string>() ?? entry["id"]?.Value<string>() ?? "";
			var itemTitle = item["media"]?["metadata"]?["title"]?.Value<string>()?.Replace("\u00A0", " ").Trim();
			var itemSubtitle = item["media"]?["metadata"]?["subtitle"]?.Value<string>()?.Replace("\u00A0", " ").Trim();

			var itemFullTitle = string.IsNullOrWhiteSpace(itemSubtitle)
				? itemTitle
				: $"{itemTitle}: {itemSubtitle}";

			var authors = item["media"]?["metadata"]?["authors"] as JArray;
			var authorList = authors?.Select(a => a["name"]?.Value<string>()?.Trim() ?? "")
				.Where(n => !string.IsNullOrWhiteSpace(n))
				.ToList() ?? [];

			results.Add(new SearchCandidate(
				id,
				NormalizeTitle(itemTitle),
				NormalizeTitle(itemFullTitle),
				authorList));
		}

		return results;
	}

	private static bool AuthorsCompatible(string? libationAuthor, List<string> absAuthors)
	{
		// If either side has no author info, we can't use authors to disambiguate
		if (string.IsNullOrWhiteSpace(libationAuthor))
			return true;
		if (absAuthors == null || absAuthors.Count == 0)
			return true;

		var libationAuthors = libationAuthor.Split(',', StringSplitOptions.RemoveEmptyEntries)
			.Select(a => NormalizeAuthor(a))
			.Where(a => !string.IsNullOrWhiteSpace(a))
			.ToList();

		if (libationAuthors.Count == 0)
			return true;

		foreach (var absAuthor in absAuthors.Select(NormalizeAuthor))
		{
			foreach (var la in libationAuthors)
			{
				if (string.Equals(absAuthor, la, StringComparison.OrdinalIgnoreCase))
					return true;
			}
		}

		return false;
	}

	private static string NormalizeAuthor(string? author)
	{
		if (string.IsNullOrWhiteSpace(author))
			return author ?? "";
		return author.Replace("\u00A0", " ").Trim();
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
			if (await BookExistsAsync(serverUrl, apiToken, libraryId, title, author))
			{
				Serilog.Log.Logger.Information("Skipping Audiobookshelf upload: book '{Title}' already exists in library {LibraryId}", title, libraryId);
				return UploadResult.AlreadyExists;
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Warning(ex, "Pre-check for existing book on Audiobookshelf failed; will still attempt upload");
			// Continue to upload attempt below
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
