using System.Drawing;
using System.Windows.Forms;
using Dinah.Core.WindowsDesktop.Forms;
using LibationUiBase.GridView;

namespace LibationWinForms.GridView
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
		private static Image ButtonImage { get; } = Properties.Resources.edit_25x25;

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (rowIndex >= 0 && DataGridView.GetBoundItem<IGridEntry>(rowIndex) is ISeriesEntry)
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
			else if (value is string tagStr && tagStr.Length == 0)
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);
				DrawButtonImage(graphics, ButtonImage, cellBounds);
			}
			else
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
	}
}
