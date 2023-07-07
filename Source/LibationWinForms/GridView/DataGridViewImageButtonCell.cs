using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public class DataGridViewImageButtonCell : DataGridViewButtonCell
	{
		protected void DrawButtonImage(Graphics graphics, Image image, Rectangle cellBounds)
		{
			var w = graphics.ScaleX(image.Width);
			var h = graphics.ScaleY(image.Height);
			var x = cellBounds.Left + (cellBounds.Width - w) / 2;
			var y = cellBounds.Top + (cellBounds.Height - h) / 2;

			graphics.DrawImage(image, new Rectangle(x, y, w, h));
		}
	}
}
