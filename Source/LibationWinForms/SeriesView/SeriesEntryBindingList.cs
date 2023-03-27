using LibationUiBase.SeriesView;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms.SeriesView
{
	internal class SeriesEntryBindingList : BindingList<SeriesItem>
	{
		private PropertyDescriptor _propertyDescriptor;

		private ListSortDirection _listSortDirection;

		private bool _isSortedCore;

		protected override PropertyDescriptor SortPropertyCore => _propertyDescriptor;

		protected override ListSortDirection SortDirectionCore => _listSortDirection;

		protected override bool IsSortedCore => _isSortedCore;

		protected override bool SupportsSortingCore => true;

		public SeriesEntryBindingList() : base(new List<SeriesItem>()) { }
		public SeriesEntryBindingList(IEnumerable<SeriesItem> entries) : base(entries.ToList()) { }

		protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
		{
			var itemsList = (List<SeriesItem>)base.Items;

			var sorted
				= (direction == ListSortDirection.Ascending)
				? itemsList.OrderBy(prop.GetValue).ToList()
				: itemsList.OrderByDescending(prop.GetValue).ToList();

			itemsList.Clear();
			itemsList.AddRange(sorted);

			_propertyDescriptor = prop;
			_listSortDirection = direction;
			_isSortedCore = true;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}
}
