using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
	public class EditTagsDataGridViewImageButtonColumn : DataGridViewButtonColumn
	{
		public EditTagsDataGridViewImageButtonColumn()
		{
			CellTemplate = new EditTagsDataGridViewImageButtonCell();
		}
	}

	internal class EditTagsDataGridViewImageButtonCell : DataGridViewImageButtonCell
	{
		private static readonly Image ButtonImage = Properties.Resources.edit_tags_25x25;
		private static readonly Color HiddenForeColor = Color.LightGray;

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			var tagsString = (string)value;

			var foreColor = tagsString?.Contains("hidden") == true ? HiddenForeColor : DataGridView.DefaultCellStyle.ForeColor;

			if (DataGridView.Rows[RowIndex].DefaultCellStyle.ForeColor != foreColor)
			{
				DataGridView.Rows[RowIndex].DefaultCellStyle.ForeColor = foreColor;
			}

			if (tagsString.Length == 0)
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);
				DrawButtonImage(graphics, ButtonImage, cellBounds);
			}
			else
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
			}
		}
	}
}
