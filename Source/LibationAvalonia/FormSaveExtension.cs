using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using LibationFileManager;
using System;
using System.Linq;

#nullable enable
namespace LibationAvalonia
{
	public static class FormSaveExtension
	{
		static readonly WindowIcon? WindowIcon;
		static FormSaveExtension()
		{
			WindowIcon = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow?.Icon is WindowIcon icon
				? icon
				: null;
		}

		public static void SetLibationIcon(this Window form)
		{
			form.Icon = WindowIcon;
		}

		public static void RestoreSizeAndLocation(this Window form, Configuration config)
		{
			if (Design.IsDesignMode) return;
			try
			{
				var savedState = config.GetNonString<FormSizeAndPosition?>(defaultValue: null, form.GetType().Name);

				if (savedState is null)
					return;

				// too small -- something must have gone wrong. use defaults
				if (savedState.Width < form.MinWidth || savedState.Height < form.MinHeight)
				{
					savedState.Width = (int)form.Width;
					savedState.Height = (int)form.Height;
				}
				if (form.Screens.Primary is Screen primaryScreen)
				{
					// Fit to the current screen size in case the screen resolution changed since the size was last persisted
					if (savedState.Width > primaryScreen.WorkingArea.Width)
						savedState.Width = primaryScreen.WorkingArea.Width;
					if (savedState.Height > primaryScreen.WorkingArea.Height)
						savedState.Height = primaryScreen.WorkingArea.Height;
				}

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
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Failed to save {form} size and location", form.GetType().Name);
			}
		}
		public static void SaveSizeAndLocation(this Window form, Configuration config)
		{
			if (Design.IsDesignMode) return;

			try
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

				config.SetNonString(saveState, form.GetType().Name);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Failed to save {form} size and location", form.GetType().Name);
			}
		}

		private record FormSizeAndPosition
		{
			public int X;
			public int Y;
			public int Height;
			public int Width;
			public bool IsMaximized;
		}
	}
}
