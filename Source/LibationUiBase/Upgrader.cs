using AppScaffolding;
using Dinah.Core.Net.Http;
using LibationFileManager;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LibationUiBase
{
	public class UpgradeEventArgs
	{
		public required UpgradeProperties UpgradeProperties { get; init; }
		public bool CapUpgrade { get; internal init; }
		private bool _ignore = false;
		private bool _installUpgrade = true;
		public bool Ignore
		{
			get => _ignore;
			set
			{
				_ignore = value;
				_installUpgrade &= !Ignore;
			}
		}
		public bool InstallUpgrade
		{
			get => _installUpgrade;
			set
			{
				_installUpgrade = value;
				_ignore &= !InstallUpgrade;
			}
		}
	}

	public class Upgrader : UpgraderBase
	{
		protected override async Task<UpgradeProperties?> CheckForUpgradeAsync()
		{
			try
			{
				return await Task.Run(LibationScaffolding.GetLatestRelease);
			}
			catch (Exception ex)
			{
				string message = "An error occurred while checking for app upgrades.";
				Serilog.Log.Logger.Error(ex, message);
				OnUpgradeFailed(message, ex);
				return null;
			}
		}

		protected override async Task<string?> DownloadUpgradeAsync(UpgradeProperties upgradeProperties)
		{
			if (upgradeProperties.ZipUrl is null)
			{
				string message = "Download link for new version not found.";
				Serilog.Log.Logger.Warning(message);
				OnUpgradeFailed(message, null);
				return null;
			}

			//Silently download the upgrade in the background, save it to a temp file.

			var zipFile = Path.Combine(Path.GetTempPath(), Path.GetFileName(upgradeProperties.ZipUrl));

			Serilog.Log.Logger.Information($"Downloading {zipFile}");

			try
			{
				using var dlClient = new HttpClient();
				using var response = await dlClient.GetAsync(upgradeProperties.ZipUrl, HttpCompletionOption.ResponseHeadersRead);
				using var dlStream = await response.Content.ReadAsStreamAsync();
				using var tempFile = File.OpenWrite(zipFile);

				int read;
				long totalRead = 0;
				Memory<byte> buffer = new byte[128 * 1024];
				long contentLength = response.Content.Headers.ContentLength ?? 0;

				while ((read = await dlStream.ReadAsync(buffer)) > 0)
				{
					await tempFile.WriteAsync(buffer[..read]);
					totalRead += read;

					OnDownloadProgress(
						new DownloadProgress
						{
							BytesReceived = totalRead,
							TotalBytesToReceive = contentLength,
							ProgressPercentage = contentLength > 0 ? 100d * totalRead / contentLength : 0
						});
				}

				return zipFile;
			}
			catch (Exception ex)
			{
				var message = $"Failed to download the upgrade: {upgradeProperties.ZipUrl}";
				Serilog.Log.Logger.Error(ex, message);
				OnUpgradeFailed(message, ex);
				return null;
			}
		}
	}

	public class MockUpgrader : UpgraderBase
	{
		public int DownloadTimeMs { get; set; } = 3000;
		public int DownloadSizeInBytes { get; set; } = 150 * 1024 * 1024;
		public bool CheckForUpgradeSucceeds { get; set; } = true;
		public bool DownloadUpgradeSucceeds { get; set; } = true;
		public string? MockUpgradeBundle { get; set; }

		protected override Task<UpgradeProperties?> CheckForUpgradeAsync()
		{
			if (!CheckForUpgradeSucceeds)
			{
				OnUpgradeFailed("Mock Check For Upgrade Failed", null);
				return Task.FromResult<UpgradeProperties?>(null);
			}
			return Task.FromResult<UpgradeProperties?>(new UpgradeProperties(
						"http://fake.url/to/bundle.zip",
						"",
						Path.GetFileName(MockUpgradeBundle) ?? "",
						LibationScaffolding.BuildVersion ?? new(1, 0, 0, 0),
						"<RELEASE NOTES>"));
		}

		protected override async Task<string?> DownloadUpgradeAsync(UpgradeProperties upgradeProperties)
		{
			if (!File.Exists(MockUpgradeBundle))
			{
				OnUpgradeFailed("Mock Download bundle file not found", null);
				return null;
			}

			for (int i = 1; i <= 100; i++)
			{
				await Task.Delay(DownloadTimeMs / 100);
				OnDownloadProgress(new()
				{
					BytesReceived = DownloadSizeInBytes / 100,
					ProgressPercentage = i,
					TotalBytesToReceive = DownloadSizeInBytes * i / 100
				});
			}
			if (!DownloadUpgradeSucceeds)
			{
				OnUpgradeFailed("Mock Download Upgrade Failed", null);
				return null;
			}

			return MockUpgradeBundle;
		}
	}

	public abstract class UpgraderBase
	{
		public event EventHandler? DownloadBegin;
		public event EventHandler<DownloadProgress>? DownloadProgress;
		public event EventHandler<bool>? DownloadCompleted;
		public event EventHandler<string>? UpgradeFailed;

		protected void OnDownloadProgress(DownloadProgress args) => DownloadProgress?.Invoke(this, args);
		protected void OnUpgradeFailed(string message, Exception? ex)
			=> UpgradeFailed?.Invoke(this, (message + Environment.NewLine + Environment.NewLine + ex?.Message).Trim());
		protected abstract Task<UpgradeProperties?> CheckForUpgradeAsync();
		protected abstract Task<string?> DownloadUpgradeAsync(UpgradeProperties upgradeProperties);

		public async Task CheckForUpgradeAsync(Func<UpgradeEventArgs, Task> upgradeAvailableHandler)
		{
			try
			{
				if (await CheckForUpgradeAsync() is not UpgradeProperties upgradeProperties)
					return;

				const string ignoreUpgrade = "IgnoreUpgrade";
				var config = Configuration.Instance;

				if (config.GetString(propertyName: ignoreUpgrade) == upgradeProperties.LatestRelease.ToString())
					return;

				var interop = InteropFactory.Create();

				if (!interop.CanUpgrade)
					Serilog.Log.Logger.Information("Can't perform upgrade automatically");

				var upgradeEventArgs = new UpgradeEventArgs
				{
					UpgradeProperties = upgradeProperties,
					CapUpgrade = interop.CanUpgrade
				};

				await upgradeAvailableHandler(upgradeEventArgs);

				if (upgradeEventArgs.Ignore)
					config.SetString(upgradeProperties.LatestRelease.ToString(), ignoreUpgrade);

				if (!upgradeEventArgs.InstallUpgrade) return;

				//Download the upgrade file in the background,
				DownloadBegin?.Invoke(this, EventArgs.Empty);
				string? upgradeBundle = await DownloadUpgradeAsync(upgradeProperties);

				if (string.IsNullOrEmpty(upgradeBundle) || !File.Exists(upgradeBundle))
				{
					DownloadCompleted?.Invoke(this, false);
				}
				else
				{
					DownloadCompleted?.Invoke(this, true);

					//Install the upgrade
					Serilog.Log.Logger.Information($"Begin running auto-upgrader");
					interop.InstallUpgrade(upgradeBundle);
					Serilog.Log.Logger.Information($"Completed running auto-upgrader");
				}
			}
			catch (Exception ex)
			{
				var message = "An error occurred while checking for app upgrades.";
				Serilog.Log.Logger.Error(ex, message);
				OnUpgradeFailed(message, ex);
			}
		}
	}
}
