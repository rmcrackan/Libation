using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Dinah.Core;
using LibationFileManager;
using System.Collections.Generic;
using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Data;
using System.IO;
using System.Reactive.Subjects;

namespace LibationAvalonia.AvaloniaUI.Controls
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

	public partial class DirectorySelectControl : UserControl
	{
		public static List<Configuration.KnownDirectories> DefaultKnownDirectories { get; }
			= new()
			{
				Configuration.KnownDirectories.WinTemp,
				Configuration.KnownDirectories.UserProfile,
				Configuration.KnownDirectories.AppDir,
				Configuration.KnownDirectories.MyDocs,
				Configuration.KnownDirectories.LibationFiles
			};

		public static readonly StyledProperty<Configuration.KnownDirectories> SelectedDirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, Configuration.KnownDirectories>(nameof(SelectedDirectory));

		public static readonly StyledProperty<List<Configuration.KnownDirectories>> KnownDirectoriesProperty =
		AvaloniaProperty.Register<DirectorySelectControl, List<Configuration.KnownDirectories>>(nameof(KnownDirectories), DefaultKnownDirectories);

		public static readonly StyledProperty<string> SubDirectoryProperty =
		AvaloniaProperty.Register<DirectorySelectControl, string>(nameof(SubDirectory));

		public DirectorySelectControl()
		{
			InitializeComponent();

			displayPathTbox = this.Get<TextBox>(nameof(displayPathTbox));
			displayPathTbox.Bind(TextBox.TextProperty, TextboxPath);
			PropertyChanged += DirectorySelectControl_PropertyChanged;
		}

		private Subject<string> TextboxPath = new Subject<string>();

		private void DirectorySelectControl_PropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
		{
			if (e.Property.Name == nameof(SelectedDirectory))
			{
				TextboxPath.OnNext(
					Path.Combine(
						SelectedDirectory is Configuration.KnownDirectories.None ? string.Empty
						: SelectedDirectory is Configuration.KnownDirectories.AppDir ? Configuration.AppDir_Absolute
						: Configuration.GetKnownDirectoryPath(SelectedDirectory)
						, SubDirectory ?? string.Empty));
			}
		}

		public List<Configuration.KnownDirectories> KnownDirectories
		{
			get => GetValue(KnownDirectoriesProperty);
			set => SetValue(KnownDirectoriesProperty, value);
		}

		public Configuration.KnownDirectories SelectedDirectory
		{
			get => GetValue(SelectedDirectoryProperty);
			set => SetValue(SelectedDirectoryProperty, value);
		}

		public string SubDirectory
		{
			get => GetValue(SubDirectoryProperty);
			set => SetValue(SubDirectoryProperty, value);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
