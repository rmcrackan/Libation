using LibationAvalonia.Controls;
using LibationAvalonia.ViewModels;
using AppScaffolding;
using LibationFileManager;
using LibationUiBase;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class AboutDialog : DialogWindow
{
	private readonly AboutVM _viewModel;

	/// <summary>
	/// Accepts an optional shared update checker so the About dialog can reflect the same in-flight state
	/// and outcome text as the native macOS "Check for Updates…" menu item.
	/// </summary>
	public AboutDialog(UpdateCheckViewModel? updateChecker = null) : base(saveAndRestorePosition: false)
	{
		InitializeComponent();

		DataContext = _viewModel = new AboutVM(updateChecker);
	}

	private async void CheckForUpgrade_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		var mainWindow = Owner as Views.MainWindow;
		await _viewModel.UpdateChecker.CheckForUpgradeAsync(this, mainWindow);
	}

	private void ContributorLink_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
	{
		if (sender is LinkLabel lbl && lbl.DataContext is LibationContributor contributor)
		{
			Dinah.Core.Go.To.Url(contributor.Link.AbsoluteUri);
		}
	}

	private void Link_getlibation(object sender, Avalonia.Input.TappedEventArgs e) => Dinah.Core.Go.To.Url(AppScaffolding.LibationScaffolding.WebsiteUrl);

	private void ViewReleaseNotes_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		=> Dinah.Core.Go.To.Url($"{AppScaffolding.LibationScaffolding.RepositoryUrl}/releases/tag/v{AppScaffolding.LibationScaffolding.BuildVersion.ToVersionString()}");
}

public class AboutVM : ViewModelBase
{
	public string Version { get; }

	/// <summary>
	/// Shared manual update-check state used by the About button and, on macOS, the app-menu entry.
	/// </summary>
	public UpdateCheckViewModel UpdateChecker { get; }

	public IEnumerable<LibationContributor> PrimaryContributors => LibationContributor.PrimaryContributors;
	public IEnumerable<LibationContributor> AdditionalContributors => LibationContributor.AdditionalContributors;

	public AboutVM(UpdateCheckViewModel? updateChecker = null)
	{
		UpdateChecker = updateChecker ?? new UpdateCheckViewModel();
		Version = $"Libation {AppScaffolding.LibationScaffolding.Variety} v{AppScaffolding.LibationScaffolding.BuildVersion}";
	}
}
