using DataLayer;
using LibationUiBase.GridView;
using System.Drawing;
using System.Windows.Forms;

#nullable enable
namespace LibationWinForms.GridView
{
	public class LiberateDataGridViewImageButtonColumn : DataGridViewButtonColumn, IDataGridScaleColumn
	{
		public LiberateDataGridViewImageButtonColumn()
		{
			CellTemplate = new LiberateDataGridViewImageButtonCell();
		}

		public float ScaleFactor { get; set; }
	}

	internal class LiberateDataGridViewImageButtonCell : DataGridViewImageButtonCell
	{
		public LiberateDataGridViewImageButtonCell() : base("Liberate button") { }

        private static readonly Brush DISABLED_GRAY = new SolidBrush(Color.FromArgb(0x60, Color.LightGray));
		private static readonly Color HiddenForeColor = Color.LightGray;
		private static readonly Color SERIES_BG_COLOR = Color.FromArgb(230, 255, 230);

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (OwningRow is DataGridViewRow row && row.DataGridView is DataGridView grid && value is EntryStatus status)
			{
				if (status.BookStatus is LiberatedStatus.Error || status.IsUnavailable)
					//Don't paint the button graphic
					paintParts ^= DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.SelectionBackground;

				row.DefaultCellStyle.BackColor = status.IsEpisode ? SERIES_BG_COLOR : grid.DefaultCellStyle.BackColor;
				row.DefaultCellStyle.ForeColor = status.Opacity == 1 ? grid.DefaultCellStyle.ForeColor : HiddenForeColor;
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

				DrawButtonImage(graphics, (Image)status.ButtonImage, cellBounds);
                AccessibilityDescription = status.ToolTip;

				if (status.IsUnavailable || status.Opacity < 1)
					graphics.FillRectangle(DISABLED_GRAY, cellBounds);
			}
		}
	}
}
