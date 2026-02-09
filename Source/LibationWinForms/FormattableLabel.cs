using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace LibationWinForms;

public class FormattableLabel : Label
{
	public string FormatText { get; set; }

	/// <summary>Text set: first non-null, non-whitespace <see cref="Text"/> set is also saved as <see cref="FormatText"/></summary>
	[AllowNull]
	public override string Text
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
	public FormattableLabel() : base() { FormatText = string.Empty; }
	#endregion

	/// <summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array. Returns <see cref="Text"/> for convenience.</summary>
	/// <param name="args">An object array that contains zero or more objects to format.</param>
	public string Format(params object[] args) => Text = string.Format(FormatText, args);
}
