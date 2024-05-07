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
        #region Accessibility
        private string accessibilityName => "Liberate Image Button";
		private string accessibilityDescription = "undefined";
        protected override AccessibleObject CreateAccessibilityInstance() => new MyAccessibilityObject(accessibilityName, accessibilityDescription);
        protected class MyAccessibilityObject : DataGridViewCellAccessibleObject
        {
            public override string Name => _name;
            public override string Description => _description;

            private string _name { get; }
            private string _description { get; }

            public MyAccessibilityObject(string name, string description) : base()
            {
                _name = name;
                _description = description;
            }
        }
        #endregion

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
                accessibilityDescription = status.ToolTip;
                ToolTipText = status.ToolTip;

				if (status.IsUnavailable || status.Opacity < 1)
					graphics.FillRectangle(DISABLED_GRAY, cellBounds);
			}
		}
	}
}
