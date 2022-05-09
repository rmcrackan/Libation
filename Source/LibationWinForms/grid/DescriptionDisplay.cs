using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms
{
	public partial class DescriptionDisplay : Form
	{
		public string DescriptionText { get => textBox1.Text; set => textBox1.Text = value; }
		public Point SpawnLocation { get; set; }
		public DescriptionDisplay()
		{
			InitializeComponent();
			textBox1.LostFocus += (_, _) => Close();
			Shown += DescriptionDisplay_Shown;
		}

		private void DescriptionDisplay_Shown(object sender, EventArgs e)
		{
			textBox1.DeselectAll();
			HideCaret(textBox1.Handle);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			int lineCount = textBox1.GetLineFromCharIndex(int.MaxValue) + 2;
			Height = Height - textBox1.Height + lineCount * TextRenderer.MeasureText("X", textBox1.Font).Height;

			int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;


			var tboxLocation = PointToScreen(textBox1.Location);
			var tboxOffset = new Size(tboxLocation.X - Location.X, tboxLocation.Y - Location.Y);

			Location = new Point(SpawnLocation.X - tboxOffset.Width, Math.Min(SpawnLocation.Y - tboxOffset.Height, screenHeight - Height));
		}

		[DllImport("user32.dll")]
		static extern bool HideCaret(IntPtr hWnd);
	}
}
