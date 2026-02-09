using System;

namespace LibationUiBase.SeriesView;

public class SeriesOrder : IComparable
{
	public float Order { get; }
	public string OrderString { get; }

	public SeriesOrder(string orderString)
	{
		OrderString = orderString;
		Order = float.TryParse(orderString, out var o) ? o : -1f;
	}
	public override string ToString() => OrderString;

	public int CompareTo(object? obj)
	{
		if (obj is not SeriesOrder other) return 1;
		return Order.CompareTo(other.Order);
	}
}
