using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
	public class BitrateDataGridTextBoxColumn : AccessibleDataGridViewColumn
	{
		public BitrateDataGridTextBoxColumn() : base(new BitrateDataGridViewTextBoxCell()) { }
		private class BitrateDataGridViewTextBoxCell : AccessibleDataGridViewTextBoxCell
		{
			protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
			{
				if (value is int bitrate)
				{
					formattedValue = bitrate > 0 ? $"{bitrate} kbps" : "";
				}
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
			}
		}
	}
}
