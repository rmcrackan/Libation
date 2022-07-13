using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI
{
	internal static class AvaloniaUtils
	{

		public static IBrush GetBrushFromResources(string name)
			=> GetBrushFromResources(name, Brushes.Transparent);
		public static IBrush GetBrushFromResources(string name, IBrush defaultBrush)
		{
			if (App.Current.Styles.TryGetResource(name, out var value) && value is IBrush brush)
				return brush;
			return defaultBrush;
		}
	}
}
