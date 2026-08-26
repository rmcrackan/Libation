using AudibleApi;
using AudibleUtilities;
using Avalonia.Collections;
using LibationUiBase;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

/// <summary>
/// Which Audible marketplaces one account should read. Opened from the accounts grid, and only for an account
/// that has already logged in - the check is made with that account's own credentials.
/// </summary>
public partial class MarketplacesDialog : DialogWindow
{
	public string Intro => MarketplacesUi.Intro;
	public string CheckButtonText => MarketplacesUi.CheckButton;
	public string AccountLabel { get; } = "";

	public AvaloniaList<ListItem> Marketplaces { get; } = new();

	/// <summary>The additional marketplaces the user checked. The registered one is never among them.</summary>
	public IReadOnlyList<string> SelectedAdditionalLocaleNames
		=> Marketplaces
			.Where(m => m.IsChecked && m.CanCheck)
			.Select(m => m.Locale.Name)
			.ToList();

	public class ListItem : ViewModels.ViewModelBase
	{
		public ListItem(Locale locale, string text, bool isChecked, bool canCheck, string? toolTip = null)
		{
			Locale = locale;
			Text = text;
			IsChecked = isChecked;
			CanCheck = canCheck;
			ToolTip = toolTip;
		}

		public Locale Locale { get; }
		public string Text
		{
			get => field;
			set => this.RaiseAndSetIfChanged(ref field, value);
		}
		public bool IsChecked
		{
			get => field;
			set => this.RaiseAndSetIfChanged(ref field, value);
		}
		public bool CanCheck
		{
			get => field;
			set => this.RaiseAndSetIfChanged(ref field, value);
		}
		public string? ToolTip { get; }
		public override string ToString() => Text;
	}

	private readonly Account? account;
	private readonly AccountsSettings? accountsSettings;

	// parameterless ctor for the axaml designer
	public MarketplacesDialog()
	{
		InitializeComponent();
		DataContext = this;
	}

	/// <param name="selectedAdditionalLocaleNames">
	/// What the accounts grid currently shows for this account, which may not yet be saved.
	/// </param>
	public MarketplacesDialog(Account account, AccountsSettings accountsSettings, IEnumerable<string> selectedAdditionalLocaleNames)
	{
		InitializeComponent();

		this.account = account;
		this.accountsSettings = accountsSettings;

		AccountLabel = AccountCredentialStatus.FormatAccountLabel(account);

		var selected = selectedAdditionalLocaleNames.ToHashSet();

		// list every candidate up front, so the dialog is a full picture before anything is asked of Audible
		foreach (var locale in MarketplaceProbe.CandidateLocales(account))
		{
			var isRegistered = locale.Name == account.Locale?.Name;

			Marketplaces.Add(new ListItem(
				locale,
				isRegistered
					? $"{locale.Name} - this account's own marketplace"
					: locale.Name,
				isChecked: isRegistered || selected.Contains(locale.Name),
				canCheck: !isRegistered,
				toolTip: isRegistered ? "Always scanned. This is where the account is registered." : null));
		}

		StatusTextBlock.Text = MarketplacesUi.ButtonToolTip;
		ControlToFocusOnShow = CheckButton;
		DataContext = this;
	}

	public async void CheckButton_Clicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> await ProbeAsync();

	public async Task ProbeAsync()
	{
		if (account is null || accountsSettings is null)
			return;

		CheckButton.IsEnabled = false;
		StatusTextBlock.Text = MarketplacesUi.Checking;

		var results = new List<MarketplaceProbeResult>();

		try
		{
			await foreach (var result in MarketplaceProbe.ProbeAsync(account, accountsSettings))
			{
				results.Add(result);

				if (Marketplaces.FirstOrDefault(m => m.Locale.Name == result.Locale.Name) is not { } item)
					continue;

				item.Text = MarketplacesUi.ResultText(result);

				// a marketplace another account already scans must not be checkable here: two rows scanning one
				// marketplace would import it twice
				if (result.Outcome is MarketplaceProbeOutcome.ScannedByAnotherAccount or MarketplaceProbeOutcome.Failed)
					item.CanCheck = false;

				if (result.Outcome is MarketplaceProbeOutcome.TitlesFound)
					item.IsChecked = true;
			}

			StatusTextBlock.Text = MarketplacesUi.Summary(results);
		}
		finally
		{
			CheckButton.IsEnabled = true;
		}
	}

	public new void SaveAndClose() => base.SaveAndClose();
}
