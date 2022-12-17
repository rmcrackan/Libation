using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dinah.Core;
using LibationFileManager;
using System.Collections.Generic;
using ReactiveUI;
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
		CustomState customStates = new();

		public DirectoryOrCustomSelectControl()
		{
			InitializeComponent();

			customDirBrowseBtn = this.Find<Button>(nameof(customDirBrowseBtn));
			directorySelectControl = this.Find<DirectorySelectControl>(nameof(directorySelectControl));

			this.Find<TextBox>(nameof(customDirTbox)).DataContext = customStates;
			this.Find<RadioButton>(nameof(knownDirRadio)).DataContext = customStates;
			this.Find<RadioButton>(nameof(customDirRadio)).DataContext = customStates;

			customStates.PropertyChanged += CheckStates_PropertyChanged;
			customDirBrowseBtn.Click += CustomDirBrowseBtn_Click;
			PropertyChanged += DirectoryOrCustomSelectControl_PropertyChanged;
			directorySelectControl.PropertyChanged += DirectorySelectControl_PropertyChanged;
		}

		private class CustomState: ViewModels.ViewModelBase
		{
			private string _customDir;
			private bool _knownChecked;
			private bool _customChecked;
			public string CustomDir { get=> _customDir; set => this.RaiseAndSetIfChanged(ref _customDir, value); }
			public bool KnownChecked
			{
				get => _knownChecked;
				set
				{
					this.RaiseAndSetIfChanged(ref _knownChecked, value);
					if (value)
						CustomChecked = false;
					else if (!CustomChecked)
						CustomChecked = true;
				}
			}
			public bool CustomChecked
			{
				get => _customChecked;
				set
				{
					this.RaiseAndSetIfChanged(ref _customChecked, value);
					if (value)
						KnownChecked = false;
					else if (!KnownChecked)
						KnownChecked = true;
				}
			}
		}

		private async void CustomDirBrowseBtn_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			var options = new Avalonia.Platform.Storage.FolderPickerOpenOptions
			{
				AllowMultiple = false
			};

			var selectedFolders = await (VisualRoot as Window).StorageProvider.OpenFolderPickerAsync(options);

			customStates.CustomDir =
				selectedFolders
				.SingleOrDefault()?.
				TryGetUri(out var uri) is true
				? uri.LocalPath
				: customStates.CustomDir;
		}

		private void CheckStates_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(CustomState.CustomDir))
			{
				directorySelectControl.IsEnabled = !customStates.CustomChecked;
				customDirBrowseBtn.IsEnabled = customStates.CustomChecked;
			}

			setDirectory();
		}


		private void DirectorySelectControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property.Name == nameof(DirectorySelectControl.SelectedDirectory))
			{
				setDirectory();
			}
		}

		private void setDirectory()
		{
			var selectedDir
				= customStates.CustomChecked ? customStates.CustomDir
					: directorySelectControl.SelectedDirectory is Configuration.KnownDirectories.AppDir ? Configuration.AppDir_Absolute
					: Configuration.GetKnownDirectoryPath(directorySelectControl.SelectedDirectory);
			selectedDir ??= string.Empty;

			Directory = customStates.CustomChecked ? selectedDir : System.IO.Path.Combine(selectedDir, SubDirectory);
		}

		private void DirectoryOrCustomSelectControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property.Name == nameof(Directory) && e.OldValue is null)
			{
				var directory = Directory?.Trim() ?? "";

				var noSubDir = RemoveSubDirectoryFromPath(directory);
				var known = Configuration.GetKnownDirectory(noSubDir);

				if (known == Configuration.KnownDirectories.None && noSubDir == Configuration.AppDir_Absolute)
					known = Configuration.KnownDirectories.AppDir;				

				if (known is Configuration.KnownDirectories.None)
				{
					customStates.CustomChecked = true;
					customStates.CustomDir = directory;
				}
				else
				{
					customStates.KnownChecked = true;
					directorySelectControl.SelectedDirectory = known;
				}
			}
			else if (e.Property.Name == nameof(KnownDirectories))
				directorySelectControl.KnownDirectories = KnownDirectories;
			else if (e.Property.Name == nameof(SubDirectory))
				directorySelectControl.SubDirectory = SubDirectory;
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

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
