using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace FileManager;

public interface IJsonBackedDictionary
{
	JObject GetJObject();
	bool Exists(string propertyName);
	string? GetString(string propertyName, string? defaultValue = null);
	T? GetNonString<T>(string propertyName, T? defaultValue = default);
	object? GetObject(string propertyName);
	void SetString(string propertyName, string? newValue);
	void SetNonString(string propertyName, object? newValue);
	bool RemoveProperty(string propertyName);
	bool SetWithJsonPath(string jsonPath, string propertyName, string? newValue, bool suppressLogging = false);
	string? GetStringFromJsonPath(string jsonPath);

	string? GetStringFromJsonPath(string jsonPath, string propertyName)
		=> GetStringFromJsonPath($"{jsonPath}.{propertyName}");

	static T? UpCast<T>(object obj)
	{
		if (obj.GetType().IsAssignableTo(typeof(T))) return (T)obj;
		if (obj is JObject jObject) return jObject.ToObject<T>();
		if (obj is JValue jValue)
		{
			if (typeof(T).IsAssignableTo(typeof(Enum)))
			{
				return
					Enum.TryParse(typeof(T), jValue.Value<string>(), out var enumVal)
					? (T)enumVal
					: Enum.GetValues(typeof(T)).Cast<T>().First();
			}
			return jValue.Value<T>();
		}
		throw new InvalidCastException($"{obj.GetType()} is not convertible to {typeof(T)}");
	}
}
