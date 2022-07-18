using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dinah.Core;
using LibationFileManager;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using Avalonia.Controls.Primitives;
using System.Collections;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Data;

namespace LibationWinForms.AvaloniaUI.Controls
{
	public class TextCaseConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Configuration.KnownDirectories dir)
			{

			}
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
	public partial class DirectorySelectControl : TemplatedControl
	{
		private static readonly List<Configuration.KnownDirectories> defaultList = new List<Configuration.KnownDirectories>()
		{
					Configuration.KnownDirectories.WinTemp,
					Configuration.KnownDirectories.UserProfile,
					Configuration.KnownDirectories.AppDir,
					Configuration.KnownDirectories.MyDocs,
					Configuration.KnownDirectories.LibationFiles
			};
	public static readonly StyledProperty<Configuration.KnownDirectories?> SelectedirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, Configuration.KnownDirectories?>(nameof(Selectedirectory), defaultList[0]);

		public static readonly StyledProperty<List<Configuration.KnownDirectories>> KnownDirectoriesProperty =
		AvaloniaProperty.Register<DirectorySelectControl, List<Configuration.KnownDirectories>>(nameof(KnownDirectories), defaultList);

		public static readonly StyledProperty<string?> SubdirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, string?>(nameof(Subdirectory), "subdir");

		DirectorySelectViewModel DirectorySelect { get; } = new();
		public DirectorySelectControl()
		{
			InitializeComponent();
		}

		protected override void OnInitialized()
		{
			DirectorySelect.Directories.Clear();

			int insertIndex = 0;
			foreach (var kd in KnownDirectories.Distinct())
				DirectorySelect.Directories.Insert(insertIndex++, new(this, kd));

			DataContext = DirectorySelect;
			base.OnInitialized();
		}

		public List<Configuration.KnownDirectories> KnownDirectories
		{
			get { return GetValue(KnownDirectoriesProperty); }
			set 
			{
				SetValue(KnownDirectoriesProperty, value);
				//SetDirectoryItems(KnownDirectories);
			}
		}


		public Configuration.KnownDirectories? Selectedirectory
		{
			get { return GetValue(SelectedirectoryProperty); }
			set 
			{
				SetValue(SelectedirectoryProperty, value);

				if (value is null or Configuration.KnownDirectories.None)
					return;

				// set default
				var item = DirectorySelect.Directories.SingleOrDefault(item => item.Value == value.Value);
				if (item is null)
					return;

				DirectorySelect.SelectedDirectory = item;
			}
		}


		public string? Subdirectory
		{
			get { return GetValue(SubdirectoryProperty); }
			set 
			{
				SetValue(SubdirectoryProperty, value);
			}
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}

	public class DirectorySelectViewModel : ViewModels.ViewModelBase
	{
		public class DirectoryComboBoxItem
		{
			private readonly DirectorySelectControl _parentControl;
			public string Description { get; }
			public Configuration.KnownDirectories Value { get; }

			public string FullPath => AddSubDirectoryToPath(Configuration.GetKnownDirectoryPath(Value));

			/// <summary>Displaying relative paths is confusing. UI should display absolute equivalent</summary>
			public string UiDisplayPath => Value == Configuration.KnownDirectories.AppDir ? AddSubDirectoryToPath(Configuration.AppDir_Absolute) : FullPath;

			public DirectoryComboBoxItem(DirectorySelectControl parentControl, Configuration.KnownDirectories knownDirectory)
			{
				_parentControl = parentControl;
				Value = knownDirectory;
				Description = Value.GetDescription();
			}

			internal string AddSubDirectoryToPath(string path) => string.IsNullOrWhiteSpace(_parentControl.Subdirectory) ? path : System.IO.Path.Combine(path, _parentControl.Subdirectory);

			public override string ToString() => Description;
		}
		public ObservableCollection<DirectoryComboBoxItem> Directories { get; } = new(new());
		private DirectoryComboBoxItem _selectedDirectory;
		public DirectoryComboBoxItem SelectedDirectory { get => _selectedDirectory; set => this.RaiseAndSetIfChanged(ref _selectedDirectory, value); }
	}
}
