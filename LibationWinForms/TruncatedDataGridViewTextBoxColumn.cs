using System.ComponentModel;
using System.Windows.Forms;

namespace LibationWinForms
{
	public class TruncatedDataGridViewTextBoxColumn : DataGridViewTextBoxColumn
	{
		public TruncatedDataGridViewTextBoxColumn()
		{
			CellTemplate = new TruncatedDataGridViewTextBoxCell();
		}
	}

	internal class TruncatedDataGridViewTextBoxCell : DataGridViewTextBoxCell
	{
		private const int MAX_DISPLAY_CHARS = 63;
		private string truncatedString;

		protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		{
			if (value is null || value is not string valueStr)
				return value;

			truncatedString ??= valueStr.Length < MAX_DISPLAY_CHARS ? valueStr : valueStr.Substring(0, MAX_DISPLAY_CHARS - 1) + "…";

			return truncatedString;
		}
	}
}
