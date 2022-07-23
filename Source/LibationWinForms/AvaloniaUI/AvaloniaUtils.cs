using Avalonia.Media;
using System;
using System.Threading;
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

		public static T ShowDialogSynchronously<T>(this Avalonia.Controls.Window window, Avalonia.Controls.Window owner)
		{
			using var source = new CancellationTokenSource();
			var dialogTask = window.ShowDialog<T>(owner);
			dialogTask.ContinueWith(t => source.Cancel(), TaskScheduler.FromCurrentSynchronizationContext());
			Avalonia.Threading.Dispatcher.UIThread.MainLoop(source.Token);
			return dialogTask.Result;
		}
	}
}
