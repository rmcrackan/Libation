using DataLayer;
using Dinah.Core.Logging;
using Dinah.Core;
using Dinah.Core.Threading;
using LibationWinForms.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms;

public static class MessageBoxLib
{
	internal static DialogResult ShowLicenseRecovery(IWin32Window? owner, string message, string caption)
	{
		using var dialog = new Form
		{
			Text = caption, StartPosition = FormStartPosition.CenterParent,
			Size = new System.Drawing.Size(740, 660), MinimizeBox = false, MaximizeBox = false,
			Padding = new Padding(12)
		};
		var area = (owner is null ? Screen.PrimaryScreen : Screen.FromHandle(owner.Handle))?.WorkingArea;
		if (area is { } bounds)
			dialog.Size = new System.Drawing.Size(Math.Min(740, bounds.Width * 9 / 10), Math.Min(660, bounds.Height * 9 / 10));
		var text = new RichTextBox
		{
			Text = message, ReadOnly = true, DetectUrls = true, Dock = DockStyle.Fill,
			Font = System.Drawing.SystemFonts.MessageBoxFont, BackColor = System.Drawing.SystemColors.Window,
			ScrollBars = RichTextBoxScrollBars.Vertical
		};
		text.LinkClicked += (_, e) =>
		{
			try { Go.To.Url(e.LinkText); }
			catch { MessageBox.Show(dialog, "Could not open the link. Copy it into your browser.", caption); }
		};
		var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Dock = DockStyle.Bottom, Height = 36 };
		dialog.Controls.Add(text);
		dialog.Controls.Add(ok);
		dialog.AcceptButton = ok;
		dialog.CancelButton = ok;
		return dialog.ShowDialog(owner);
	}

	/// <summary>
	/// Logs error. Displays a message box dialog with specified text and caption.
	/// </summary>
	/// <param name="synchronizeInvoke">Form calling this method.</param>
	/// <param name="text">The text to display in the message box.</param>
	/// <param name="caption">The text to display in the title bar of the message box.</param>
	/// <param name="exception">Exception to log.</param>
	public static void ShowAdminAlert(System.ComponentModel.ISynchronizeInvoke? owner, string text, string caption, Exception exception)
	{
		// for development and debugging, show me what broke!
		if (System.Diagnostics.Debugger.IsAttached)
			//Wrap the exception to preserve its stack trace.
			throw new Exception("An unhandled exception was encountered", exception);

		try
		{
			Serilog.Log.Logger.Error(exception, "Alert admin error: {@DebugText}", new { text, caption });
		}
		catch { }

		using var form = new MessageBoxAlertAdminDialog(text, caption, exception);

		if (owner is not null)
		{
			try
			{
				owner.UIThreadSync(() => form.ShowDialog());
				return;
			}
			catch { }
		}

		// synchronizeInvoke is null or previous attempt failed. final try
		form.ShowDialog();
	}

	public static void VerboseLoggingWarning_ShowIfTrue()
	{
		// when turning on debug (and especially Verbose) to share logs, some privacy settings may not be obscured
		if (Log.Logger.IsVerboseEnabled())
			MessageBox.Show(@"
Warning: verbose logging is enabled.

This should be used for debugging only. It creates many
more logs and debug files, neither of which are as
strictly anonymous.

When you are finished debugging, it's highly recommended
to set your debug MinimumLevel to Information and restart
Libation.
".Trim(), "Verbose logging enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
	}

	/// <summary>
	/// Note: the format field should use {0} and NOT use the `$` string interpolation. Formatting is done inside this method.
	/// </summary>
	public static DialogResult ShowConfirmationDialog(IEnumerable<LibraryBook> libraryBooks, string format, string title)
	{
		if (libraryBooks is null || !libraryBooks.Any())
			return DialogResult.Cancel;

		var count = libraryBooks.Count();

		string thisThese = count > 1 ? "these" : "this";
		string bookBooks = count > 1 ? "books" : "book";
		string titlesAgg = libraryBooks.AggregateTitles();

		var message
			= string.Format(format, $"{thisThese} {count} {bookBooks}")
			+ $"\r\n\r\n{titlesAgg}";
		return MessageBox.Show(
			message,
			title,
			MessageBoxButtons.YesNo,
			MessageBoxIcon.Question,
			MessageBoxDefaultButton.Button1);
	}
}
