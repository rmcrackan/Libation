using DataLayer;
using System.Drawing;
using System.Windows.Forms;

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
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (value is WinFormsEntryStatus status)
			{
				if (status.BookStatus is LiberatedStatus.Error || status.IsUnavailable)
					//Don't paint the button graphic
					paintParts ^= DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.SelectionBackground;

				DataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = (Color)status.BackgroundBrush;
				DataGridView.Rows[rowIndex].DefaultCellStyle.ForeColor = status.Opacity == 1 ? DataGridView.DefaultCellStyle.ForeColor : HiddenForeColor;
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

				DrawButtonImage(graphics, (Image)status.ButtonImage, cellBounds);
                AccessibilityDescription = status.ToolTip;

				if (status.IsUnavailable || status.Opacity < 1)
					graphics.FillRectangle(DISABLED_GRAY, cellBounds);
			}
		}
	}
}
