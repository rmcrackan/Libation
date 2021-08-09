using ApplicationServices;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
	public class LiberateDataGridViewImageButtonColumn : DataGridViewImageButtonColumn
	{
		protected override DataGridViewImageButtonCell NewCell()
			=> new LiberateDataGridViewImageButtonCell();
	}

	internal class LiberateDataGridViewImageButtonCell : DataGridViewImageButtonCell
	{
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

			if (value is (LiberatedState liberatedState, PdfState pdfState))
			{
				(string mouseoverText, Bitmap buttonImage) = GridEntry.GetLiberateDisplay(liberatedState, pdfState);

				DrawImage(graphics, buttonImage, cellBounds);

				ToolTipText = mouseoverText;
			}
		}
	}
}
