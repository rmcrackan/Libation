using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using LibationFileManager;

namespace LibationWinForms.Dialogs
{
	public partial class DirectoryOrCustomSelectControl : UserControl
	{
		public bool SelectedDirectoryIsKnown => knownDirectoryRb.Checked;
		public bool SelectedDirectoryIsCustom => customDirectoryRb.Checked;
		public string SelectedDirectory
			=> SelectedDirectoryIsKnown ? directorySelectControl.SelectedDirectory
			: SelectedDirectoryIsCustom ? customTb.Text.Trim()
			: null;

		public DirectoryOrCustomSelectControl()
		{
			InitializeComponent();

			// doing this after InitializeComponent will fire event
			this.knownDirectoryRb.Checked = true;
		}

		/// <summary>Set items for combobox</summary>
		/// <param name="knownDirectories">List rather than IEnumerable so that client can determine display order</param>
		/// <param name="defaultDirectory"></param>
		public void SetDirectoryItems(List<Configuration.KnownDirectories> knownDirectories, Configuration.KnownDirectories? defaultDirectory = Configuration.KnownDirectories.UserProfile, string subDirectory = null)
			=> this.directorySelectControl.SetDirectoryItems(knownDirectories, defaultDirectory, subDirectory);

		/// <summary>set selection</summary>
		/// <param name="directory"></param>
		public void SelectDirectory(Configuration.KnownDirectories directory)
		{
			// if None: take no action
			if (directory != Configuration.KnownDirectories.None)
				selectDir(directory, null);
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			//Workaround for anchoring bug in user controls
			//https://github.com/dotnet/winforms/issues/6381
			customBtn.Location = new System.Drawing.Point(Width - customBtn.Width, customTb.Location.Y);
			customBtn.Height = customTb.Height;
			directorySelectControl.Width = Width - directorySelectControl.Location.X;
			customTb.Width = Width - customTb.Location.X - customBtn.Width - customTb.Margin.Left;
		}

		/// <summary>set selection</summary>
		public void SelectDirectory(string directory)
		{
			directory = directory?.Trim() ?? "";

			// remove SubDirectory setting to find known directories
			var noSubDir = this.directorySelectControl.RemoveSubDirectoryFromPath(directory);
			var knownDir = Configuration.GetKnownDirectory(noSubDir);
			// DO NOT remove SubDirectory setting for custom
			var customDir = directory;
			selectDir(knownDir, customDir);
		}

		private void selectDir(Configuration.KnownDirectories knownDir, string customDir)
		{
			var isKnown
				= knownDir != Configuration.KnownDirectories.None
				// this could be a well known dir which isn't an option in this particular dropdown. This will always be true of LibationFiles
				&& this.directorySelectControl.SelectDirectory(knownDir);

			customDirectoryRb.Checked = !isKnown;
			knownDirectoryRb.Checked = isKnown;
			this.customTb.Text = isKnown ? "" : customDir;
		}

		private string dirSearchTitle;
		public void SetSearchTitle(string dirSearchTitle) => this.dirSearchTitle = dirSearchTitle?.Trim();

		private void customBtn_Click(object sender, EventArgs e)
		{
			using var dialog = new FolderBrowserDialog
			{
				Description = string.IsNullOrWhiteSpace(dirSearchTitle) ? "Search" : $"Search for {dirSearchTitle}",
				SelectedPath = this.customTb.Text
			};
			dialog.ShowDialog();
			if (!string.IsNullOrWhiteSpace(dialog.SelectedPath))
				this.customTb.Text = dialog.SelectedPath;
		}

		private void radioButton_CheckedChanged(object sender, EventArgs e)
		{
			var isCustom = this.customDirectoryRb.Checked;

			customTb.Enabled = isCustom;
			customBtn.Enabled = isCustom;

			directorySelectControl.Enabled = !isCustom;
		}

		private void DirectoryOrCustomSelectControl_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

		}
	}
}
