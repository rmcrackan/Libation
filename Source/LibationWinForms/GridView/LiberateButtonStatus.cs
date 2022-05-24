using DataLayer;
using System;

namespace LibationWinForms.GridView
{
	public class LiberateButtonStatus : IComparable
	{
		public LiberatedStatus BookStatus { get; set; }
		public LiberatedStatus? PdfStatus { get; set; }
		public bool IsSeries { get; init; }
		public bool Expanded { get; set; }

		/// <summary>
		/// Defines the Liberate column's sorting behavior
		/// </summary>
		public int CompareTo(object obj)
		{
			if (obj is not LiberateButtonStatus second) return -1;

			if (IsSeries && !second.IsSeries) return -1;
			else if (!IsSeries && second.IsSeries) return 1;
			else if (IsSeries && second.IsSeries) return 0;
			else if (BookStatus == LiberatedStatus.Liberated && second.BookStatus != LiberatedStatus.Liberated) return -1;
			else if (BookStatus != LiberatedStatus.Liberated && second.BookStatus == LiberatedStatus.Liberated) return 1;
			else return BookStatus.CompareTo(second.BookStatus);
		}
	}
}
