using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using LibationAvalonia.Dialogs;

namespace LibationAvalonia
{
	public class AppBasic : Application
	{
		public override void Initialize()
		{
			AvaloniaXamlLoader.Load(this);
		}
		public Window MainWindow { get; set; }

		public override void OnFrameworkInitializationCompleted()
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new SetupDialog();
			}

			base.OnFrameworkInitializationCompleted();
		}

	}
}