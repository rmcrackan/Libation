using ApplicationServices;
using LibationUiBase;
using System;
using System.Windows.Forms;

namespace LibationWinForms;

public partial class Form1
{
	private void Configure_SearchIndex()
		=> SearchEngineCommands.UpdateFailed += searchIndexUpdateFailed;

	private void searchIndexUpdateFailed(object? sender, Exception ex)
	{
		try
		{
			if (!SearchIndexRecovery.ShouldNotify())
				return;

			if (InvokeRequired)
			{
				BeginInvoke(showSearchIndexRecoveryInstructions);
				return;
			}

			showSearchIndexRecoveryInstructions();
		}
		catch (Exception dialogEx)
		{
			// nothing above this is allowed to fail: the library change that got us here already succeeded
			Serilog.Log.Logger.Error(dialogEx, "Could not show the search index recovery instructions");
		}
	}

	private void showSearchIndexRecoveryInstructions()
		=> MessageBox.Show(
			this,
			SearchIndexRecovery.ManualRecoveryInstructions,
			SearchIndexRecovery.Caption,
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
}
