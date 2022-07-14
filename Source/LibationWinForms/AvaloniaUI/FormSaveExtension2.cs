using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LibationFileManager;

namespace LibationWinForms.AvaloniaUI
{
	public static class FormSaveExtension2
	{
		static readonly WindowIcon WindowIcon;
		static FormSaveExtension2()
		{
			if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				WindowIcon = desktop.MainWindow.Icon;
			else
				WindowIcon = null;
		}

		public static void SetLibationIcon(this Window form)
		{
			form.Icon = WindowIcon;
		}

		public static void RestoreSizeAndLocation(this Window form, Configuration config)
		{
			FormSizeAndPosition savedState = config.GetNonString<FormSizeAndPosition>(form.Name);

			if (savedState is null)
				return;

			// too small -- something must have gone wrong. use defaults
			if (savedState.Width < form.MinWidth || savedState.Height < form.MinHeight)
			{
				savedState.Width = (int)form.Width;
				savedState.Height = (int)form.Height;
			}

			// Fit to the current screen size in case the screen resolution changed since the size was last persisted
			if (savedState.Width > form.Screens.Primary.WorkingArea.Width)
				savedState.Width = form.Screens.Primary.WorkingArea.Width;
			if (savedState.Height > form.Screens.Primary.WorkingArea.Height)
				savedState.Height = form.Screens.Primary.WorkingArea.Height;

			var rect = new PixelRect(savedState.X, savedState.Y, savedState.Width, savedState.Height);

			form.Width = savedState.Width;
			form.Height = savedState.Height;

			// is proposed rect on a screen?
			if (form.Screens.All.Any(screen => screen.WorkingArea.Contains(rect)))
			{
				form.WindowStartupLocation = WindowStartupLocation.Manual;
				form.Position = new PixelPoint(savedState.X, savedState.Y);
			}
			else
			{
				form.WindowStartupLocation = WindowStartupLocation.CenterScreen;
			}

			// FINAL: for Maximized: start normal state, set size and location, THEN set max state
			form.WindowState = savedState.IsMaximized ? WindowState.Maximized : WindowState.Normal;
		}
		public static void SaveSizeAndLocation(this Window form, Configuration config)
		{
			var saveState = new FormSizeAndPosition();

			saveState.IsMaximized = form.WindowState == WindowState.Maximized;

			// restore normal state to get real window size.
			if (form.WindowState != WindowState.Normal)
			{
				form.WindowState = WindowState.Normal;
			}

			saveState.X = form.Position.X;
			saveState.Y = form.Position.Y;

			saveState.Width = (int)form.Bounds.Size.Width;
			saveState.Height = (int)form.Bounds.Size.Height;

			config.SetObject(form.Name, saveState);
		}

		class FormSizeAndPosition
		{
			public int X;
			public int Y;
			public int Height;
			public int Width;
			public bool IsMaximized;
		}
	}
}
