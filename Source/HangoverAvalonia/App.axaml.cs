using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HangoverAvalonia.ViewModels;
using HangoverAvalonia.Views;

namespace HangoverAvalonia
{
	public partial class App : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				var mainWindow = new MainWindow
				{
					DataContext = new MainVM(),
				};
				desktop.MainWindow = mainWindow;
				mainWindow.OnLoad();
			}

			base.OnFrameworkInitializationCompleted();
		}
	}
}
