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
		public UpgradeProperties UpgradeProperties { get; internal init; }
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

	public class Upgrader
	{
		public event EventHandler DownloadBegin;
		public event EventHandler<DownloadProgress> DownloadProgress;
		public event EventHandler<bool> DownloadCompleted;

		public async Task CheForUpgradeAsync(Func<UpgradeEventArgs,Task> upgradeAvailableHandler)
		{
			try
			{
				var upgradeProperties = await Task.Run(LibationScaffolding.GetLatestRelease);
				if (upgradeProperties is null) return;

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
				string upgradeBundle = await DownloadUpgradeAsync(upgradeProperties);

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
				Serilog.Log.Logger.Error(ex, "An error occured while checking for app upgrades.");
			}
		}

		private async Task<string> DownloadUpgradeAsync(UpgradeProperties upgradeProperties)
		{
			if (upgradeProperties.ZipUrl is null)
			{
				Serilog.Log.Logger.Warning("Download link for new version not found");
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

					DownloadProgress?.Invoke(
						this,
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
				Serilog.Log.Logger.Error(ex, "Failed to download the upgrade: {bundle}", upgradeProperties.ZipUrl);
				return null;
			}
		}
	}
}
