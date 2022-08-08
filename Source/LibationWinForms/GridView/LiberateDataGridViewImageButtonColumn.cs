using System;
using System.Drawing;
using System.Windows.Forms;
using DataLayer;
using Dinah.Core.WindowsDesktop.Forms;

namespace LibationWinForms.GridView
{
	public class LiberateDataGridViewImageButtonColumn : DataGridViewButtonColumn
	{
		public LiberateDataGridViewImageButtonColumn()
		{
			CellTemplate = new LiberateDataGridViewImageButtonCell();
		}
	}

	internal class LiberateDataGridViewImageButtonCell : DataGridViewImageButtonCell
	{
		private static readonly Color SERIES_BG_COLOR = Color.FromArgb(230, 255, 230);
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			if (value is LiberateButtonStatus status)
			{
				if (status.BookStatus is LiberatedStatus.Error)
					paintParts ^= DataGridViewPaintParts.ContentBackground | DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.SelectionBackground;

				if (rowIndex >= 0 && DataGridView.GetBoundItem<GridEntry>(rowIndex) is LibraryBookEntry lbEntry && lbEntry.Parent is not null)
					DataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = SERIES_BG_COLOR;

				base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

				if (status.IsSeries)
				{
					DrawButtonImage(graphics, status.Expanded ? Properties.Resources.minus : Properties.Resources.plus, cellBounds);

					ToolTipText = status.Expanded ? "Click to Collpase" : "Click to Expand";
				}
				else
				{
					(string mouseoverText, Bitmap buttonImage) = GetLiberateDisplay(status.BookStatus, status.PdfStatus);

					DrawButtonImage(graphics, buttonImage, cellBounds);

					ToolTipText = mouseoverText;
				}
			}
		}

		private static (string mouseoverText, Bitmap buttonImage) GetLiberateDisplay(LiberatedStatus liberatedStatus, LiberatedStatus? pdfStatus)
		{
			if (liberatedStatus == LiberatedStatus.Error)
				return ("Book downloaded ERROR", Properties.Resources.error);

			(string libState, string image_lib) = liberatedStatus switch
			{
				LiberatedStatus.Liberated => ("Liberated", "green"),
				LiberatedStatus.PartialDownload => ("File has been at least\r\npartially downloaded", "yellow"),
				LiberatedStatus.NotLiberated => ("Book NOT downloaded", "red"),
				_ => throw new Exception("Unexpected liberation state")
			};

			(string pdfState, string image_pdf) = pdfStatus switch
			{
				LiberatedStatus.Liberated => ("\r\nPDF downloaded", "_pdf_yes"),
				LiberatedStatus.NotLiberated => ("\r\nPDF NOT downloaded", "_pdf_no"),
				LiberatedStatus.Error => ("\r\nPDF downloaded ERROR", "_pdf_no"),
				null => ("", ""),
				_ => throw new Exception("Unexpected PDF state")
			};

			var mouseoverText = libState + pdfState;

			if (liberatedStatus == LiberatedStatus.NotLiberated ||
				liberatedStatus == LiberatedStatus.PartialDownload ||
				pdfStatus == LiberatedStatus.NotLiberated)
				mouseoverText += "\r\nClick to complete";

			var buttonImage = (Bitmap)Properties.Resources.ResourceManager.GetObject($"liberate_{image_lib}{image_pdf}");

			return (mouseoverText, buttonImage);
		}
	}
}
