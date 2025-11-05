using Dinah.Core;
using FileManager;
using LibationFileManager;
using LibationUiBase;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace LibationAvalonia.ViewModels.Settings
{
	public class ImportantSettingsVM : ViewModelBase
	{
		private string themeVariant;
		private string initialThemeVariant;
		private readonly Configuration config;

		public ImportantSettingsVM(Configuration config)
		{
			this.config = config;

			BooksDirectory = config.Books?.PathWithoutPrefix ?? Configuration.DefaultBooksDirectory;
			SavePodcastsToParentFolder = config.SavePodcastsToParentFolder;
			OverwriteExisting = config.OverwriteExisting;
			CreationTime = DateTimeSources.SingleOrDefault(v => v.Value == config.CreationTime) ?? DateTimeSources[0];
			LastWriteTime = DateTimeSources.SingleOrDefault(v => v.Value == config.LastWriteTime) ?? DateTimeSources[0];
			LoggingLevel = config.LogLevel;
			GridScaleFactor = scaleFactorToLinearRange(config.GridScaleFactor);
			GridFontScaleFactor = scaleFactorToLinearRange(config.GridFontScaleFactor);

			themeVariant = initialThemeVariant = config.GetString(propertyName: nameof(ThemeVariant)) ?? "";
			if (string.IsNullOrWhiteSpace(initialThemeVariant))
				themeVariant = initialThemeVariant = "System";
		}

		public void SaveSettings(Configuration config)
		{			
			config.Books = GetBooksDirectory();
			config.SavePodcastsToParentFolder = SavePodcastsToParentFolder;
			config.OverwriteExisting = OverwriteExisting;
			config.CreationTime = CreationTime.Value;
			config.LastWriteTime = LastWriteTime.Value;
			config.LogLevel = LoggingLevel;
		}

		public LongPath GetBooksDirectory()
			=> Configuration.GetKnownDirectory(BooksDirectory) is Configuration.KnownDirectories.None ? BooksDirectory : System.IO.Path.Combine(BooksDirectory, "Books");

		private static float scaleFactorToLinearRange(float scaleFactor)
			=> float.Round(100 * MathF.Log2(scaleFactor));
		private static float linearRangeToScaleFactor(float value)
			=> MathF.Pow(2, value / 100f);

		public void ApplyDisplaySettings()
		{
			config.GridFontScaleFactor = linearRangeToScaleFactor(GridFontScaleFactor);
			config.GridScaleFactor = linearRangeToScaleFactor(GridScaleFactor);
		}
		public void OpenLogFolderButton()
		{
			if (System.IO.File.Exists(LogFileFilter.LogFilePath))
				Go.To.File(LogFileFilter.LogFilePath);
			else
				Go.To.Folder(((LongPath)Configuration.Instance.LibationFiles).ShortPathName);
		}

		public List<Configuration.KnownDirectories> KnownDirectories { get; } = new()
		{
			Configuration.KnownDirectories.UserProfile,
			Configuration.KnownDirectories.AppDir,
			Configuration.KnownDirectories.MyDocs,
			Configuration.KnownDirectories.MyMusic,
		};

		public string BooksText { get; } = Configuration.GetDescription(nameof(Configuration.Books));
		public string SavePodcastsToParentFolderText { get; } = Configuration.GetDescription(nameof(Configuration.SavePodcastsToParentFolder));
		public string OverwriteExistingText { get; } = Configuration.GetDescription(nameof(Configuration.OverwriteExisting));
		public string CreationTimeText { get; } = Configuration.GetDescription(nameof(Configuration.CreationTime));
		public string LastWriteTimeText { get; } = Configuration.GetDescription(nameof(Configuration.LastWriteTime));
		public EnumDisplay<Configuration.DateTimeSource>[] DateTimeSources { get; }
			= Enum.GetValues<Configuration.DateTimeSource>()
			.Select(v => new EnumDisplay<Configuration.DateTimeSource>(v))
			.ToArray();
		public Serilog.Events.LogEventLevel[] LoggingLevels { get; } = Enum.GetValues<Serilog.Events.LogEventLevel>();
		public string GridScaleFactorText { get; } = Configuration.GetDescription(nameof(Configuration.GridScaleFactor));
		public string GridFontScaleFactorText { get; } = Configuration.GetDescription(nameof(Configuration.GridFontScaleFactor));
		public string BetaOptInText { get; } = Configuration.GetDescription(nameof(Configuration.BetaOptIn));
		public string[] Themes { get; } = { "System", nameof(Avalonia.Styling.ThemeVariant.Light), nameof(Avalonia.Styling.ThemeVariant.Dark) };

		public string BooksDirectory { get; set; }
		public bool SavePodcastsToParentFolder { get; set; }
		public bool OverwriteExisting { get; set; }
		public float GridScaleFactor { get; set; }
		public float GridFontScaleFactor { get; set; }
		public EnumDisplay<Configuration.DateTimeSource> CreationTime { get; set; }
		public EnumDisplay<Configuration.DateTimeSource> LastWriteTime { get; set; }
		public Serilog.Events.LogEventLevel LoggingLevel { get; set; }

		public string ThemeVariant
		{
			get => themeVariant;
			set => this.RaiseAndSetIfChanged(ref themeVariant, value);
		}
	}
}
