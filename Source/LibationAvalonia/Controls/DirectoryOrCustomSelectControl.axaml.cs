using Avalonia;
using Avalonia.Controls;
using Dinah.Core;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Controls
{
	public partial class DirectoryOrCustomSelectControl : UserControl
	{
		public static readonly StyledProperty<List<Configuration.KnownDirectories>> KnownDirectoriesProperty =
		AvaloniaProperty.Register<DirectorySelectControl, List<Configuration.KnownDirectories>>(nameof(KnownDirectories), DirectorySelectControl.DefaultKnownDirectories);

		public static readonly StyledProperty<string> SubDirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, string>(nameof(SubDirectory));

		public static readonly StyledProperty<string> DirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, string>(nameof(Directory));

		public List<Configuration.KnownDirectories> KnownDirectories
		{
			get => GetValue(KnownDirectoriesProperty);
			set => SetValue(KnownDirectoriesProperty, value);
		}

		public string Directory
		{
			get => GetValue(DirectoryProperty);
			set => SetValue(DirectoryProperty, value);
		}

		public string SubDirectory
		{
			get => GetValue(SubDirectoryProperty);
			set => SetValue(SubDirectoryProperty, value);
		}

		private readonly DirectoryState directoryState = new();

		public DirectoryOrCustomSelectControl()
		{
			InitializeComponent();

			grid.DataContext = directoryState;

			directoryState.PropertyChanged += DirectoryState_PropertyChanged;
			PropertyChanged += DirectoryOrCustomSelectControl_PropertyChanged;
		}

		private void DirectoryState_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName is nameof(DirectoryState.SelectedDirectory) or nameof(DirectoryState.KnownChecked) &&
				directoryState.KnownChecked &&
				directoryState.SelectedDirectory is Configuration.KnownDirectories kdir &&
				kdir is not Configuration.KnownDirectories.None)
			{
				Directory = kdir is Configuration.KnownDirectories.AppDir ? Configuration.AppDir_Absolute : Configuration.GetKnownDirectoryPath(kdir);
			}
			else if (e.PropertyName is nameof(DirectoryState.CustomDir) or nameof(DirectoryState.CustomChecked) &&
				directoryState.CustomChecked &&
				directoryState.CustomDir is not null)
			{
				Directory = directoryState.CustomDir;
			}
		}

		private class DirectoryState : ViewModels.ViewModelBase
		{
			private string _customDir;
			private string _subDirectory;
			private bool _knownChecked;
			private bool _customChecked;
			private Configuration.KnownDirectories? _selectedDirectory;
			public string CustomDir { get => _customDir; set => this.RaiseAndSetIfChanged(ref _customDir, value); }
			public string SubDirectory { get => _subDirectory; set => this.RaiseAndSetIfChanged(ref _subDirectory, value); }
			public bool KnownChecked { get => _knownChecked; set => this.RaiseAndSetIfChanged(ref _knownChecked, value); }
			public bool CustomChecked { get => _customChecked; set => this.RaiseAndSetIfChanged(ref _customChecked, value); }

			public Configuration.KnownDirectories? SelectedDirectory { get => _selectedDirectory; set => this.RaiseAndSetIfChanged(ref _selectedDirectory, value); }
		}

		private async void CustomDirBrowseBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var options = new Avalonia.Platform.Storage.FolderPickerOpenOptions
			{
				AllowMultiple = false
			};

			var selectedFolders = await (VisualRoot as Window).StorageProvider.OpenFolderPickerAsync(options);

			directoryState.CustomDir = selectedFolders.SingleOrDefault()?.Path?.LocalPath ?? directoryState.CustomDir;
		}

		private void DirectoryOrCustomSelectControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property == DirectoryProperty)
			{
				var directory = Directory?.Trim() ?? "";

				var noSubDir = RemoveSubDirectoryFromPath(directory);
				var known = Configuration.GetKnownDirectory(noSubDir);

				if (known == Configuration.KnownDirectories.None && noSubDir == Configuration.AppDir_Absolute)
					known = Configuration.KnownDirectories.AppDir;

				if (known is Configuration.KnownDirectories.None)
				{
					directoryState.CustomDir = noSubDir;
					directoryState.CustomChecked = true;
				}
				else
				{
					directoryState.SelectedDirectory = known;
					directoryState.KnownChecked = true;
				}
			}
			else if (e.Property == KnownDirectoriesProperty &&
					KnownDirectories.Count > 0 &&
					directoryState.SelectedDirectory is null or Configuration.KnownDirectories.None)
				directoryState.SelectedDirectory = KnownDirectories[0];
		}

		private string RemoveSubDirectoryFromPath(string path)
		{
			if (string.IsNullOrWhiteSpace(SubDirectory))
				return path;

			path = path?.Trim() ?? "";
			if (string.IsNullOrWhiteSpace(path))
				return path;

			var bottomDir = System.IO.Path.GetFileName(path);
			if (SubDirectory.EqualsInsensitive(bottomDir))
				return System.IO.Path.GetDirectoryName(path);

			return path;
		}
	}
}
