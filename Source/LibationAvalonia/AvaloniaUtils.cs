using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LibationAvalonia
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

		public static Window GetParentWindow(this IControl control)
		{
            Window window = null;

            var p = control.Parent;
            while (p != null)
            {
                if (p is Window)
                {
                    window = (Window)p;
                    break;
                }
                p = p.Parent;
            }

			return window;
        }
	}
}
