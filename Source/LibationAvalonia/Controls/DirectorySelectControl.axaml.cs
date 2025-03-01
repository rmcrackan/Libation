using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Dinah.Core;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace LibationAvalonia.Controls
{
	public class KnownDirectoryConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Configuration.KnownDirectories dir)
				return dir.GetDescription();
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}
	}
	public class KnownDirectoryPath : IMultiValueConverter
	{
		public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
		{
			if (values?.Count == 2 && values[0] is Configuration.KnownDirectories kdir && kdir is not Configuration.KnownDirectories.None)
			{
				var subdir = values[1] as string ?? "";
				var path = kdir is Configuration.KnownDirectories.AppDir ? Configuration.AppDir_Absolute : Configuration.GetKnownDirectoryPath(kdir);
				return Path.Combine(path, subdir);
			}
			return "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
		}
	}

	public partial class DirectorySelectControl : UserControl
	{
		public static List<Configuration.KnownDirectories> DefaultKnownDirectories { get; }
			= new()
			{
				Configuration.KnownDirectories.WinTemp,
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.ApplicationData,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyMusic,
				Configuration.KnownDirectories.MyDocs,
				Configuration.KnownDirectories.LibationFiles
			};

		public static readonly StyledProperty<Configuration.KnownDirectories?> SelectedDirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, Configuration.KnownDirectories?>(nameof(SelectedDirectory));

		public static readonly StyledProperty<List<Configuration.KnownDirectories>> KnownDirectoriesProperty =
		AvaloniaProperty.Register<DirectorySelectControl, List<Configuration.KnownDirectories>>(nameof(KnownDirectories), DefaultKnownDirectories);

		public static readonly StyledProperty<string> SubDirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, string>(nameof(SubDirectory));

		public DirectorySelectControl()
		{
			InitializeComponent();
		}

		public List<Configuration.KnownDirectories> KnownDirectories
		{
			get => GetValue(KnownDirectoriesProperty);
			set => SetValue(KnownDirectoriesProperty, value);
		}

		public Configuration.KnownDirectories? SelectedDirectory
		{
			get => GetValue(SelectedDirectoryProperty);
			set => SetValue(SelectedDirectoryProperty, value);
		}

		public string SubDirectory
		{
			get => GetValue(SubDirectoryProperty);
			set => SetValue(SubDirectoryProperty, value);
		}
	}
}
