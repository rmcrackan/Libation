using System;

namespace LibationWinForms.Dialogs.Login
{
	public abstract class WinformLoginBase
	{
		/// <returns>True if ShowDialog's DialogResult == OK</returns>
		protected static bool ShowDialog(System.Windows.Forms.Form dialog)
		{
			var result = dialog.ShowDialog();
			Serilog.Log.Logger.Debug("{@DebugInfo}", new { DialogResult = result });
			return result == System.Windows.Forms.DialogResult.OK;
		}
	}
}
