using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

#nullable enable
namespace LibationFileManager.Templates;

public class SeriesOrder : IFormattable
{
	public object[] OrderParts { get; }
	private SeriesOrder(object[] orderParts)
	{
		OrderParts = orderParts;
	}

	public override string ToString() => ToString(null, null);

	/// <summary>
	/// Use float formatters to format the number parts of the order.
	/// </summary>
	public string ToString(string? format, IFormatProvider? formatProvider)
		=> string.Concat(OrderParts.Select(p => p is float f ? f.ToString(format) : p.ToString())).Trim();

	public static SeriesOrder Parse(string? order)
	{
		List<object> parts = new();
		while (TryParseNumber(order, out var value, out var range))
		{
			var prefix = order[..range.Start.Value];
			if(!string.IsNullOrWhiteSpace(prefix))
				parts.Add(prefix);

			parts.Add(value);

			order = order[range.End.Value..];
		}

		if (!string.IsNullOrWhiteSpace(order))
			parts.Add(order);

		return new(parts.ToArray());
	}

	/// <summary>
	/// Try to parse any positive number from within the string (greedy).
	/// </summary>
	/// <param name="numString">the string to search for a numeric value</param>
	/// <param name="value">If this function succeeds, the number that was found; otherwise zero.</param>
	/// <param name="range">If this function succeeds, the range of characters representing <paramref name="value"/> in <paramref name="numString"/>; otherwise default</param>
	/// <returns>True if a number was found; otherwise false.</returns>
	private static bool TryParseNumber([NotNullWhen(true)] string? numString, out float value, out Range range)
	{
		value = 0;
		if (string.IsNullOrWhiteSpace(numString))
		{
			range = default;
			return false;
		}

		for (int s = 0; s < numString.Length; s++)
		{
			//Assume any valid number will begin with a digit.
			//This way, leading dots and dashes will never be considered part of a number, so
			//no negative series numbers and no fractional series numbers < 1 (unless preceded with a '0').
			if (!char.IsDigit(numString[s]))
				continue;

			for (int e = numString.Length; e > s; e--)
			{
				//The float parser will succeed with trailing whitespace,
				//but we want to preserve it in the final display string.
				if (char.IsWhiteSpace(numString[e - 1]))
					continue;

				var substring = numString[s..e];
				if (float.TryParse(substring, System.Globalization.CultureInfo.InvariantCulture, out value))
				{
					range = new Range(s, e);
					return true;
				}
			}
		}

		range = default;
		return false;
	}
}
