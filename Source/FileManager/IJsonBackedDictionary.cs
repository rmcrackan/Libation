using Newtonsoft.Json.Linq;
using System;

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

	static T? UpCast<T>(object obj, string? propertyName = null)
	{
		if (obj.GetType().IsAssignableTo(typeof(T))) return (T)obj;
		if (obj is JObject jObject) return jObject.ToObject<T>();
		if (obj is JValue jValue)
		{
			if (typeof(T).IsAssignableTo(typeof(Enum)))
				return ParseEnum<T>(jValue, propertyName ?? typeof(T).Name);

			return jValue.Value<T>();
		}
		throw new InvalidCastException($"{obj.GetType()} is not convertible to {typeof(T)}");
	}

	private static T ParseEnum<T>(JValue jValue, string propertyPath)
	{
		var enumType = typeof(T);

		if (TryGetEnumFromNumber(jValue, enumType, propertyPath, out var fromNumber))
			return (T)fromNumber!;

		var text = jValue.Type == JTokenType.String
			? jValue.Value<string>()
			: jValue.Value?.ToString();

		if (text is not null
			&& Enum.TryParse(enumType, text, ignoreCase: true, out var parsed)
			&& parsed is not null
			&& Enum.IsDefined(enumType, parsed))
		{
			return (T)parsed;
		}

		throw InvalidConfigurationValueException.ForEnum(propertyPath, text ?? jValue.ToString(), enumType);
	}

	private static bool TryGetEnumFromNumber(JValue jValue, Type enumType, string propertyPath, out object? value)
	{
		value = null;
		if (jValue.Type is not (JTokenType.Integer or JTokenType.Float))
			return false;

		object? raw = jValue.Value;
		if (raw is null)
			return false;

		object converted;
		try
		{
			converted = Convert.ChangeType(raw, Enum.GetUnderlyingType(enumType));
		}
		catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
		{
			throw InvalidConfigurationValueException.ForEnum(propertyPath, raw.ToString(), enumType);
		}

		if (!Enum.IsDefined(enumType, converted))
			throw InvalidConfigurationValueException.ForEnum(propertyPath, raw.ToString(), enumType);

		value = Enum.ToObject(enumType, converted);
		return true;
	}
}
