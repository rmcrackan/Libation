using ApplicationServices;
using LibationUiBase;
using LibationUiBase.Forms;
using System;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	private void Configure_SearchIndex()
		=> SearchEngineCommands.UpdateFailed += searchIndexUpdateFailed;

	private async void searchIndexUpdateFailed(object? sender, Exception ex)
	{
		try
		{
			if (!SearchIndexRecovery.ShouldNotify())
				return;

			await MessageBox.Show(
				MainWindow,
				SearchIndexRecovery.ManualRecoveryInstructions,
				SearchIndexRecovery.Caption,
				MessageBoxButtons.OK,
				MessageBoxIcon.Warning);
		}
		catch (Exception dialogEx)
		{
			// nothing above this is allowed to fail: the library change that got us here already succeeded
			Serilog.Log.Logger.Error(dialogEx, "Could not show the search index recovery instructions");
		}
	}
}
