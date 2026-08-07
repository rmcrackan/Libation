using System;

namespace FileManager;

/// <summary>
/// Settings.json (or other JSON-backed config) contained a value that cannot be mapped to the expected type.
/// </summary>
public sealed class InvalidConfigurationValueException : Exception
{
	public string PropertyPath { get; }
	public string? InvalidValue { get; }
	public Type? ExpectedType { get; }

	public InvalidConfigurationValueException(string propertyPath, string? invalidValue, Type? expectedType, string message)
		: base(message)
	{
		PropertyPath = propertyPath;
		InvalidValue = invalidValue;
		ExpectedType = expectedType;
	}

	public static InvalidConfigurationValueException ForEnum(string propertyPath, string? invalidValue, Type enumType)
	{
		var allowed = string.Join(", ", Enum.GetNames(enumType));
		var display = FormatValue(invalidValue);
		var message =
			$"Invalid value for '{propertyPath}': {display}. " +
			$"Expected one of: {allowed}.";
		return new InvalidConfigurationValueException(propertyPath, invalidValue, enumType, message);
	}

	public static InvalidConfigurationValueException ForPath(string propertyPath, string? invalidValue, string message)
		=> new(propertyPath, invalidValue, expectedType: null, message);

	public static string FormatValue(string? value)
		=> value is null ? "[null]"
		: value.Length == 0 ? "[empty]"
		: $"\"{value}\"";
}
