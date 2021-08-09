using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
	public class EditTagsDataGridViewImageButtonColumn : DataGridViewImageButtonColumn
	{
		protected override DataGridViewImageButtonCell NewCell()
			=> new EditTagsDataGridViewImageButtonCell();
	}

	internal class EditTagsDataGridViewImageButtonCell : DataGridViewImageButtonCell
	{
		private static readonly Bitmap ButtonImage = Properties.Resources.edit_tags_25x25;
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (((string)value).Length == 0)
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);
				DrawImage(graphics, ButtonImage, cellBounds);
			}
			else
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
	}
}
