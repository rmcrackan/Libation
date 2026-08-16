using ApplicationServices;
using AudibleUtilities;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	private readonly InterruptableTimer autoScanTimer = new(TimeSpan.FromMinutes(5));
	private AutoScanRunner? autoScanRunner;

	private void Configure_ScanAuto()
	{
		autoScanRunner = new AutoScanRunner(
			isAutoScanEnabled: () => Configuration.Instance.AutoScan,
			pauseTimer: () => autoScanTimer.Stop(),
			resumeTimer: () => autoScanTimer.PerformNow(),
			notifyAuthRequired: notifyAutoScanAuthRequiredAsync);

		autoScanTimer.Elapsed += async (_, __) => await autoScanRunner.RunAsync();

		MainWindow.Loaded += startAutoScan;

		AccountsSettingsPersister.Saving += accountsPreSave;
		AccountsSettingsPersister.Saved += accountsPostSave;

		Configuration.Instance.PropertyChanged += startAutoScan;
	}

	private async Task notifyAutoScanAuthRequiredAsync(AuthenticationRequiredException ex)
	{
		await MessageBox.Show(
			MainWindow,
			AutoScanAuthPrompt.FormatBody(ex),
			AutoScanAuthPrompt.Caption,
			MessageBoxButtons.OK,
			MessageBoxIcon.Warning);
	}

	private List<(string AccountId, string LocaleName)>? preSaveDefaultAccounts;
	private List<(string AccountId, string LocaleName)> getDefaultAccounts()
	{
		using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
		return persister.AccountsSettings
			.GetAll()
			.Where(a => a.LibraryScan)
			.Select(a => (a.AccountId, a.Locale?.Name))
			.OfType<(string, string)>()
			.ToList();
	}

	private void accountsPreSave(object? sender = null, EventArgs? e = null)
		=> preSaveDefaultAccounts = getDefaultAccounts();

	private void accountsPostSave(object? sender = null, EventArgs? e = null)
	{
		if (getDefaultAccounts().Except(preSaveDefaultAccounts ?? Enumerable.Empty<(string AccountId, string LocaleName)>()).Any())
			startAutoScan();
	}

	[PropertyChangeFilter(nameof(Configuration.AutoScan))]
	private void startAutoScan(object? sender = null, EventArgs? e = null)
	{
		autoScanRunner?.OnAutoScanSettingChanged();
		AutoScanChecked = Configuration.Instance.AutoScan;
		if (AutoScanChecked)
			autoScanTimer.PerformNow();
		else
			autoScanTimer.Stop();
	}
}
