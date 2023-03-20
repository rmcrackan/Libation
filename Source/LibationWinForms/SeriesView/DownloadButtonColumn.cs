using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using LibationUiBase.SeriesView;

namespace LibationWinForms.SeriesView
{
	public class DownloadButtonColumn : DataGridViewButtonColumn
	{
		public DownloadButtonColumn()
		{
			CellTemplate = new DownloadButtonColumnCell();
			CellTemplate.Style.WrapMode = DataGridViewTriState.True;
		}
	}
	internal class DownloadButtonColumnCell : DataGridViewButtonCell
	{
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (value is SeriesButton sentry)
			{
				string cellValue = sentry.DisplayText;
				if (!sentry.Enabled)
				{
					//Draw disabled button
					Rectangle buttonArea = cellBounds;
					Rectangle buttonAdjustment = BorderWidths(advancedBorderStyle);
					buttonArea.X += buttonAdjustment.X;
					buttonArea.Y += buttonAdjustment.Y;
					buttonArea.Height -= buttonAdjustment.Height;
					buttonArea.Width -= buttonAdjustment.Width;
					ButtonRenderer.DrawButton(graphics, buttonArea, cellValue, cellStyle.Font, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak, focused: false, PushButtonState.Disabled);

				}
				else if (sentry.HasButtonAction)
					base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, cellValue, cellValue, errorText, cellStyle, advancedBorderStyle, paintParts);
				else
				{
					base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
					TextRenderer.DrawText(graphics, cellValue, cellStyle.Font, cellBounds, cellStyle.ForeColor);
				}
			}
			else
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
			}
		}
	}
}
