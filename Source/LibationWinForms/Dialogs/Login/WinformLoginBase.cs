using System;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs.Login
{
	public abstract class WinformLoginBase
	{
		private readonly IWin32Window _owner;
		protected WinformLoginBase(IWin32Window owner)
		{
			_owner = owner;
		}

		/// <returns>True if ShowDialog's DialogResult == OK</returns>
		protected bool ShowDialog(Form dialog)
		{
			var result = dialog.ShowDialog(_owner);
			Serilog.Log.Logger.Debug("{@DebugInfo}", new { DialogResult = result });
			return result == DialogResult.OK;
		}
	}
}
