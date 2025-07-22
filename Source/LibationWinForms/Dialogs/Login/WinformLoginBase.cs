using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public abstract class WinformLoginBase
	{
		protected Control Owner { get; }
		protected WinformLoginBase(Control owner)
		{
			Owner = owner;
		}

		/// <returns>True if ShowDialog's DialogResult == OK</returns>
		protected bool ShowDialog(Form dialog)
			=> Owner.Invoke(() =>
			{
				var result = dialog.ShowDialog(Owner);
				Serilog.Log.Logger.Debug("{@DebugInfo}", new { DialogResult = result });
				return result == DialogResult.OK;
			});
	}
}
