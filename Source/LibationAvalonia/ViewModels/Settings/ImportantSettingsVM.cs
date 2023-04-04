using Avalonia.Controls.Shapes;
using FileManager;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Collections.Generic;

namespace LibationAvalonia.ViewModels.Settings
{
	public class ImportantSettingsVM : ViewModelBase, ISettingsDisplay
	{
		private string themeVariant;
		private string initialThemeVariant;

		public ImportantSettingsVM(Configuration config)
		{
			LoadSettings(config);
		}

		public void LoadSettings(Configuration config)
		{
			BooksDirectory = config.Books.PathWithoutPrefix;
			SavePodcastsToParentFolder = config.SavePodcastsToParentFolder;
			LoggingLevel = config.LogLevel;
			ThemeVariant = initialThemeVariant
				= Configuration.Instance.GetString(propertyName: nameof(ThemeVariant)) is nameof(Avalonia.Styling.ThemeVariant.Dark)
				? nameof(Avalonia.Styling.ThemeVariant.Dark)
				: nameof(Avalonia.Styling.ThemeVariant.Light);
		}

		public void SaveSettings(Configuration config)
		{
			LongPath lonNewBooks = Configuration.GetKnownDirectory(BooksDirectory) is Configuration.KnownDirectories.None ? BooksDirectory : System.IO.Path.Combine(BooksDirectory, "Books");
			if (!System.IO.Directory.Exists(lonNewBooks))
				System.IO.Directory.CreateDirectory(lonNewBooks);
			config.Books = lonNewBooks;
			config.SavePodcastsToParentFolder = SavePodcastsToParentFolder;
			config.LogLevel = LoggingLevel;
			Configuration.Instance.SetString(ThemeVariant, nameof(ThemeVariant));
		}

		public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
		{
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs
		};

		public string BooksText { get; } = Configuration.GetDescription(nameof(Configuration.Books));
		public string SavePodcastsToParentFolderText { get; } = Configuration.GetDescription(nameof(Configuration.SavePodcastsToParentFolder));
		public Serilog.Events.LogEventLevel[] LoggingLevels { get; } = Enum.GetValues<Serilog.Events.LogEventLevel>();
		public string BetaOptInText { get; } = Configuration.GetDescription(nameof(Configuration.BetaOptIn));
		public string[] Themes { get; } = { nameof(Avalonia.Styling.ThemeVariant.Light), nameof(Avalonia.Styling.ThemeVariant.Dark) };

		public string BooksDirectory { get; set; }
		public bool SavePodcastsToParentFolder { get; set; }
		public Serilog.Events.LogEventLevel LoggingLevel { get; set; }

		public string ThemeVariant
		{
			get => themeVariant;
			set
			{
				this.RaiseAndSetIfChanged(ref themeVariant, value);

				SelectionChanged = ThemeVariant != initialThemeVariant;
				this.RaisePropertyChanged(nameof(SelectionChanged));
			}
		}
		public bool SelectionChanged { get; private set; }
	}
}
