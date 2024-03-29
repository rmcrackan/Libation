﻿using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public partial class DescriptionDisplay : Form
	{
		private int borderThickness = 5;

		public int BorderThickness
		{
			get => borderThickness;
			set
			{
				borderThickness = value;
				textBox1.Location = new Point(borderThickness, borderThickness);
				textBox1.Size = new Size(Width - 2 * borderThickness, Height - 2 * borderThickness);
			}
		}
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

			Location = new Point(SpawnLocation.X, Math.Min(SpawnLocation.Y, Screen.PrimaryScreen.WorkingArea.Height - Height));
		}

		[DllImport("user32.dll")]
		static extern bool HideCaret(IntPtr hWnd);
	}
}
