using FileManager;
using Newtonsoft.Json.Linq;

#nullable enable
namespace LibationFileManager;

internal class MockPersistentDictionary : IPersistentDictionary
{
	private JObject JsonObject { get; } = new();

	public bool Exists(string propertyName)
		=> JsonObject.ContainsKey(propertyName);
	public string? GetString(string propertyName, string? defaultValue = null)
		=> JsonObject[propertyName]?.Value<string>() ?? defaultValue;
	public T? GetNonString<T>(string propertyName, T? defaultValue = default)
		=> GetObject(propertyName) is object obj ? IPersistentDictionary.UpCast<T>(obj) : defaultValue;
	public object? GetObject(string propertyName)
		=> JsonObject[propertyName]?.Value<object>();
	public void SetString(string propertyName, string? newValue)
		=> JsonObject[propertyName] = newValue;
	public void SetNonString(string propertyName, object? newValue)
		=> JsonObject[propertyName] = newValue is null ? null : JToken.FromObject(newValue);
	public bool RemoveProperty(string propertyName)
		=> JsonObject.Remove(propertyName);
	public string? GetStringFromJsonPath(string jsonPath)
		=> JsonObject.SelectToken(jsonPath)?.Value<string>();
	public bool SetWithJsonPath(string jsonPath, string propertyName, string? newValue, bool suppressLogging = false)
	{
		if (JsonObject.SelectToken(jsonPath) is JToken token && token?[propertyName] is not null)
		{
			token[propertyName] = newValue;
			return true;
		}
		return false;
	}
}