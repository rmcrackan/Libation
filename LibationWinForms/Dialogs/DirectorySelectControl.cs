using Dinah.Core;
using FileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class DirectorySelectControl : UserControl
	{
		private class DirectoryComboBoxItem
		{
			public string Description { get; }
			public Configuration.KnownDirectories Value { get; }
			private readonly DirectorySelectControl _parentControl;

			public string FullPath => _parentControl.AddSubDirectoryToPath(Configuration.GetKnownDirectoryPath(Value));

			/// <summary>Displaying relative paths is confusing. UI should display absolute equivalent</summary>
			public string UiDisplayPath => Value == Configuration.KnownDirectories.AppDir ? _parentControl.AddSubDirectoryToPath(Configuration.AppDir_Absolute) : FullPath;

			public DirectoryComboBoxItem(DirectorySelectControl parentControl, Configuration.KnownDirectories knownDirectory)
			{
				_parentControl = parentControl;

				Value = knownDirectory;
				Description = Value.GetDescription();
			}

			public override string ToString() => Description;
		}

		public string SelectedDirectory => selectedItem?.FullPath;

		private string _subDirectory;
		internal string AddSubDirectoryToPath(string path) => string.IsNullOrWhiteSpace(_subDirectory) ? path : System.IO.Path.Combine(path, _subDirectory);
		internal string RemoveSubDirectoryFromPath(string path)
		{
			if (string.IsNullOrWhiteSpace(_subDirectory))
				return path;

			path = path?.Trim() ?? "";
			if (string.IsNullOrWhiteSpace(path))
				return path;

			var bottomDir = System.IO.Path.GetFileName(path);
			if (_subDirectory.EqualsInsensitive(bottomDir))
				return System.IO.Path.GetDirectoryName(path);

			return path;
		}

		private DirectoryComboBoxItem selectedItem => (DirectoryComboBoxItem)this.directoryComboBox.SelectedItem;

		public DirectorySelectControl() => InitializeComponent();

		/// <summary>Set items for combobox</summary>
		/// <param name="knownDirectories">List rather than IEnumerable so that client can determine display order</param>
		/// <param name="defaultDirectory">Optional default item to select</param>
		public void SetDirectoryItems(List<Configuration.KnownDirectories> knownDirectories, Configuration.KnownDirectories? defaultDirectory = null, string subDirectory = null)
		{
			// set this 1st so all DirectoryComboBoxItems can reference it
			_subDirectory = subDirectory;

			this.directoryComboBox.Items.Clear();

			foreach (var dir in knownDirectories.Where(d => d != Configuration.KnownDirectories.None).Distinct())
				this.directoryComboBox.Items.Add(new DirectoryComboBoxItem(this, dir));

			SelectDirectory(defaultDirectory);
		}

		/// <summary>set selection</summary>
		/// <param name="directory"></param>
		/// <returns>True is there was a matching entry</returns>
		public bool SelectDirectory(string directory)
		{
			directory = directory?.Trim() ?? "";

			var noSubDir = RemoveSubDirectoryFromPath(directory);
			var knownDir = Configuration.GetKnownDirectory(noSubDir);
			return SelectDirectory(knownDir);
		}

		/// <summary>set selection</summary>
		/// <param name="directory"></param>
		/// <returns>True is there was a matching entry</returns>
		public bool SelectDirectory(Configuration.KnownDirectories? directory)
		{
			if (directory is null || directory == Configuration.KnownDirectories.None)
				return false;

			// set default
			var item = this.directoryComboBox.Items.Cast<DirectoryComboBoxItem>().SingleOrDefault(item => item.Value == directory.Value);
			if (item is null)
				return false;

			this.directoryComboBox.SelectedItem = item;
			return true;
		}

		private void DirectorySelectControl_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

		}

		private void directoryComboBox_SelectedIndexChanged(object sender, EventArgs e) => this.textBox1.Text = selectedItem.UiDisplayPath;
	}
}
