using DataLayer;
using System;

namespace LibationWinForms.GridView
{
	public class LiberateButtonStatus : IComparable
	{
		public LiberatedStatus BookStatus { get; set; }
		public LiberatedStatus? PdfStatus { get; set; }
		public bool Expanded { get; set; }
		public bool IsSeries { get; }
		private bool IsAbsent { get; }
		public bool IsUnavailable => !IsSeries & IsAbsent & (BookStatus is not LiberatedStatus.Liberated || PdfStatus is not null and not LiberatedStatus.Liberated);

		public LiberateButtonStatus(bool isSeries, bool isAbsent)
		{
			IsSeries = isSeries;
			IsAbsent = isAbsent;
		}

		/// <summary>
		/// Defines the Liberate column's sorting behavior
		/// </summary>
		public int CompareTo(object obj)
		{
			if (obj is not LiberateButtonStatus second) return -1;

			if (IsSeries && !second.IsSeries) return -1;
			else if (!IsSeries && second.IsSeries) return 1;
			else if (IsSeries && second.IsSeries) return 0;
			else if (IsUnavailable && !second.IsUnavailable) return 1;
			else if (!IsUnavailable && second.IsUnavailable) return -1;
			else if (BookStatus == LiberatedStatus.Liberated && second.BookStatus != LiberatedStatus.Liberated) return -1;
			else if (BookStatus != LiberatedStatus.Liberated && second.BookStatus == LiberatedStatus.Liberated) return 1;
			else return BookStatus.CompareTo(second.BookStatus);
		}
	}
}
