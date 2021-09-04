using FileManager;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms
{
	public static class FormSaveExtension
	{
		public static void RestoreSizeAndLocation(this Form form, Configuration config)
		{
			FormSizeAndPosition savedState = config.GetNonString<FormSizeAndPosition>(form.Name);

			if (savedState is null)
				return;

			// too small -- something must have gone wrong. use defaults
			if (savedState.Width < 25 || savedState.Height < 25)
			{
				savedState.Width = form.Width;
				savedState.Height = form.Height;
			}

			// Fit to the current screen size in case the screen resolution changed since the size was last persisted
			if (savedState.Width > Screen.PrimaryScreen.WorkingArea.Width)
				savedState.Width = Screen.PrimaryScreen.WorkingArea.Width;
			if (savedState.Height > Screen.PrimaryScreen.WorkingArea.Height)
				savedState.Height = Screen.PrimaryScreen.WorkingArea.Height;

			var x = savedState.X;
			var y = savedState.Y;

			var rect = new Rectangle(x, y, savedState.Width, savedState.Height);

			// is proposed rect on a screen?
			if (Screen.AllScreens.Any(screen => screen.WorkingArea.Contains(rect)))
			{
				form.StartPosition = FormStartPosition.Manual;
				form.DesktopBounds = rect;
			}
			else
			{
				form.StartPosition = FormStartPosition.WindowsDefaultLocation;
				form.Size = rect.Size;
			}

			// FINAL: for Maximized: start normal state, set size and location, THEN set max state
			form.WindowState = savedState.IsMaximized ? FormWindowState.Maximized : FormWindowState.Normal;
		}

		public static void SaveSizeAndLocation(this Form form, Configuration config)
		{
			Point location;
			Size size;
			var saveState = new FormSizeAndPosition();

			// save location and size if the state is normal
			if (form.WindowState == FormWindowState.Normal)
			{
				location = form.Location;
				size = form.Size;
			}
			else
			{
				// save the RestoreBounds if the form is minimized or maximized
				location = form.RestoreBounds.Location;
				size = form.RestoreBounds.Size;
			}

			saveState.X = location.X;
			saveState.Y = location.Y;

			saveState.Width = size.Width;
			saveState.Height = size.Height;

			saveState.IsMaximized = form.WindowState == FormWindowState.Maximized;

			config.SetObject(form.Name, saveState);
		}
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
