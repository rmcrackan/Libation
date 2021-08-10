using System;
using System.Linq;
using Dinah.Core.Logging;
using Serilog;
using System.Windows.Forms;

namespace LibationWinForms
{
	public static class MessageBoxVerboseLoggingWarning
	{
		public static void ShowIfTrue()
		{
			// when turning on debug (and especially Verbose) to share logs, some privacy settings may not be obscured
			if (Log.Logger.IsVerboseEnabled())
				MessageBox.Show(@"
Warning: verbose logging is enabled.

This should be used for debugging only. It creates many
more logs and debug files, neither of which are as
strictly anonomous.

When you are finished debugging, it's highly recommended
to set your debug MinimumLevel to Information and restart
Libation.
".Trim(), "Verbose logging enabled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
		}
	}
}
