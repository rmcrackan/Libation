using Dinah.Core;
using System;

namespace LibationUiBase;

public class EnumDisplay<T> where T : Enum
{
	public T Value { get; }
	public string Description { get; }
	public EnumDisplay(T value, string? description = null)
	{
		Value = value;
		Description = description ?? value.GetDescription() ?? value.ToString();
	}
	public override string ToString() => Description;

	public override bool Equals(object? obj)
		=> (obj is EnumDisplay<T> other && other.Value.Equals(Value)) || (obj is T value && value.Equals(Value));
	public override int GetHashCode() => Value.GetHashCode();
}
