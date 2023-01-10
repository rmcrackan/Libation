using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LibationFileManager;

namespace LibationAvalonia
{
	public static class FormSaveExtension
	{
		static readonly WindowIcon WindowIcon;
		static FormSaveExtension()
		{
			if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow is not null)
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
			if (Design.IsDesignMode) return;
			try
			{
				var savedState = config.GetNonString<FormSizeAndPosition>(defaultValue: null, form.GetType().Name);

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

		public static void HideMinMaxBtns(this Window form)
		{
			if (Design.IsDesignMode || !Configuration.IsWindows)
				return;
			var handle = form.PlatformImpl.Handle.Handle;
			var currentStyle = GetWindowLong(handle, GWL_STYLE);

			SetWindowLong(handle, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
		}

		const long WS_MINIMIZEBOX = 0x00020000L;
		const long WS_MAXIMIZEBOX = 0x10000L;
		const int GWL_STYLE = -16;
		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		static extern long GetWindowLong(IntPtr hWnd, int nIndex);
		[System.Runtime.InteropServices.DllImport("user32.dll")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
	}
}
