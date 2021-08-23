using ApplicationServices;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using DataLayer;

namespace LibationWinForms
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
		protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, null, null, null, cellStyle, advancedBorderStyle, paintParts);

			if (value is (LiberatedStatus, LiberatedStatus) or (LiberatedStatus, null))
			{
				var (bookState, pdfState) = ((LiberatedStatus bookState, LiberatedStatus? pdfState))value;

				(string mouseoverText, Bitmap buttonImage) = GetLiberateDisplay(bookState, pdfState);

				DrawButtonImage(graphics, buttonImage, cellBounds);

				ToolTipText = mouseoverText;
			}
		}

		private static (string mouseoverText, Bitmap buttonImage) GetLiberateDisplay(LiberatedStatus liberatedStatus, LiberatedStatus? pdfStatus)
		{
			(string libState, string image_lib) = liberatedStatus switch
			{
				LiberatedStatus.Liberated => ("Liberated", "green"),
				LiberatedStatus.PartialDownload => ("File has been at least\r\npartially downloaded", "yellow"),
				LiberatedStatus.NotLiberated => ("Book NOT downloaded", "red"),
				LiberatedStatus.Error => ("Book downloaded ERROR", "red"),
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
