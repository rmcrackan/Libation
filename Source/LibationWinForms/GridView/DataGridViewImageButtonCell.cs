using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public class DataGridViewImageButtonCell : DataGridViewButtonCell
	{
		protected void DrawButtonImage(Graphics graphics, Image image, Rectangle cellBounds)
		{
			var scaleFactor = OwningColumn is IDataGridScaleColumn scCol ? scCol.ScaleFactor : 1f;

			var w = (int)float.Round(graphics.ScaleX(image.Width) * scaleFactor);
			var h = (int)float.Round(graphics.ScaleY(image.Height) * scaleFactor);
			var x = cellBounds.Left + (cellBounds.Width - w) / 2;
			var y = cellBounds.Top + (cellBounds.Height - h) / 2;

			graphics.DrawImage(image, new Rectangle(x, y, w, h));
		}
	}
}
