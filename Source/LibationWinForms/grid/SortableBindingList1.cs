using Dinah.Core.DataBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace LibationWinForms
{
	internal class SortableBindingList1<T> : BindingList<T> where T : class, IMemberComparable, IHierarchical<T>
	{
		private bool isSorted;
		private ListSortDirection listSortDirection;
		private PropertyDescriptor propertyDescriptor;

		public SortableBindingList1() : base(new List<T>()) { }
		public SortableBindingList1(IEnumerable<T> enumeration) : base(new List<T>(enumeration)) { }

		protected bool SuspendSorting { get; set; }
		protected MemberComparer<T> Comparer { get; } = new();
		protected override bool SupportsSortingCore => true;
		protected override bool SupportsSearchingCore => true;
		protected override bool IsSortedCore => isSorted;
		protected override PropertyDescriptor SortPropertyCore => propertyDescriptor;
		protected override ListSortDirection SortDirectionCore => listSortDirection;

		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			Comparer.PropertyName = property.Name;
			Comparer.Direction = direction;

			Sort();

			propertyDescriptor = property;
			listSortDirection = direction;
			isSorted = true;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		protected void Sort()
		{
			List<T> itemsList = (List<T>)Items;

			var sortedItems = Items.OrderBy(ge => ge, Comparer).ToList();

			var children = sortedItems.Where(i => i.Parent is not null).ToList();

			itemsList.Clear();

			//Only add parentless items at this stage. After these items are added in the
			//correct sorting order, go back and add the children beneath their parents.
			itemsList.AddRange(sortedItems.Except(children));

			foreach (var parent in children.Select(c => c.Parent).Distinct())
			{
				var pIndex = itemsList.IndexOf(parent);
				foreach (var c in children.Where(c=> c.Parent == parent))
					itemsList.Insert(++pIndex, c);
			}
		}

		protected override void OnListChanged(ListChangedEventArgs e)
		{
			if (isSorted && !SuspendSorting &&
				((e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor == SortPropertyCore) || 
				e.ListChangedType == ListChangedType.ItemAdded))
			{
				var item = Items[e.NewIndex];
				Sort();
				var newIndex = Items.IndexOf(item);

				base.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemMoved, newIndex, e.NewIndex));
			}
			else
				base.OnListChanged(e);
		}

		protected override void RemoveSortCore()
		{
			isSorted = false;
			propertyDescriptor = base.SortPropertyCore;
			listSortDirection = base.SortDirectionCore;

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}
}
