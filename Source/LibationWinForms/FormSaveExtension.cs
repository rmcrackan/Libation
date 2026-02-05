using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LibationFileManager;

namespace LibationWinForms;

public static class FormSaveExtension
{
	static readonly Icon? libationIcon;
	static FormSaveExtension()
	{
		var resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
		libationIcon = resources.GetObject("$this.Icon") as Icon;
	}

	public static void SetLibationIcon(this Form form)
	{
		form.Icon = libationIcon;
	}

	public static void RestoreSizeAndLocation(this Form form, Configuration config)
	{
		var savedState = config.GetNonString<FormSizeAndPosition?>(defaultValue: null, form.Name);

		if (savedState is null)
			return;

		// too small -- something must have gone wrong. use defaults
		if (savedState.Width < 25 || savedState.Height < 25)
		{
			savedState.Width = form.Width;
			savedState.Height = form.Height;
		}

		var workingArea = ThemeExtensions.GetPrimaryScreenWorkingArea();
		if (workingArea.Width == 0 || workingArea.Height == 0)
			return;
		// Fit to the current screen size in case the screen resolution changed since the size was last persisted
		if (savedState.Width > workingArea.Width)
			savedState.Width = workingArea.Width;
		if (savedState.Height > workingArea.Height)
			savedState.Height = workingArea.Height;

		var x = savedState.X;
		var y = savedState.Y;

		var rect = new Rectangle(x, y, savedState.Width, savedState.Height);

		if (savedState.IsMaximized)
		{
			//When a window is maximized, the client rectangle is not on a screen (y is negative).
			form.StartPosition = FormStartPosition.Manual;
			form.DesktopBounds = rect;

			// FINAL: for Maximized: start normal state, set size and location, THEN set max state
			form.WindowState = FormWindowState.Maximized;
		}
		else
		{
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

			form.WindowState = FormWindowState.Normal;
		}
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

		config.SetNonString(saveState, form.Name);
	}
}
record FormSizeAndPosition
{
	public int X;
	public int Y;
	public int Height;
	public int Width;
	public bool IsMaximized;
}
