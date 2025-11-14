using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Dinah.Core;
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
				if ((form.Screens.Primary ?? form.Screens.All.FirstOrDefault()) is Screen primaryScreen)
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

		public static void HideMinMaxBtns(this Window form)
		{
			if (Design.IsDesignMode || !Configuration.IsWindows || form.TryGetPlatformHandle() is not IPlatformHandle handle)
				return;

			var windowStyle
				= GetWindowStyle(handle.Handle)
				.Remove(WINDOW_STYLE.WS_MINIMIZEBOX)
				.Remove(WINDOW_STYLE.WS_MAXIMIZEBOX);

			SetWindowStyle(handle.Handle, windowStyle);
		}

		const int GWL_STYLE = -16;

		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "GetWindowLong")]
		static extern long GetWindowLong(IntPtr hWnd, int nIndex);

		[System.Runtime.InteropServices.DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		static extern int SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);

		static WINDOW_STYLE GetWindowStyle(IntPtr hWnd) => (WINDOW_STYLE)GetWindowLong(hWnd, GWL_STYLE);
		static void SetWindowStyle(IntPtr hWnd, WINDOW_STYLE style) => SetWindowLong(hWnd, GWL_STYLE, (long)style);
		

		[Flags]
		enum WINDOW_STYLE : long
		{
			WS_OVERLAPPED = 0x0,
			WS_TILED = 0x0,
			WS_ACTIVECAPTION = 0x1,
			WS_MAXIMIZEBOX = 0x10000,
			WS_TABSTOP = 0x10000,
			WS_MINIMIZEBOX = 0x20000,
			WS_GROUP = 0x20000,
			WS_THICKFRAME = 0x40000,
			WS_SIZEBOX = 0x40000,
			WS_SYSMENU = 0x80000,
			WS_HSCROLL = 0x100000,
			WS_VSCROLL = 0x200000,
			WS_DLGFRAME = 0x400000,
			WS_BORDER = 0x800000,
			WS_CAPTION = 0xc00000,
			WS_OVERLAPPEDWINDOW = 0xcf0000,
			WS_TILEDWINDOW = 0xcf0000,
			WS_MAXIMIZE = 0x1000000,
			WS_CLIPCHILDREN = 0x2000000,
			WS_CLIPSIBLINGS = 0x4000000,
			WS_DISABLED = 0x8000000,
			WS_VISIBLE = 0x10000000,
			WS_ICONIC = 0x20000000,
			WS_MINIMIZE = 0x20000000,
			WS_CHILD = 0x40000000,
			WS_CHILDWINDOW = 0x40000000,
			WS_POPUP = 0x80000000,
			WS_POPUPWINDOW = 0x80880000
		}
	}
}
