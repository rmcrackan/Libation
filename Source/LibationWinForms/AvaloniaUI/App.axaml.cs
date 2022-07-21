using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using LibationFileManager;
using LibationWinForms.AvaloniaUI.Views;
using System;

namespace LibationWinForms.AvaloniaUI
{
	public class App : Application
	{
		public static IBrush ProcessQueueBookFailedBrush { get; private set; }
		public static IBrush ProcessQueueBookCompletedBrush { get; private set; }
		public static IBrush ProcessQueueBookCancelledBrush { get; private set; }
		public static IBrush ProcessQueueBookDefaultBrush { get; private set; }
		public static IBrush SeriesEntryGridBackgroundBrush { get; private set; }

		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			LoadStyles();

			var SEGOEUI = new Typeface(new FontFamily(new Uri("avares://Libation/AvaloniaUI/Assets/WINGDING.TTF"), "SEGOEUI_Local"));
			var gtf = FontManager.Current.GetOrAddGlyphTypeface(SEGOEUI);


			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				var mainWindow = new MainWindow();
				desktop.MainWindow = mainWindow;
				mainWindow.RestoreSizeAndLocation(Configuration.Instance);
				mainWindow.OnLoad();
			}

			base.OnFrameworkInitializationCompleted();
		}

		private void LoadStyles()
		{
			ProcessQueueBookFailedBrush = AvaloniaUtils.GetBrushFromResources("ProcessQueueBookFailedBrush");
			ProcessQueueBookCompletedBrush = AvaloniaUtils.GetBrushFromResources("ProcessQueueBookCompletedBrush");
			ProcessQueueBookCancelledBrush = AvaloniaUtils.GetBrushFromResources("ProcessQueueBookCancelledBrush");
			ProcessQueueBookDefaultBrush = AvaloniaUtils.GetBrushFromResources("ProcessQueueBookDefaultBrush");
			SeriesEntryGridBackgroundBrush = AvaloniaUtils.GetBrushFromResources("SeriesEntryGridBackgroundBrush");
		}
	}
}