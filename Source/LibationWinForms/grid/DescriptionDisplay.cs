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
		public DescriptionDisplay()
		{
			InitializeComponent();
			textBox1.LostFocus += (_, _) => Close();
			this.Shown += DescriptionDisplay_Shown;

			var textHeight = TextRenderer.MeasureText("\n", textBox1.Font).Height;
		}

		[DllImport("user32.dll")]
		static extern bool HideCaret(IntPtr hWnd);

		private void DescriptionDisplay_Shown(object sender, EventArgs e)
		{
			textBox1.DeselectAll();
			HideCaret(textBox1.Handle);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			int lineCount = textBox1.GetLineFromCharIndex(int.MaxValue) + 2;
			Height = Height - textBox1.Height + lineCount * TextRenderer.MeasureText(textBox1.Text, textBox1.Font).Height;

		}
	}
}
