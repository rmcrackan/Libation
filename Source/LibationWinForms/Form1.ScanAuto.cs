using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationServices;
using AudibleUtilities;
using Dinah.Core;
using LibationFileManager;

namespace LibationWinForms
{
	// This is for the auto-scanner. It is unrelated to manual scanning/import
	public partial class Form1
	{
		private InterruptableTimer autoScanTimer;

		private void Configure_ScanAuto()
        {
			// creating InterruptableTimer inside 'Configure_' is a break from the pattern. As long as no one else needs to access or subscribe to it, this is ok

			autoScanTimer = new InterruptableTimer(TimeSpan.FromMinutes(5));

			// subscribe as async/non-blocking. I'd actually rather prefer blocking but real-world testing found that caused a deadlock in the AudibleAPI
			autoScanTimer.Elapsed += async (_, __) =>
			{
				using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
				var accounts = persister.AccountsSettings
					.GetAll()
					.Where(a => a.LibraryScan)
					.ToArray();

				// in autoScan, new books SHALL NOT show dialog
				try
				{
					Task importAsync() => LibraryCommands.ImportAccountAsync(Login.WinformLoginChoiceEager.ApiExtendedFunc, accounts);
					if (InvokeRequired)
						await Invoke(importAsync);
					else
						await importAsync();
				}
                catch (Exception ex)
                {
					Serilog.Log.Logger.Error(ex, "Error invoking auto-scan");
                }
			};

			// load init state to menu checkbox
			Load += updateAutoScanLibraryToolStripMenuItem;
			// if enabled: begin on load
			Load += startAutoScan;

			// if new 'default' account is added, run autoscan
			AccountsSettingsPersister.Saving += accountsPreSave;
			AccountsSettingsPersister.Saved += accountsPostSave;

			Configuration.Instance.PropertyChanged += Configuration_PropertyChanged;
		}


		[PropertyChangeFilter(nameof(Configuration.AutoScan))]
		private void Configuration_PropertyChanged(object sender, PropertyChangedEventArgsEx e)
		{
			// when autoscan setting is changed, update menu checkbox and run autoscan
			updateAutoScanLibraryToolStripMenuItem(sender, e);
			startAutoScan(sender, e);
		}

		private List<(string AccountId, string LocaleName)> preSaveDefaultAccounts;
		private List<(string AccountId, string LocaleName)> getDefaultAccounts()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			return persister.AccountsSettings
				.GetAll()
				.Where(a => a.LibraryScan)
				.Select(a => (a.AccountId, a.Locale.Name))
				.ToList();
		}
		private void accountsPreSave(object sender = null, EventArgs e = null)
			=> preSaveDefaultAccounts = getDefaultAccounts();
		private void accountsPostSave(object sender = null, EventArgs e = null)
		{
			var postSaveDefaultAccounts = getDefaultAccounts();
			var newDefaultAccounts = postSaveDefaultAccounts.Except(preSaveDefaultAccounts).ToList();

			if (newDefaultAccounts.Any())
				startAutoScan();
		}

		private void startAutoScan(object sender = null, EventArgs e = null)
		{
			if (Configuration.Instance.AutoScan)
				autoScanTimer.PerformNow();
			else
				autoScanTimer.Stop();
		}

		private void updateAutoScanLibraryToolStripMenuItem(object sender, EventArgs e) => autoScanLibraryToolStripMenuItem.Checked = Configuration.Instance.AutoScan;

		private void autoScanLibraryToolStripMenuItem_Click(object sender, EventArgs e) => Configuration.Instance.AutoScan = !autoScanLibraryToolStripMenuItem.Checked;
	}
}
