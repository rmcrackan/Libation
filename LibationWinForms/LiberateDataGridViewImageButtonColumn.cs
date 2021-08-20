using ApplicationServices;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

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

			if (value is (LiberatedState liberatedState, PdfState pdfState))
			{
				(string mouseoverText, Bitmap buttonImage) = GetLiberateDisplay(liberatedState, pdfState);

				DrawButtonImage(graphics, buttonImage, cellBounds);

				ToolTipText = mouseoverText;
			}
		}

		private static (string mouseoverText, Bitmap buttonImage) GetLiberateDisplay(LiberatedState liberatedStatus, PdfState pdfStatus)
		{
			(string libState, string image_lib) = liberatedStatus switch
			{
				LiberatedState.Liberated => ("Liberated", "green"),
				LiberatedState.PartialDownload => ("File has been at least\r\npartially downloaded", "yellow"),
				LiberatedState.NotDownloaded => ("Book NOT downloaded", "red"),
				_ => throw new Exception("Unexpected liberation state")
			};

			(string pdfState, string image_pdf) = pdfStatus switch
			{
				PdfState.Downloaded => ("\r\nPDF downloaded", "_pdf_yes"),
				PdfState.NotDownloaded => ("\r\nPDF NOT downloaded", "_pdf_no"),
				PdfState.NoPdf => ("", ""),
				_ => throw new Exception("Unexpected PDF state")
			};

			var mouseoverText = libState + pdfState;

			if (liberatedStatus == LiberatedState.NotDownloaded ||
				liberatedStatus == LiberatedState.PartialDownload ||
				pdfStatus == PdfState.NotDownloaded)
				mouseoverText += "\r\nClick to complete";

			var buttonImage = (Bitmap)Properties.Resources.ResourceManager.GetObject($"liberate_{image_lib}{image_pdf}");

			return (mouseoverText, buttonImage);
		}
	}
}
