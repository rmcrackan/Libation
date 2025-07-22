using ApplicationServices;
using AudibleUtilities;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable
namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private readonly InterruptableTimer autoScanTimer = new InterruptableTimer(TimeSpan.FromMinutes(5));

		private void Configure_ScanAuto()
		{
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
					await Task.Run(() => LibraryCommands.ImportAccountAsync(accounts));
				}
				catch (OperationCanceledException)
				{
					Serilog.Log.Information("Audible login attempt cancelled by user");
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error invoking auto-scan");
				}
			};

			// if enabled: begin on load
			MainWindow.Loaded += startAutoScan;

			// if new 'default' account is added, run autoscan
			AccountsSettingsPersister.Saving += accountsPreSave;
			AccountsSettingsPersister.Saved += accountsPostSave;

			// when autoscan setting is changed, update menu checkbox and run autoscan
			Configuration.Instance.PropertyChanged += startAutoScan;
		}


		private List<(string AccountId, string LocaleName)>? preSaveDefaultAccounts;
		private List<(string AccountId, string LocaleName)> getDefaultAccounts()
		{
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			return persister.AccountsSettings
				.GetAll()
				.Where(a => a.LibraryScan)
				.Select(a => (a.AccountId, a.Locale.Name))
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
			AutoScanChecked = Configuration.Instance.AutoScan;
			if (AutoScanChecked)
				autoScanTimer.PerformNow();
			else
				autoScanTimer.Stop();
		}
	}
}
