using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public partial class ImageDisplay : Form
	{
		public string PictureFileName { get; set; }
		public string BookSaveDirectory { get; set; }

		public ImageDisplay()
		{
			InitializeComponent();
			lastWidth = Width;
			lastHeight = Height;
		}

		public void SetCoverArt(byte[] cover)
		{
			try
			{
				pictureBox1.Image = Dinah.Core.WindowsDesktop.Drawing.ImageReader.ToImage(cover);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error loading cover art for {file}", PictureFileName);
				pictureBox1.Image = Properties.Resources.default_cover_500x500;
			}
		}

		#region Make the form's aspect ratio always match the picture's aspect ratio.

		private bool detectedResizeDirection = false;
		private bool resizingWidth = false;
		private bool resizingHeight = false;

		private int lastWidth;
		private int lastHeight;
		private int formExtraWidth;
		private int formExtraHeight;

		private double pictureAR = 1;
		protected override void OnResizeBegin(EventArgs e)
		{
			detectedResizeDirection = false;
			base.OnResizeBegin(e);
		}

		protected override void OnResizeEnd(EventArgs e)
		{
			base.OnResize(e);
			base.OnResizeEnd(e);
		}

		protected override void OnResize(EventArgs e)
		{
			if (WindowState != FormWindowState.Normal)
			{
				base.OnResize(e);
				return;
			}

			int width = this.Width, height = this.Height;

			if (!detectedResizeDirection)
			{
				resizingWidth = lastWidth != width;
				resizingHeight = lastHeight != height;
				detectedResizeDirection = true;
			}

			if (resizingWidth && !resizingHeight)
				height = CalculateARHeight(width);
			else
				width = CalculateARWidth(height);

			pictureBox1.Size = new Size(width - formExtraWidth, height - formExtraHeight);

			lastWidth = width;
			lastHeight = height;

			SetBoundsCore(Location.X, Location.Y, width, height, BoundsSpecified.Width | BoundsSpecified.Height);
		}

		private int CalculateARHeight(int width)
		{
			return (int)((width - formExtraWidth) * pictureAR) + formExtraHeight;
		}

		private int CalculateARWidth(int height)
		{
			return (int)((height - formExtraHeight) * pictureAR) + formExtraWidth;
		}

		#endregion

		private void ImageDisplay_Shown(object sender, EventArgs e)
		{
			formExtraWidth = Width - pictureBox1.Width;
			formExtraHeight = Height - pictureBox1.Height;
			OnResize(e);
		}

		private void savePictureToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog saveFileDialog = new();
			saveFileDialog.Filter = "jpeg|*.jpg";
			saveFileDialog.InitialDirectory = Directory.Exists(BookSaveDirectory) ? BookSaveDirectory : Path.GetDirectoryName(BookSaveDirectory);
			saveFileDialog.FileName = PictureFileName;

			if (saveFileDialog.ShowDialog() != DialogResult.OK)
				return;

			try
			{
				pictureBox1.Image.Save(saveFileDialog.FileName);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, $"Failed to save picture to {saveFileDialog.FileName}");
				MessageBox.Show(this, $"An error was encountered while trying to save the picture\r\n\r\n{ex.Message}", "Failed to save picture", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
			}
		}
	}
}
