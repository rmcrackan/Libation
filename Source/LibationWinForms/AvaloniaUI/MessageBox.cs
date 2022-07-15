using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using LibationWinForms.AvaloniaUI.ViewModels.Dialogs;
using LibationWinForms.AvaloniaUI.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationWinForms.AvaloniaUI
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

		/// <summary>Displays a message box with the specified text, caption, buttons, icon, and default button.</summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="buttons">One of the <see cref="T:System.Windows.Forms.MessageBoxButtons" /> values that specifies which buttons to display in the message box.</param>
		/// <param name="icon">One of the <see cref="T:System.Windows.Forms.MessageBoxIcon" /> values that specifies which icon to display in the message box.</param>
		/// <param name="defaultButton">One of the <see cref="T:System.Windows.Forms.MessageBoxDefaultButton" /> values that specifies the default button for the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
		///   <paramref name="buttons" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.  
		/// -or-  
		/// <paramref name="icon" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxIcon" />.  
		/// -or-  
		/// <paramref name="defaultButton" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxDefaultButton" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt was made to display the <see cref="T:System.Windows.Forms.MessageBox" /> in a process that is not running in User Interactive mode. This is specified by the <see cref="P:System.Windows.Forms.SystemInformation.UserInteractive" /> property.</exception>
		public static async Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return await ShowCore(null, text, caption, buttons, icon, defaultButton);
		}


		/// <summary>Displays a message box with specified text, caption, buttons, and icon.</summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="buttons">One of the <see cref="T:System.Windows.Forms.MessageBoxButtons" /> values that specifies which buttons to display in the message box.</param>
		/// <param name="icon">One of the <see cref="T:System.Windows.Forms.MessageBoxIcon" /> values that specifies which icon to display in the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The <paramref name="buttons" /> parameter specified is not a member of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.  
		///  -or-  
		///  The <paramref name="icon" /> parameter specified is not a member of <see cref="T:System.Windows.Forms.MessageBoxIcon" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt was made to display the <see cref="T:System.Windows.Forms.MessageBox" /> in a process that is not running in User Interactive mode. This is specified by the <see cref="P:System.Windows.Forms.SystemInformation.UserInteractive" /> property.</exception>
		public static async Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return await ShowCore(null, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}


		/// <summary>Displays a message box with specified text, caption, and buttons.</summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="buttons">One of the <see cref="T:System.Windows.Forms.MessageBoxButtons" /> values that specifies which buttons to display in the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The <paramref name="buttons" /> parameter specified is not a member of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt was made to display the <see cref="T:System.Windows.Forms.MessageBox" /> in a process that is not running in User Interactive mode. This is specified by the <see cref="P:System.Windows.Forms.SystemInformation.UserInteractive" /> property.</exception>
		public static async Task<DialogResult> Show(string text, string caption, MessageBoxButtons buttons)
		{
			return await ShowCore(null, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}


		/// <summary>Displays a message box with specified text and caption.</summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		public static async Task<DialogResult> Show(string text, string caption)
		{
			return await ShowCore(null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		/// <summary>Displays a message box with specified text.</summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		public static async Task<DialogResult> Show(string text)
		{
			return await ShowCore(null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}


		/// <summary>Displays a message box in front of the specified object and with the specified text, caption, buttons, icon, default button, and options.</summary>
		/// <param name="owner">An implementation of <see cref="T:System.Windows.Forms.IWin32Window" /> that will own the modal dialog box.</param>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="buttons">One of the <see cref="T:System.Windows.Forms.MessageBoxButtons" /> values that specifies which buttons to display in the message box.</param>
		/// <param name="icon">One of the <see cref="T:System.Windows.Forms.MessageBoxIcon" /> values that specifies which icon to display in the message box.</param>
		/// <param name="defaultButton">One of the <see cref="T:System.Windows.Forms.MessageBoxDefaultButton" /> values the specifies the default button for the message box.</param>
		/// <param name="options">One of the <see cref="T:System.Windows.Forms.MessageBoxOptions" /> values that specifies which display and association options will be used for the message box. You may pass in 0 if you wish to use the defaults.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
		///   <paramref name="buttons" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.  
		/// -or-  
		/// <paramref name="icon" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxIcon" />.  
		/// -or-  
		/// <paramref name="defaultButton" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxDefaultButton" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt was made to display the <see cref="T:System.Windows.Forms.MessageBox" /> in a process that is not running in User Interactive mode. This is specified by the <see cref="P:System.Windows.Forms.SystemInformation.UserInteractive" /> property.</exception>
		/// <exception cref="T:System.ArgumentException">		
		/// -or-  
		/// <paramref name="buttons" /> specified an invalid combination of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.</exception>
		public static async Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			return await ShowCore(owner, text, caption, buttons, icon, defaultButton);
		}


		/// <summary>Displays a message box in front of the specified object and with the specified text, caption, buttons, and icon.</summary>
		/// <param name="owner">An implementation of <see cref="T:System.Windows.Forms.IWin32Window" /> that will own the modal dialog box.</param>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="buttons">One of the <see cref="T:System.Windows.Forms.MessageBoxButtons" /> values that specifies which buttons to display in the message box.</param>
		/// <param name="icon">One of the <see cref="T:System.Windows.Forms.MessageBoxIcon" /> values that specifies which icon to display in the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
		///   <paramref name="buttons" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.  
		/// -or-  
		/// <paramref name="icon" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxIcon" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt was made to display the <see cref="T:System.Windows.Forms.MessageBox" /> in a process that is not running in User Interactive mode. This is specified by the <see cref="P:System.Windows.Forms.SystemInformation.UserInteractive" /> property.</exception>
		public static async Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			return await ShowCore(owner, text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
		}

		/// <summary>Displays a message box in front of the specified object and with the specified text, caption, and buttons.</summary>
		/// <param name="owner">An implementation of <see cref="T:System.Windows.Forms.IWin32Window" /> that will own the modal dialog box.</param>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="buttons">One of the <see cref="T:System.Windows.Forms.MessageBoxButtons" /> values that specifies which buttons to display in the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">
		///   <paramref name="buttons" /> is not a member of <see cref="T:System.Windows.Forms.MessageBoxButtons" />.</exception>
		/// <exception cref="T:System.InvalidOperationException">An attempt was made to display the <see cref="T:System.Windows.Forms.MessageBox" /> in a process that is not running in User Interactive mode. This is specified by the <see cref="P:System.Windows.Forms.SystemInformation.UserInteractive" /> property.</exception>
		public static async Task<DialogResult> Show(Window owner, string text, string caption, MessageBoxButtons buttons)
		{
			return await ShowCore(owner, text, caption, buttons, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		/// <summary>Displays a message box in front of the specified object and with the specified text and caption.</summary>
		/// <param name="owner">An implementation of <see cref="T:System.Windows.Forms.IWin32Window" /> that will own the modal dialog box.</param>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		public static async Task<DialogResult> Show(Window owner, string text, string caption)
		{
			return await ShowCore(owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		/// <summary>Displays a message box in front of the specified object and with the specified text.</summary>
		/// <param name="owner">An implementation of <see cref="T:System.Windows.Forms.IWin32Window" /> that will own the modal dialog box.</param>
		/// <param name="text">The text to display in the message box.</param>
		/// <returns>One of the <see cref="T:System.Windows.Forms.DialogResult" /> values.</returns>
		public static async Task<DialogResult> Show(Window owner, string text)
		{
			return await ShowCore(owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
		}

		private static async Task<DialogResult> ShowCore(Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
				return await ShowCore2(owner, message, caption, buttons, icon, defaultButton);
			else
				return await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => ShowCore2(owner, message, caption, buttons, icon, defaultButton));

		}
		private static async Task<DialogResult> ShowCore2(Window owner, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{
			var dialog = new MessageBoxWindow();

#if WINDOWS7_0
			HideMinMaxBtns(dialog.PlatformImpl.Handle.Handle);
#endif

			var vm = new MessageBoxViewModel(message, caption, buttons, icon, defaultButton);
			dialog.DataContext = vm;
			dialog.CanResize = false;
			dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			var tbx = dialog.FindControl<TextBlock>("messageTextBlock");

			tbx.MinWidth = vm.TextBlockMinWidth;
			tbx.Text = message;

			var thisScreen = (owner ?? dialog).Screens.ScreenFromVisual(owner ?? dialog);

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

			dialog.Opened += (_, _) =>
			{
				switch (defaultButton)
				{
					case MessageBoxDefaultButton.Button1:
						dialog.FindControl<Button>("Button1").Focus();
						break;
					case MessageBoxDefaultButton.Button2:
						dialog.FindControl<Button>("Button2").Focus();
						break;
					case MessageBoxDefaultButton.Button3:
						dialog.FindControl<Button>("Button3").Focus();
						break;

				}
			};

			if (owner is null)
			{
				if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
				{
					return await dialog.ShowDialog<DialogResult>(desktop.MainWindow);
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
					var result = await dialog.ShowDialog<DialogResult>(window);
					window.Close();
					return result;
				}

			}
			else
			{
				return await dialog.ShowDialog<DialogResult>(owner);
			}
		}

#if WINDOWS7_0

		private static void HideMinMaxBtns(IntPtr handle)
		{
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
#endif
	}
}
