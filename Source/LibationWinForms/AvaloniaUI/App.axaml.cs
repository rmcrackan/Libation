using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using LibationWinForms.AvaloniaUI.Views;

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

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainWindow = new MainWindow();
                desktop.MainWindow = mainWindow;
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