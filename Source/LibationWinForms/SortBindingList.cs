using LibationWinForms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms;

/// <summary>
/// Basic implementation of a sortable binding list to allow automatic column sorting in DataGridViews
/// </summary>
internal class SortBindingList<TItem> : BindingList<TItem>
{
	private PropertyDescriptor? _propertyDescriptor;
	private ListSortDirection _listSortDirection;
	private bool _isSortedCore;

	protected override PropertyDescriptor? SortPropertyCore => _propertyDescriptor;
	protected override ListSortDirection SortDirectionCore => _listSortDirection;
	protected override bool IsSortedCore => _isSortedCore;
	protected override bool SupportsSortingCore => true;
	public SortBindingList() : base(new List<TItem>()) { }
	public SortBindingList(IEnumerable<TItem> records) : base(records.ToList()) { }
	public SortBindingList(List<TItem> records) : base(records) { }
	protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
	{
		var itemsList = (List<TItem>)Items;

		var sorted =
			direction is ListSortDirection.Ascending ? itemsList.OrderBy(i => prop.GetValue(i)).ToList()
			: itemsList.OrderByDescending(i => prop.GetValue(i)).ToList();

		itemsList.Clear();
		itemsList.AddRange(sorted);

		_propertyDescriptor = prop;
		_listSortDirection = direction;
		_isSortedCore = true;

		OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
	}
}