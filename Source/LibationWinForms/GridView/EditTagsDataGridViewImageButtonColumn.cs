using System.Drawing;
using System.Windows.Forms;
using Dinah.Core.WindowsDesktop.Forms;
using LibationUiBase.GridView;

namespace LibationWinForms.GridView
{
	public interface IDataGridScaleColumn
	{
		float ScaleFactor { get; set; }
	}
    public class EditTagsDataGridViewImageButtonColumn : DataGridViewButtonColumn, IDataGridScaleColumn
	{
		public EditTagsDataGridViewImageButtonColumn()
		{
			CellTemplate = new EditTagsDataGridViewImageButtonCell();
		}

		public float ScaleFactor { get; set; }
	}

	internal class EditTagsDataGridViewImageButtonCell : DataGridViewImageButtonCell
    {
        public EditTagsDataGridViewImageButtonCell() : base("Edit Tags button") { }

        private static Image ButtonImage { get; } = Properties.Resources.edit_25x25;

		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
            // series
            if (rowIndex >= 0 && DataGridView.GetBoundItem<IGridEntry>(rowIndex) is ISeriesEntry)
			{
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, DataGridViewPaintParts.Background | DataGridViewPaintParts.Border);
            }
            // tag: empty
            else if (value is string tagStr && tagStr.Length == 0)
			{
				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

				DrawButtonImage(graphics, ButtonImage, cellBounds);
                AccessibilityDescription = "Click to edit tags";
            }
			// tag: not empty
			else
			{
                base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

                AccessibilityDescription = (string)value;
            }
        }
	}
}
