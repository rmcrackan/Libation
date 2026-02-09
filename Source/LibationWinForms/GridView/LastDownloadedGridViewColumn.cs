using LibationUiBase.GridView;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.GridView;

public class LastDownloadedGridViewColumn : DataGridViewColumn
{
	public LastDownloadedGridViewColumn() : base(new LastDownloadedGridViewCell()) { }
	public override DataGridViewCell? CellTemplate
	{
		get => base.CellTemplate;
		set
		{
			if (value is not LastDownloadedGridViewCell)
				throw new InvalidCastException($"Must be a {nameof(LastDownloadedGridViewCell)}");

			base.CellTemplate = value;
		}
	}
}

internal class LastDownloadedGridViewCell : AccessibleDataGridViewTextBoxCell
{
	private LastDownloadStatus? LastDownload => Value as LastDownloadStatus;

	public LastDownloadedGridViewCell() : base("Last Downloaded") { }

	protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
	{
		base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

		if (value is LastDownloadStatus lastDl)
			AccessibilityDescription = lastDl.ToolTipText;
	}

	protected override void OnDoubleClick(DataGridViewCellEventArgs e)
	{
		LastDownload?.OpenReleaseUrl();
		base.OnDoubleClick(e);
	}
}
