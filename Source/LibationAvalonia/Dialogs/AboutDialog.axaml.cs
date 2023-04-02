using Avalonia.Collections;
using Avalonia.Controls;
using LibationAvalonia.ViewModels;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public partial class AboutDialog : DialogWindow
	{
		private readonly AboutVM _viewModel;
		public AboutDialog() : base(saveAndRestorePosition:false)
		{
			if (Design.IsDesignMode)
				_ = Configuration.Instance.LibationFiles;

			InitializeComponent();

			DataContext = _viewModel = new AboutVM();
		}

		private async void CheckForUpgrade_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var mainWindow = Owner as Views.MainWindow;

			var upgrader = new Upgrader();
			upgrader.DownloadProgress += async (_, e) => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => mainWindow.ViewModel.DownloadProgress = e.ProgressPercentage);
			upgrader.DownloadCompleted += async (_, _) => await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => mainWindow.ViewModel.DownloadProgress = null);

			_viewModel.CanCheckForUpgrade = false;
			Version latestVersion = null;
			await upgrader.CheckForUpgradeAsync(OnUpgradeAvailable);

			_viewModel.CanCheckForUpgrade = latestVersion is null;

			_viewModel.UpgradeButtonText = latestVersion is null ? "Libation is up to date. Check Again." : $"Version {latestVersion:3} is available";

			async Task OnUpgradeAvailable(UpgradeEventArgs e)
			{
				var notificationResult = await new UpgradeNotificationDialog(e.UpgradeProperties, e.CapUpgrade).ShowDialogAsync(this);

				e.Ignore = notificationResult == DialogResult.Ignore;
				e.InstallUpgrade = notificationResult == DialogResult.OK;
				latestVersion = e.UpgradeProperties.LatestRelease;
			}
		}

		private void ViewReleaseNotes_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		{
			Dinah.Core.Go.To.Url($"{AppScaffolding.LibationScaffolding.RepositoryUrl}/releases/tag/v{AppScaffolding.LibationScaffolding.BuildVersion.ToString(3)}");
		}
	}

	public class AboutVM : ViewModelBase
	{
		public string Version { get; }
		public AvaloniaList<AssemblyName> Assemblies { get; } = new();
		public bool CanCheckForUpgrade { get => canCheckForUpgrade; set => this.RaiseAndSetIfChanged(ref canCheckForUpgrade, value); }
		public string UpgradeButtonText { get => upgradeButtonText; set => this.RaiseAndSetIfChanged(ref upgradeButtonText, value); }


		private bool canCheckForUpgrade = true;
		private string upgradeButtonText = "Check for Upgrade";

		public AboutVM()
		{
			Version = $"Libation {AppScaffolding.LibationScaffolding.Variety} v{AppScaffolding.LibationScaffolding.BuildVersion}";
			Assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Select(a => new AssemblyName(a.FullName)).Where(a => a.Version.Major + a.Version.Minor + a.Version.Build + a.Version.Revision > 0).OrderBy(a => a.Name));
		}

		public async Task CopyAssembliesAsync() => await App.Current.Clipboard.SetTextAsync(string.Join(Environment.NewLine, Assemblies.Select(a => $"{a.Name}\t{a.Version}")));
	}
}
