using Avalonia.Controls;
using Avalonia.Styling;
using System;

namespace LibationWinForms.AvaloniaUI.Controls
{
	public partial class FormattableMenuItem : MenuItem, IStyleable
	{
		Type IStyleable.StyleKey => typeof(MenuItem);

		private string _formatText;
		public string FormatText
		{
			get => _formatText;
			set
			{
				_formatText = value;
				Header = value;
			}
		}

		public string Format(params object[] args)
		{
			var formatText = string.Format(FormatText, args);
			Header = formatText;
			return formatText;
		}
	}
}
