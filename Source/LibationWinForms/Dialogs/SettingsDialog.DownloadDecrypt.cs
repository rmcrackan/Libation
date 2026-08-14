using Dinah.Core;
using LibationFileManager;
using LibationFileManager.Templates;
using LibationUiBase;
using System;
using System.IO;
using System.Linq;

namespace LibationWinForms.Dialogs;

public partial class SettingsDialog
{
	private void folderTemplateBtn_Click(object sender, EventArgs e)
		=> editTemplate(TemplateEditor<Templates.FolderTemplate>.CreateFilenameEditor(config.Books?.Path ?? Path.GetTempPath(), folderTemplateTb.Text), folderTemplateTb);
	private void fileTemplateBtn_Click(object sender, EventArgs e)
		=> editTemplate(TemplateEditor<Templates.FileTemplate>.CreateFilenameEditor(config.Books?.Path ?? Path.GetTempPath(), fileTemplateTb.Text), fileTemplateTb);
	private void chapterFileTemplateBtn_Click(object sender, EventArgs e)
		=> editTemplate(TemplateEditor<Templates.ChapterFileTemplate>.CreateFilenameEditor(config.Books?.Path ?? Path.GetTempPath(), chapterFileTemplateTb.Text), chapterFileTemplateTb);

	private void editCharreplacementBtn_Click(object sender, EventArgs e)
	{
		var form = new EditReplacementChars(config);
		form.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		form.ShowDialog(this);
	}

	private void Load_DownloadDecrypt(Configuration config)
	{
		Load_DailyDownloadLimit(config);

		inProgressDescLbl.Text = desc(nameof(config.InProgress));
		editCharreplacementBtn.Text = desc(nameof(config.ReplacementCharacters));

		badBookGb.Text = desc(nameof(config.BadBook));
		badBookAskRb.Text = Configuration.BadBookAction.Ask.GetDescription();
		badBookAbortRb.Text = Configuration.BadBookAction.Abort.GetDescription();
		badBookRetryRb.Text = Configuration.BadBookAction.Retry.GetDescription();
		badBookIgnoreRb.Text = Configuration.BadBookAction.Ignore.GetDescription();
		useCoverAsFolderIconCb.Text = desc(nameof(config.UseCoverAsFolderIcon));
		saveMetadataToFileCbox.Text = desc(nameof(config.SaveMetadataToFile));

		inProgressSelectControl.SetDirectoryItems(new()
		{
			Configuration.KnownDirectories.WinTemp,
			Configuration.KnownDirectories.ApplicationData,
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs,
			Configuration.KnownDirectories.LibationFiles
		}, Configuration.KnownDirectories.WinTemp);
		inProgressSelectControl.SelectDirectory(config.InProgress);

		var rb = config.BadBook switch
		{
			Configuration.BadBookAction.Ask => this.badBookAskRb,
			Configuration.BadBookAction.Abort => this.badBookAbortRb,
			Configuration.BadBookAction.Retry => this.badBookRetryRb,
			Configuration.BadBookAction.Ignore => this.badBookIgnoreRb,
			_ => this.badBookAskRb
		};
		rb.Checked = true;

		folderTemplateLbl.Text = desc(nameof(config.FolderTemplate));
		fileTemplateLbl.Text = desc(nameof(config.FileTemplate));
		chapterFileTemplateLbl.Text = desc(nameof(config.ChapterFileTemplate));
		folderTemplateTb.Text = config.FolderTemplate;
		fileTemplateTb.Text = config.FileTemplate;
		chapterFileTemplateTb.Text = config.ChapterFileTemplate;
		useCoverAsFolderIconCb.Checked = config.UseCoverAsFolderIcon;
		saveMetadataToFileCbox.Checked = config.SaveMetadataToFile;
	}

	#region daily download limit

