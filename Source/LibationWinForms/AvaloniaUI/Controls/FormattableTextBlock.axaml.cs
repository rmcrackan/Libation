using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace LibationWinForms.AvaloniaUI.Controls
{
	public partial class FormattableTextBlock : TextBlock, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TextBlock);

		private string _formatText;
		public string FormatText 
		{	
			get => _formatText;
			set
			{
				_formatText = value;
				Text = value;
			}
		}

		public string Format(params object[] args)
		{
			return Text = string.Format(FormatText, args);
		}
	}
}
