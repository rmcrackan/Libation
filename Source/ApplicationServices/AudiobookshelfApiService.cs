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
	public record Library(string Id, string Name, List<Folder> Folders);
	public record Folder(string Id, string FullPath);
	public record LoginResponse(string Token);

	private static HttpClient CreateClient(string serverUrl)
	{
		var client = new HttpClient();
		client.BaseAddress = new Uri(serverUrl.TrimEnd('/') + "/");
		client.Timeout = TimeSpan.FromSeconds(30);
		return client;
	}

	public static async Task<LoginResponse?> LoginAsync(string serverUrl, string username, string password)
	{
		using var client = CreateClient(serverUrl);
		var content = new StringContent(
			$"{{\"username\":\"{JsonEscape(username)}\",\"password\":\"{JsonEscape(password)}\"}}",
			Encoding.UTF8,
			"application/json");

		var response = await client.PostAsync("login", content);
		if (!response.IsSuccessStatusCode)
			return null;

		var json = await response.Content.ReadAsStringAsync();
		var obj = JObject.Parse(json);
		var token = obj["user"]?["token"]?.Value<string>();

		return token is null ? null : new LoginResponse(token);
	}

	public static async Task<List<Library>> GetLibrariesAsync(string serverUrl, string apiToken)
	{
		using var client = CreateClient(serverUrl);
		client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

		var response = await client.GetAsync("api/libraries");
		if (!response.IsSuccessStatusCode)
			return [];

		var json = await response.Content.ReadAsStringAsync();
		var obj = JObject.Parse(json);
		var libraries = obj["libraries"] as JArray ?? new JArray();

		return libraries.Select(l => new Library(
			l["id"]?.Value<string>() ?? "",
			l["name"]?.Value<string>() ?? "",
			(l["folders"] as JArray ?? new JArray())
				.Select(f => new Folder(
					f["id"]?.Value<string>() ?? "",
					f["fullPath"]?.Value<string>() ?? ""))
				.ToList()))
			.ToList();
	}

	public static async Task<bool> TestConnectionAsync(string serverUrl, string apiToken)
	{
		try
		{
			using var client = CreateClient(serverUrl);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);
			client.Timeout = TimeSpan.FromSeconds(10);

			var response = await client.GetAsync("api/libraries");
			return response.IsSuccessStatusCode;
		}
		catch
		{
			return false;
		}
	}

	public static async Task<bool> UploadBookAsync(
		string serverUrl,
		string apiToken,
		string libraryId,
		string folderId,
		string title,
		string? author,
		string? series,
		IEnumerable<string> filePaths)
	{
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
		foreach (var path in filePaths.Where(File.Exists))
		{
			var fileContent = new StreamContent(File.OpenRead(path));
			fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
			form.Add(fileContent, fileIndex.ToString(), Path.GetFileName(path));
			fileIndex++;
		}

		if (fileIndex == 0)
			return false;

		var response = await client.PostAsync("api/upload", form);
		return response.IsSuccessStatusCode;
	}

	private static string JsonEscape(string s)
		=> s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
}
