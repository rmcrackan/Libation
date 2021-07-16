using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Dinah.Core;
using FileManager;

namespace LibationWinForms.Dialogs
{
	public partial class DirectorySelectControl : UserControl
	{
		private class DirectoryComboBoxItem
		{
			public string Description { get; }
			public Configuration.KnownDirectories Value { get; }

			public string FullPath => Configuration.GetKnownDirectoryPath(Value);

			/// <summary>Displaying relative paths is confusing. UI should display absolute equivalent</summary>
			public string UiDisplayPath => Value == Configuration.KnownDirectories.AppDir ? Configuration.AppDir_Absolute : FullPath;

			public DirectoryComboBoxItem(Configuration.KnownDirectories knownDirectory)
			{
				Value = knownDirectory;
				Description = Value.GetDescription();
			}

			public override string ToString() => Description;
		}

		private DirectoryComboBoxItem selectedItem => (DirectoryComboBoxItem)this.directoryComboBox.SelectedItem;
		public string SelectedDirectory => selectedItem?.FullPath;

		public DirectorySelectControl() => InitializeComponent();

		/// <summary>Set items for combobox</summary>
		/// <param name="knownDirectories">List rather than IEnumerable so that client can determine display order</param>
		/// <param name="defaultDirectory">Optional default item to select</param>
		public void SetDirectoryItems(List<Configuration.KnownDirectories> knownDirectories, Configuration.KnownDirectories? defaultDirectory = Configuration.KnownDirectories.UserProfile)
		{
			this.directoryComboBox.Items.Clear();

			foreach (var dir in knownDirectories.Where(d => d != Configuration.KnownDirectories.None).Distinct())
				this.directoryComboBox.Items.Add(new DirectoryComboBoxItem(dir));

			SelectDirectory(defaultDirectory);
		}

		/// <summary>select, set default, or rehydrate</summary>
		/// <param name="directory"></param>
		/// <returns>True is there was a matching entry</returns>
		public bool SelectDirectory(string directory) => SelectDirectory(Configuration.GetKnownDirectory(directory));

		/// <summary>select, set default, or rehydrate</summary>
		/// <param name="directory"></param>
		/// <returns>True is there was a matching entry</returns>
		public bool SelectDirectory(Configuration.KnownDirectories? directory)
		{
			if (directory is null || directory == Configuration.KnownDirectories.None)
			{
				this.directoryComboBox.SelectedIndex = 0;
				return false;
			}

			// set default
			var item = this.directoryComboBox.Items.Cast<DirectoryComboBoxItem>().SingleOrDefault(item => item.Value == directory.Value);
			if (item is null)
			{
				this.directoryComboBox.SelectedIndex = 0;
				return false;
			}

			this.directoryComboBox.SelectedItem = item;
			return true;
		}

		private void DirectorySelectControl_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

		}

		private void directoryComboBox_SelectedIndexChanged(object sender, EventArgs e) => this.label1.Text = selectedItem.UiDisplayPath;
	}
}
