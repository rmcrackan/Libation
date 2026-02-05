using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Dinah.Core;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LibationAvalonia.Controls;

public partial class DirectoryOrCustomSelectControl : UserControl
{
	public static readonly StyledProperty<IList<Configuration.KnownDirectories>?> KnownDirectoriesProperty =
	AvaloniaProperty.Register<DirectoryOrCustomSelectControl, IList<Configuration.KnownDirectories>?>(nameof(KnownDirectories), DefaultKnownDirectories);

	public static readonly StyledProperty<string?> SubDirectoryProperty =
	AvaloniaProperty.Register<DirectoryOrCustomSelectControl, string?>(nameof(SubDirectory));

	public static readonly StyledProperty<string?> DirectoryProperty =
	AvaloniaProperty.Register<DirectoryOrCustomSelectControl, string?>(nameof(Directory));

	public IList<Configuration.KnownDirectories>? KnownDirectories
	{
		get => GetValue(KnownDirectoriesProperty);
		set => SetValue(KnownDirectoriesProperty, value);
	}

	public string? Directory
	{
		get => GetValue(DirectoryProperty);
		set => SetValue(DirectoryProperty, value);
	}

	public string? SubDirectory
	{
		get => GetValue(SubDirectoryProperty);
		set => SetValue(SubDirectoryProperty, value);
	}

	public static IList<Configuration.KnownDirectories> DefaultKnownDirectories => [
		Configuration.KnownDirectories.WinTemp,
		Configuration.KnownDirectories.UserProfile,
		Configuration.KnownDirectories.ApplicationData,
		Configuration.KnownDirectories.AppDir,
		Configuration.KnownDirectories.MyMusic,
		Configuration.KnownDirectories.MyDocs,
		Configuration.KnownDirectories.LibationFiles];

	private readonly AvaloniaList<KnownDirectoryItem> _knownDirNames;
	public DirectoryOrCustomSelectControl()
	{
		InitializeComponent();
		_knownDirNames = new(GetKnownDirectories(DefaultKnownDirectories));
		cmbKnownDirs.ItemsSource = _knownDirNames;
		cmbKnownDirs.SelectionChanged += CmbKnownDirs_SelectionChanged;
		btnBrowse.Click += Browse_Click;
	}

	private void CmbKnownDirs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (cmbKnownDirs.SelectedItem is KnownDirectoryItem item && item.Directory is not null)
		{
			Directory = item.Directory;
		}
	}

	private IEnumerable<KnownDirectoryItem> GetKnownDirectories(IEnumerable<Configuration.KnownDirectories> knownDirs)
		=> knownDirs.Select(k => new KnownDirectoryItem(k, SubDirectory)).Where(k => k.Directory is not null);

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (change.Property == SubDirectoryProperty)
		{
			foreach (var item in _knownDirNames)
			{
				item.SubDirectory = SubDirectory;
			}
			VerifyAndApplyDirectory(Directory);
		}
		else if (change.Property == KnownDirectoriesProperty)
		{
			var knownDirs = KnownDirectories?.Count > 0 ? KnownDirectories : DefaultKnownDirectories;
			if (!_knownDirNames.Select(k => k.KnownDirectory).SequenceEqual(knownDirs))
			{
				_knownDirNames.Clear();
				_knownDirNames.AddRange(GetKnownDirectories(knownDirs));
			}
			VerifyAndApplyDirectory(Directory);
		}
		else if (change.Property == DirectoryProperty)
		{
			VerifyAndApplyDirectory(Directory);
		}

		base.OnPropertyChanged(change);
	}

	private void VerifyAndApplyDirectory(string? directory)
	{
		if (string.IsNullOrWhiteSpace(Directory))
			return;

		bool dirIsKnown = false;
		foreach (var item in _knownDirNames)
		{
			if (item.IsSamePathAs(directory))
			{
				rbKnown.IsChecked = true;
				Directory = item.Directory;
				cmbKnownDirs.SelectedItem = item;
				dirIsKnown = true;
				break;
			}
		}
		if (!dirIsKnown)
		{
			tboxCustomDirPath.Text = directory;
			rbCustom.IsChecked = true;
		}
	}

	public async void Browse_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (VisualRoot is not Window window)
			return;

		var options = new FolderPickerOpenOptions
		{
			AllowMultiple = false
		};

		var selectedFolders = await window.StorageProvider.OpenFolderPickerAsync(options);
		Directory = selectedFolders.SingleOrDefault()?.TryGetLocalPath() ?? Directory;
	}

	private class KnownDirectoryItem : ReactiveObject
	{
		public Configuration.KnownDirectories KnownDirectory { get; set; }
		public string? Directory { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
		public string? Name { get; }
		public string? SubDirectory
		{
			get => field;
			set
			{
				field = value;
				if (Configuration.GetKnownDirectoryPath(KnownDirectory) is string dir)
				{
					Directory = Path.Combine(dir, field ?? "");
				}
			}
		}

		public KnownDirectoryItem(Configuration.KnownDirectories known, string? subDir)
		{
			Name = known.GetDescription();
			KnownDirectory = known;
			SubDirectory = subDir;
		}

		public bool IsSamePathAs(string? otherPath)
		{
			if (string.IsNullOrWhiteSpace(otherPath) || string.IsNullOrWhiteSpace(Directory))
				return false;

			try
			{
				var p1 = Path.GetFullPath(Directory);
				var p2 = Path.GetFullPath(otherPath);
				return p1.Equals(p2, System.StringComparison.OrdinalIgnoreCase);
			}
			catch { return false; }
		}

		public override string? ToString() => Name?.ToString();
	}
}
