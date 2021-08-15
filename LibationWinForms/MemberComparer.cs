using System.Collections.Generic;
using System.ComponentModel;

namespace LibationWinForms
{
	internal class MemberComparer<T> : IComparer<T> where T : IMemberComparable
	{
		public ListSortDirection Direction { get; set; } = ListSortDirection.Ascending;
		public string PropertyName { get; set; }

		public int Compare(T x, T y)
		{
			var val1 = x.GetMemberValue(PropertyName);
			var val2 = y.GetMemberValue(PropertyName);

			return DirMult * x.GetMemberComparer(val1.GetType()).Compare(val1, val2);
		}

		private int DirMult => Direction == ListSortDirection.Descending ? -1 : 1;
	}
}
