using AudibleApi;
using AudibleUtilities;
using LibationUiBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs;

/// <summary>
/// Which Audible marketplaces one account should read. Opened from the accounts grid, and only for an account
/// that has already logged in - the check is made with that account's own credentials.
/// </summary>
public partial class MarketplacesDialog : Form
{
	private readonly Account account;
	private readonly AccountsSettings accountsSettings;

	/// <summary>Locale name for each row, by index, so a checked box maps back to a marketplace.</summary>
	private readonly List<Locale> rowLocales = new();

	/// <summary>The additional marketplaces the user checked. The registered one is never among them.</summary>
	public IReadOnlyList<string> SelectedAdditionalLocaleNames { get; private set; } = [];

	/// <param name="selectedAdditionalLocaleNames">
	/// What the accounts grid currently shows for this account, which may not yet be saved.
	/// </param>
	public MarketplacesDialog(Account account, AccountsSettings accountsSettings, IEnumerable<string> selectedAdditionalLocaleNames)
	{
		this.account = account;
		this.accountsSettings = accountsSettings;

		InitializeComponent();
		this.SetLibationIcon();

		introLbl.Text = MarketplacesUi.Intro;
		accountLbl.Text = AccountCredentialStatus.FormatAccountLabel(account);
		checkBtn.Text = MarketplacesUi.CheckButton;
		statusLbl.Text = MarketplacesUi.ButtonToolTip;

		var selected = selectedAdditionalLocaleNames.ToHashSet();

		// list every candidate up front, so the dialog is a full picture before anything is asked of Audible
		foreach (var locale in MarketplaceProbe.CandidateLocales(account))
		{
			var isRegistered = locale.Name == account.Locale?.Name;

			var text = isRegistered
				? $"{locale.Name} - this account's own marketplace"
				: locale.Name;

			rowLocales.Add(locale);
			marketplacesClb.Items.Add(text, isRegistered || selected.Contains(locale.Name));
		}
	}

	/// <summary>The account's own marketplace is always scanned and must not be unchecked.</summary>
	private void marketplacesClb_ItemCheck(object sender, ItemCheckEventArgs e)
	{
		if (rowLocales[e.Index].Name == account.Locale?.Name && e.NewValue == CheckState.Unchecked)
			e.NewValue = CheckState.Checked;
	}

	private async void checkBtn_Click(object sender, EventArgs e)
	{
		checkBtn.Enabled = false;
		statusLbl.Text = MarketplacesUi.Checking;

		var results = new List<MarketplaceProbeResult>();
		var unavailable = new HashSet<int>();

		try
		{
			await foreach (var result in MarketplaceProbe.ProbeAsync(account, accountsSettings))
			{
				results.Add(result);

				var index = rowLocales.FindIndex(l => l.Name == result.Locale.Name);
				if (index < 0)
					continue;

				// a marketplace another account already scans must not be checkable here: two rows scanning one
				// marketplace would import it twice. one that could not be reached is not offered either.
				var offerable = result.Outcome is not
					(MarketplaceProbeOutcome.ScannedByAnotherAccount or MarketplaceProbeOutcome.Failed);

				if (!offerable)
					unavailable.Add(index);

				// replacing the item can reset its check mark, so the intended state is always re-applied
				var wasChecked = marketplacesClb.GetItemChecked(index);
				marketplacesClb.Items[index] = MarketplacesUi.ResultText(result);
				marketplacesClb.SetItemChecked(
					index,
					offerable && (wasChecked || result.Outcome is MarketplaceProbeOutcome.TitlesFound));
			}

			statusLbl.Text = MarketplacesUi.Summary(results);
			unavailableIndexes = unavailable;
		}
		finally
		{
			checkBtn.Enabled = true;
		}
	}

	private HashSet<int> unavailableIndexes = new();

	private void saveBtn_Click(object sender, EventArgs e)
	{
		SelectedAdditionalLocaleNames = marketplacesClb.CheckedIndices
			.Cast<int>()
			.Where(i => !unavailableIndexes.Contains(i) && rowLocales[i].Name != account.Locale?.Name)
			.Select(i => rowLocales[i].Name)
			.ToList();

		DialogResult = DialogResult.OK;
		Close();
	}

	private void cancelBtn_Click(object sender, EventArgs e)
	{
		DialogResult = DialogResult.Cancel;
		Close();
	}
}
