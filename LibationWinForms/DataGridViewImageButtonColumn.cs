using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
	public abstract class DataGridViewImageButtonColumn : DataGridViewButtonColumn
	{
		private DataGridViewImageButtonCell _cellTemplate;
		public override DataGridViewCell CellTemplate
		{
			get => GetCellTemplate();
			set
			{
				if (value is DataGridViewImageButtonCell cellTemplate)
					_cellTemplate = cellTemplate;
			}
		}

		protected abstract DataGridViewImageButtonCell NewCell();

		private DataGridViewImageButtonCell GetCellTemplate()
		{
			if (_cellTemplate is null)
				return NewCell();
			else
				return _cellTemplate;
		}

		public override object Clone()
		{
			var clone = (DataGridViewImageButtonColumn)base.Clone();
			clone._cellTemplate = _cellTemplate;

			return clone;
		}
	}

	public class DataGridViewImageButtonCell : DataGridViewButtonCell
	{
		protected void DrawButtonImage(Graphics graphics, Image image, Rectangle cellBounds)
		{
			var w = image.Width;
			var h = image.Height;
			var x = cellBounds.Left + (cellBounds.Width - w) / 2;
			var y = cellBounds.Top + (cellBounds.Height - h) / 2;

			graphics.DrawImage(image, new Rectangle(x, y, w, h));
		}
	}
}
