using LibationUiBase.SeriesView;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LibationWinForms.SeriesView;

public class DownloadButtonColumn : DataGridViewButtonColumn
{
	public DownloadButtonColumn()
	{
		CellTemplate = new DownloadButtonColumnCell();
		CellTemplate.Style.WrapMode = DataGridViewTriState.True;
	}
}
internal class DownloadButtonColumnCell : AccessibleDataGridViewButtonCell
{
	public DownloadButtonColumnCell() : base("Download Series button") { }

	protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
	{
		if (value is not SeriesButton seriesEntry)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
			return;
		}

		string cellValue = seriesEntry.DisplayText;
		AccessibilityDescription = cellValue;

		if (!seriesEntry.Enabled)
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
		else if (seriesEntry.HasButtonAction)
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, cellValue, cellValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		else
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
			TextRenderer.DrawText(graphics, cellValue, cellStyle.Font, cellBounds, cellStyle.ForeColor);
		}
	}
}
