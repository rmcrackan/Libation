using System.Drawing;

namespace LibationWinForms.GridView;

public class DataGridViewImageButtonCell : AccessibleDataGridViewButtonCell
{
	public DataGridViewImageButtonCell(string accessibilityName) : base(accessibilityName) { }

	/// <param name="imageScale">
	/// How many pixels of <paramref name="image"/> make up one of the logical pixels it should be
	/// drawn at. Images rendered above their logical size stay sharp on high-DPI displays.
	/// </param>
	protected void DrawButtonImage(Graphics graphics, Image image, Rectangle cellBounds, float imageScale = 1)
	{
		var scaleFactor = OwningColumn is IDataGridScaleColumn scCol ? scCol.ScaleFactor : 1f;

		var w = (int)float.Round(graphics.ScaleX(image.Width / imageScale) * scaleFactor);
		var h = (int)float.Round(graphics.ScaleY(image.Height / imageScale) * scaleFactor);
		var x = cellBounds.Left + (cellBounds.Width - w) / 2;
		var y = cellBounds.Top + (cellBounds.Height - h) / 2;

		graphics.DrawImage(image, new Rectangle(x, y, w, h));
	}
}