	private void Load_DailyDownloadLimit(Configuration config)
	{
		dailyDownloadLimitGb.Text = desc(nameof(config.DailyDownloadLimit));
		dailyDownloadLimitDescLbl.Text
			= "Rolling 24 hours, not a calendar day. Only downloads Libation completes are counted; books you download from the Audible app or website are not.";
		dailyDownloadLimitQtyLbl.Text = desc(nameof(config.DailyDownloadLimitQuantity));
		dailyDownloadLimitApproxLbl.Text
			= "MB and GB are approximate: Libation does not know precisely how large an audiobook is before downloading it, so it assumes about 400 MB per book.";

		var scopeTip = Configuration.GetHelpText(nameof(config.DailyDownloadLimit));
		toolTip.SetToolTip(dailyDownloadLimitScopeCb, scopeTip);
		toolTip.SetToolTip(dailyDownloadLimitDescLbl, scopeTip);
		toolTip.SetToolTip(dailyDownloadLimitUnitCb, Configuration.GetHelpText(nameof(config.DailyDownloadLimitUnit)));

		dailyDownloadLimitScopeCb.Items.Clear();
		dailyDownloadLimitScopeCb.Items.AddRange(
			Enum.GetValues<Configuration.DailyLimitScope>()
			.Select(v => (object)new EnumDisplay<Configuration.DailyLimitScope>(v))
			.ToArray());
		dailyDownloadLimitScopeCb.SelectedIndex
			= Array.IndexOf(Enum.GetValues<Configuration.DailyLimitScope>(), config.DailyDownloadLimit);

		dailyDownloadLimitUnitCb.Items.Clear();
		dailyDownloadLimitUnitCb.Items.AddRange(
			Enum.GetValues<Configuration.DailyLimitUnit>()
			.Select(v => (object)new EnumDisplay<Configuration.DailyLimitUnit>(v))
			.ToArray());
		dailyDownloadLimitUnitCb.SelectedIndex
			= Array.IndexOf(Enum.GetValues<Configuration.DailyLimitUnit>(), config.DailyDownloadLimitUnit);

		dailyDownloadLimitQtyNud.Value = Math.Clamp(config.DailyDownloadLimitQuantity, dailyDownloadLimitQtyNud.Minimum, dailyDownloadLimitQtyNud.Maximum);

		UpdateDailyDownloadLimitEnabled();
	}

	private void dailyDownloadLimitScopeCb_SelectedIndexChanged(object sender, EventArgs e) => UpdateDailyDownloadLimitEnabled();

	private void dailyDownloadLimitUnitCb_SelectedIndexChanged(object sender, EventArgs e) => UpdateDailyDownloadLimitEnabled();

	/// <summary>The quantity and unit only mean anything once a scope other than "No limit" is chosen.</summary>
	private void UpdateDailyDownloadLimitEnabled()
	{
		var limited = SelectedDailyLimitScope() is not Configuration.DailyLimitScope.NoLimit;

		dailyDownloadLimitQtyLbl.Visible = limited;
		dailyDownloadLimitQtyNud.Visible = limited;
		dailyDownloadLimitUnitCb.Visible = limited;
		dailyDownloadLimitApproxLbl.Visible = limited && SelectedDailyLimitUnit() is not Configuration.DailyLimitUnit.Books;
	}

	private Configuration.DailyLimitScope SelectedDailyLimitScope()
		=> dailyDownloadLimitScopeCb.SelectedItem is EnumDisplay<Configuration.DailyLimitScope> selected
		? selected.Value
		: Configuration.DailyLimitScope.NoLimit;

	private Configuration.DailyLimitUnit SelectedDailyLimitUnit()
		=> dailyDownloadLimitUnitCb.SelectedItem is EnumDisplay<Configuration.DailyLimitUnit> selected
		? selected.Value
		: Configuration.DailyLimitUnit.Books;

	private void Save_DailyDownloadLimit(Configuration config)
	{
		config.DailyDownloadLimit = SelectedDailyLimitScope();
		config.DailyDownloadLimitQuantity = (int)dailyDownloadLimitQtyNud.Value;
		config.DailyDownloadLimitUnit = SelectedDailyLimitUnit();
	}

	#endregion

	private void Save_DownloadDecrypt(Configuration config)
	{
		Save_DailyDownloadLimit(config);

		config.InProgress = inProgressSelectControl.SelectedDirectory;

		config.BadBook
			= badBookAskRb.Checked ? Configuration.BadBookAction.Ask
			: badBookAbortRb.Checked ? Configuration.BadBookAction.Abort
			: badBookRetryRb.Checked ? Configuration.BadBookAction.Retry
			: badBookIgnoreRb.Checked ? Configuration.BadBookAction.Ignore
			: Configuration.BadBookAction.Ask;

		config.FolderTemplate = folderTemplateTb.Text;
		config.FileTemplate = fileTemplateTb.Text;
		config.ChapterFileTemplate = chapterFileTemplateTb.Text;
		config.UseCoverAsFolderIcon = useCoverAsFolderIconCb.Checked;
		config.SaveMetadataToFile = saveMetadataToFileCbox.Checked;
	}
}
