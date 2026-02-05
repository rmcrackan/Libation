using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms;

public class FormattableToolStripStatusLabel : ToolStripStatusLabel
{
	public string FormatText { get; set; }

	/// <summary>Text set: first non-null, non-whitespace <see cref="Text"/> set is also saved as <see cref="FormatText"/></summary>
	public override string? Text
	{
		get => base.Text;
		set
		{
			if (string.IsNullOrWhiteSpace(FormatText) && !string.IsNullOrWhiteSpace(value))
				FormatText = value;

			base.Text = value;
		}
	}

	#region ctor.s
	public FormattableToolStripStatusLabel() : base() { FormatText = string.Empty; }
	public FormattableToolStripStatusLabel(string text) : base(text) => FormatText = text;
	public FormattableToolStripStatusLabel(Image image) : base(image) { FormatText = string.Empty; }
	public FormattableToolStripStatusLabel(string text, Image image) : base(text, image) => FormatText = text;
	public FormattableToolStripStatusLabel(string text, Image image, EventHandler onClick) : base(text, image, onClick) => FormatText = text;
	public FormattableToolStripStatusLabel(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) => FormatText = text;
	#endregion

	/// <summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array. Returns <see cref="Text"/> for convenience.</summary>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public string Format(params object[] args) => Text = string.Format(FormatText, args);
}
