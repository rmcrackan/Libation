using System;
using System.IO;
using Dinah.Core;
using LibationFileManager;
using LibationFileManager.Templates;

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

	private void Save_DownloadDecrypt(Configuration config)
	{
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
