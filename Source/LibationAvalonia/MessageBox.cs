using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using DataLayer;
using Dinah.Core.Logging;
using LibationAvalonia.ViewModels.Dialogs;
using LibationAvalonia.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Avalonia.Threading;

namespace LibationAvalonia
{
	public enum DialogResult
	{
		None = 0,
		OK = 1,
		Cancel = 2,
		Abort = 3,
		Retry = 4,
		Ignore = 5,
		Yes = 6,
		No = 7,
		TryAgain = 10,
		Continue = 11
	}

	public enum MessageBoxIcon
	{
		None = 0,
		Error = 16,
		Hand = 16,
		Stop = 16,
		Question = 32,
		Exclamation = 48,
		Warning = 48,
		Asterisk = 64,
		Information = 64
	}

	public enum MessageBoxButtons
	{
		OK,
		OKCancel,
		AbortRetryIgnore,
		YesNoCancel,
		YesNo,
		RetryCancel,
		CancelTryContinue
	}

	public enum MessageBoxDefaultButton
	{
		Button1,
		Button2 = 256,
		Button3 = 512,
	}

	public class MessageBox
	{

		public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			=> ShowCoreAsync(null, text, caption, buttons, icon, defaultButton);
public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, bool saveAndRestorePosition = true)
			=> ShowCoreAsync(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1, saveAndRestorePosition);
public static Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons)
			=> ShowCoreAsync(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(string text, string caption)
			=> ShowCoreAsync(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(string text)
			=> ShowCoreAsync(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
			=> ShowCoreAsync(owner, text, caption, buttons, icon, defaultButton);

public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
			=> ShowCoreAsync(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons)
			=> ShowCoreAsync(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
	public static Task<DialogResult> Show(Window owner, string text, string caption)
			=> ShowCoreAsync(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		public static Task<DialogResult> Show(Window owner, string text)
			=> ShowCoreAsync(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);


		public static async Task VerboseLoggingWarning_ShowIfTrue()
		{
			// when turning on debug (and especially Verbose) to share logs, some privacy settings may not be obscured
			if (Serilog.Log.Logger.IsVerboseEnabled())
				await Show(@"
Warning: verbose logging is enabled.

This should be used for debugging only. It creates many
more logs and debug files, neither of which are as
strictly anonymous.

When you are finished debugging, it's highly recommended
to set your debug MinimumLevel to Information and restart
Libation.
".Trim(), "Verbose logging enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}

		public static async Task<DialogResult> ShowConfirmationDialog(Window owner, IEnumerable<LibraryBook> libraryBooks, string format, string title, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
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

			return await ShowCoreAsync(owner, 
				message,
				title,
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question,
				defaultButton);
		}

		/// <summary>
		/// Logs error. Displays a message box dialog with specified text and caption.
		/// </summary>
		/// <param name="synchronizeInvoke">Form calling this method.</param>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="exception">Exception to log.</param>
		public static async Task ShowAdminAlert(Window owner, string text, string caption, Exception exception)
		{
			// for development and debugging, show me what broke!
			if (System.Diagnostics.Debugger.IsAttached)
				throw exception;

			try
			{
				Serilog.Log.Logger.Error(exception, "Alert admin error: {@DebugText}", new { text, caption });
			}
			catch { }

			var form = new MessageBoxAlertAdminDialog(text, caption, exception);

			await DisplayWindow(form, owner);
		}

		private static async Task<DialogResult> ShowCoreAsync(Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, bool saveAndRestorePosition = true)
		{
			var dialog = await Dispatcher.UIThread.InvokeAsync(() => CreateMessageBox(owner, message, caption, buttons, icon, defaultButton, saveAndRestorePosition));

			return await DisplayWindow(dialog, owner);
		}

		private static MessageBoxWindow CreateMessageBox(Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton, bool saveAndRestorePosition = true)
		{
			owner ??= (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;

			var dialog = new MessageBoxWindow(saveAndRestorePosition);

			dialog.HideMinMaxBtns();

			var vm = new MessageBoxViewModel(message, caption, buttons, icon, defaultButton);
			dialog.DataContext = vm;
			dialog.ControlToFocusOnShow = dialog.FindControl<Control>(defaultButton.ToString());
			dialog.CanResize = false;
			dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var tbx = dialog.FindControl<TextBlock>("messageTextBlock");

			tbx.MinWidth = vm.TextBlockMinWidth;
			tbx.Text = message;

			var thisScreen = owner.Screens.ScreenFromVisual(owner);

			var maxSize = new Size(0.20 * thisScreen.Bounds.Width, 0.9 * thisScreen.Bounds.Height - 55);

			var desiredMax = new Size(maxSize.Width, maxSize.Height);

			tbx.Measure(desiredMax);

			tbx.Height = tbx.DesiredSize.Height;
			tbx.Width = tbx.DesiredSize.Width;
			dialog.MinHeight = vm.FormHeightFromTboxHeight((int)tbx.DesiredSize.Height);
			dialog.MinWidth = vm.FormWidthFromTboxWidth((int)tbx.DesiredSize.Width);
			dialog.MaxHeight = dialog.MinHeight;
			dialog.MaxWidth = dialog.MinWidth;
			dialog.Height = dialog.MinHeight;
			dialog.Width = dialog.MinWidth;
			return dialog;
		}
		private static async Task<DialogResult> DisplayWindow(Window toDisplay, Window owner)
		{
			if (owner is null)
			{
				if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					return await toDisplay.ShowDialog<DialogResult>(desktop.MainWindow);
				}
				else
				{
					var window = new Window
					{
						IsVisible = false,
						Height = 1,
						Width = 1,
						SystemDecorations = SystemDecorations.None,
						ShowInTaskbar = false
					};

					window.Show();
					var result = await toDisplay.ShowDialog<DialogResult>(window);
					window.Close();
					return result;
				}

			}
			else
			{
				return await toDisplay.ShowDialog<DialogResult>(owner);
			}
		}

	}
}
