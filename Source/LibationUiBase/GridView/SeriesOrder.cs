using DataLayer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationUiBase.GridView;

public class SeriesOrder : IComparable
{
	private float[] Orders { get; }
	public string OrderString { get; }

	public SeriesOrder(IEnumerable<SeriesBook> seriesBooks)
	{
		var orderstrings = seriesBooks
			.Where(s => s.Index > 0)
			.Select(s => s.Order == "-1" ? "-" : $"#{s.Order}")
			.ToList();
		OrderString = string.Join(", ", orderstrings);

		Orders = seriesBooks.Where(s => s.Index > 0).Select(s => s.Index).ToArray();
	}
	public override string ToString() => OrderString;

	public int CompareTo(object? obj)
	{
		if (obj is not SeriesOrder other) return 1;

		int count = int.Min(Orders.Length, other.Orders.Length);
		for (int i = 0; i < count; i++)
		{
			var compare = Orders[i].CompareTo(other.Orders[i]);
			if (compare != 0) return compare;
		}

		if (Orders.Length < other.Orders.Length) return 1;
		if (Orders.Length > other.Orders.Length) return -1;
		return 0;
	}

	public static int Compare(SeriesOrder? a, SeriesOrder? b)
	{
		if (a is null && b is null) return 0;
		else if (a is null) return 1;
		else if (b is null) return -1;
		else return a.CompareTo(b);
	}
}