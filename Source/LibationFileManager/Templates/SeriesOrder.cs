using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace LibationFileManager.Templates;

public class SeriesOrder : IFormattable
{
	/// <summary>
	/// A numeric span from the original order string. Keep the original digits for unformatted
	/// output so large values (e.g. 2147483647) are not rounded through <see cref="float"/> into
	/// scientific notation (issue #2024). Apply the numeric format only when the template asks.
	/// </summary>
	private readonly record struct NumberPart(string Raw, decimal Value) : IFormattable
	{
		public override string ToString() => Raw;

		public string ToString(string? format, IFormatProvider? formatProvider)
			=> string.IsNullOrEmpty(format)
				? Raw
				: Value.ToString(format, formatProvider ?? CultureInfo.InvariantCulture);
	}

	private object[] OrderParts { get; }
	private SeriesOrder(object[] orderParts)
	{
		OrderParts = orderParts;
	}

	public override string ToString() => ToString(null, null);

	/// <summary>
	/// Use numeric formatters to format the number parts of the order.
	/// </summary>
	public string ToString(string? format, IFormatProvider? formatProvider)
		=> string.Concat(OrderParts.Select(p => p switch
		{
			IFormattable f => f.ToString(format, formatProvider ?? CultureInfo.InvariantCulture),
			_ => p.ToString(),
		})).Trim();

	public static SeriesOrder Parse(string? order)
	{
		List<object> parts = [];
		while (TryParseNumber(order, out var number, out var range))
		{
			var prefix = order[..range.Start.Value];
			if (!string.IsNullOrEmpty(prefix))
				parts.Add(prefix);

			parts.Add(number);

			order = order[range.End.Value..];
		}

		if (!string.IsNullOrEmpty(order))
			parts.Add(order);

		return new(parts.ToArray());
	}

	/// <summary>
	/// Try to parse any positive number from within the string (greedy).
	/// </summary>
	/// <param name="numString">the string to search for a numeric value</param>
	/// <param name="number">If this function succeeds, the number that was found; otherwise default.</param>
	/// <param name="range">If this function succeeds, the range of characters representing <paramref name="number"/> in <paramref name="numString"/>; otherwise default</param>
	/// <returns>True if a number was found; otherwise false.</returns>
	private static bool TryParseNumber([NotNullWhen(true)] string? numString, out NumberPart number, out Range range)
	{
		number = default;
		if (string.IsNullOrWhiteSpace(numString))
		{
			range = default;
			return false;
		}

		for (var s = 0; s < numString.Length; s++)
		{
			//Assume any valid number will begin with a digit.
			//This way, leading dots and dashes will never be considered part of a number, so
			//no negative series numbers and no fractional series numbers < 1 (unless preceded with a '0').
			if (!char.IsDigit(numString[s]))
				continue;

			for (var e = numString.Length; e > s; e--)
			{
				//The decimal parser will succeed with trailing whitespace,
				//but we want to preserve it in the final display string.
				if (char.IsWhiteSpace(numString[e - 1]))
					continue;

				var substring = numString[s..e];
				if (decimal.TryParse(substring, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
				{
					number = new NumberPart(substring, value);
					range = new Range(s, e);
					return true;
				}
			}
		}

		range = default;
		return false;
	}
}
