using LibationWinForms.Dialogs;
using System;

namespace LibationWinForms
{
	public static class MessageBoxAlertAdmin
	{
		/// <summary>
		/// Logs error. Displays a message box dialog with specified text and caption.
		/// </summary>
		/// <param name="text">The text to display in the message box.</param>
		/// <param name="caption">The text to display in the title bar of the message box.</param>
		/// <param name="exception">Exception to log</param>
		/// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
		public static System.Windows.Forms.DialogResult Show(string text, string caption, Exception exception)
		{
			Serilog.Log.Logger.Error(exception, "Alert admin error: {@DebugText}", new { text, caption });

			using var form = new MessageBoxAlertAdminDialog(text, caption, exception);
			return form.ShowDialog();
		}
	}
}
