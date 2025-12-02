using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
	public partial class ClearableTextBox : UserControl
	{
		public event EventHandler TextCleared;
		public override string Text { get => textBox1.Text; set => textBox1.Text = value; }
		public override Font Font
		{
			get => textBox1.Font;
			set
			{
				base.Font = textBox1.Font = button1.Font = value;
				OnSizeChanged(EventArgs.Empty);
			}
		}

		public int SelectionStart
		{
			get => textBox1.SelectionStart;
			set => textBox1.SelectionStart = value;
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			textBox1.Focus();
		}

		public ClearableTextBox()
		{
			InitializeComponent();
			textBox1.KeyDown += (_, e) => OnKeyDown(e);
			textBox1.KeyUp += (_, e) => OnKeyUp(e);
			textBox1.KeyPress += (_, e) => OnKeyPress(e);
			textBox1.TextChanged += (_, e) => OnTextChanged(e);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			Height = button1.Width = button1.Height = textBox1.Height;
			textBox1.Width = Width - button1.Width;
			button1.Location = new Point(textBox1.Width, 0);
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			textBox1.Clear();
			TextCleared?.Invoke(this, EventArgs.Empty);
		}
	}
}
