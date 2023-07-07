using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	internal class CoverGridViewColumn : DataGridViewImageColumn
	{
		public CoverGridViewColumn()
		{
			CellTemplate = new CoverGridViewCell();
		}
	}

	public class CoverGridViewCell : DataGridViewImageCell
	{
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

			if (value is Image image)
			{
				var w = graphics.ScaleX(image.Width);
				var h = graphics.ScaleY(image.Height);
				var x = cellBounds.Left + (cellBounds.Width - w) / 2;
				var y = cellBounds.Top + (cellBounds.Height - h) / 2;

				graphics.DrawImage(image, new Rectangle(x, y, w, h));
			}
		}
	}
}
