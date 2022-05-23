using Dinah.Core.DataBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms
{
	internal class SortableBindingList1<T> : BindingList<T> where T : class, IMemberComparable, IHierarchical<T>
	{
		private bool isSorted;
		private ListSortDirection listSortDirection;
		private PropertyDescriptor propertyDescriptor;

		public SortableBindingList1() : base(new List<T>()) { }
		public SortableBindingList1(IEnumerable<T> enumeration) : base(new List<T>(enumeration)) { }

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

			//Array.Sort() and List<T>.Sort() are unstable sorts. OrderBy is stable.
			var sortedItems = itemsList.OrderBy((ge) => ge, Comparer).ToList();

			var children = sortedItems.Where(i => i.Parent is not null).ToList();
			var parents = sortedItems.Where(i => i.Children is not null).ToList();

			//Top Level items
			var topLevelItems = sortedItems.Except(children);

			itemsList.Clear();
			itemsList.AddRange(topLevelItems);

			foreach (var p in parents)
			{
				var pIndex = itemsList.IndexOf(p);
				foreach (var c in children.Where(c=> c.Parent == p))
					itemsList.Insert(++pIndex, c);
			}
		}

		protected override void OnListChanged(ListChangedEventArgs e)
		{
			if (isSorted &&
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

		protected override int FindCore(PropertyDescriptor property, object key)
		{
			int count = Count;

			System.Collections.IComparer valueComparer = null;

			for (int i = 0; i < count; ++i)
			{
				var element = this[i];
				var elemValue = element.GetMemberValue(property.Name);
				valueComparer ??= element.GetMemberComparer(elemValue.GetType());

				if (valueComparer.Compare(elemValue, key) == 0)
				{
					return i;
				}
			}

			return -1;
		}
	}
}
