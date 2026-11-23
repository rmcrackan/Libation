using Avalonia.Controls;
using Avalonia.Interactivity;
using LibationAvalonia.ViewModels;
using LibationSearchEngine;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Dialogs;

public partial class SearchSyntaxDialog : DialogWindow
{
	public event EventHandler<string>? InsertRequested;
	public event EventHandler<string>? ReplaceRequested;

	public record FieldInfo(FieldType Type, string DisplayName, string Description, string[] Synonyms)
	{
		public bool HasSynonyms => Synonyms.Length > 0;
		public string AlsoText => "Also: " + string.Join(", ", Synonyms);
		public string InsertText => Type == FieldType.Bool ? DisplayName : DisplayName + ":";
		public string ToolTipText => $"Click to add {InsertText} to the search";
	}

	public record ExampleRow(string Code, string Description);

	private static readonly Dictionary<string, (string DisplayName, string Description)> Meta = new(StringComparer.OrdinalIgnoreCase)
	{
		["AudibleProductId"] = ("Id", "Audible product ID (ASIN)"),
		["Title"] = ("Title", "Title and subtitle"),
		["AuthorNames"] = ("Author", "Author names"),
		["NarratorNames"] = ("Narrator", "Narrator names"),
		["Publisher"] = ("Publisher", "Publisher"),
		["SeriesNames"] = ("Series", "Series names"),
		["SeriesId"] = ("SeriesId", "Audible series ID"),
		["CategoriesId"] = ("CategoryId", "Audible category IDs"),
		["Category"] = ("Category", "Genre / category names"),
		["Tags"] = ("Tags", "Your tags; [tag] is a shortcut"),
		["Locale"] = ("Locale", "Audible marketplace, e.g. us, uk"),
		["Account"] = ("Account", "Account the book belongs to"),
		["Codec"] = ("Codec", "Format of the last download, e.g. aac, mp3"),
		["HasDownloads"] = ("HasPDF", "Book comes with a PDF"),
		["IsRated"] = ("IsRated", "You've rated it"),
		["IsAuthorNarrated"] = ("AuthorNarrated", "Narrated by the author"),
		["IsAbridged"] = ("Abridged", "Abridged edition"),
		["IsSpatial"] = ("IsSpatial", "Dolby Atmos / spatial audio"),
		["IsLiberated"] = ("Liberated", "Downloaded and decrypted"),
		["LiberatedError"] = ("LiberatedError", "Last download failed"),
		["Podcast"] = ("Podcast", "Podcast episode"),
		["AbsentFromLastScan"] = ("Absent", "Missing from the last library scan"),
		["IsInSeries"] = ("InSeries", "Part of a series"),
		["HasSubtitle"] = ("HasSubtitle", "Has a subtitle"),
		["TitleHasColon"] = ("TitleHasColon", "Title contains a colon"),
		["IsFinished"] = ("Finished", "Marked as finished"),
		["IsAudiblePlus"] = ("AudiblePlus", "From the Audible Plus catalog"),
		["LengthInMinutes"] = ("Length", "Length in minutes"),
		["Hours"] = ("Hours", "Length in whole hours"),
		["ProductRating"] = ("Rating", "Audible's overall rating, 0–5"),
		["UserRating"] = ("MyRating", "Your rating, 0–5"),
		["DatePublished"] = ("DatePublished", "Release date, yyyymmdd"),
		["LastDownloaded"] = ("LastDownloaded", "When you last downloaded it, yyyymmdd"),
		["Bitrate"] = ("Bitrate", "Bitrate of the last download, kbps"),
		["SampleRate"] = ("SampleRate", "Sample rate of the last download, Hz"),
		["DateAdded"] = ("DateAdded", "When it was added to your library, yyyymmdd"),
	};

	private static List<FieldInfo> BuildFields()
	{
		var claimed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var result = new List<FieldInfo>();
		foreach (var rule in SearchEngine.FieldIndexRules.Where(r => r.FieldType != FieldType.Raw))
		{
			var names = new List<string>();
			foreach (var name in rule.FieldNames)
				if (claimed.Add(name))
					names.Add(name);
			if (names.Count == 0)
				continue;

			var (display, description) = Meta.TryGetValue(rule.FieldNames[0], out var m) ? m : (names[0], "");
			if (!names.Contains(display, StringComparer.OrdinalIgnoreCase))
				display = names[0];
			var synonyms = names.Where(n => !n.Equals(display, StringComparison.OrdinalIgnoreCase)).ToArray();
			result.Add(new FieldInfo(rule.FieldType, display, description, synonyms));
		}
		return result;
	}

	public class SearchSyntaxViewModel : ViewModelBase
	{
		private static readonly List<FieldInfo> AllFields = BuildFields();

		public IReadOnlyList<ExampleRow> Examples { get; } =
		[
			new("title:\"wizard of oz\"", "Title contains the exact phrase"),
			new("author:sanderson", "Books by an author"),
			new("author:king AND -IsLiberated", "An author's books you haven't downloaded yet"),
			new("series:mistborn OR series:stormlight", "Books in either series"),
			new("[favorites]", "Books tagged favorites"),
			new("length:[60 TO 600]", "Between 1 and 10 hours long (minutes)"),
			new("hours:[10 TO 100]", "10 hours or longer"),
			new("datepublished:[20200101 TO 20231231]", "Published 2020 through 2023 (yyyymmdd)"),
			new("-IsRated", "Books you haven't rated"),
			new("id:B015D78L0U", "One book by its Audible ID (ASIN)"),
		];

		public IReadOnlyList<ExampleRow> Combining { get; } =
		[
			new("A AND B", "Both must match"),
			new("A OR B", "Either matches (the default between words)"),
			new("-A  or  NOT A", "Excludes matches"),
			new("\"exact phrase\"", "Words together, in order"),
			new("tolk*", "Wildcard: any ending. ? matches one character"),
			new("[a TO b]", "Range, inclusive. Numbers and yyyymmdd dates"),
			new("( … )", "Groups terms, e.g. author:king AND (series:\"dark tower\" OR -IsInSeries)"),
		];

		public string FieldFilter
		{
			get => field;
			set
			{
				this.RaiseAndSetIfChanged(ref field, value);
				Refilter();
			}
		} = "";

		public IReadOnlyList<FieldInfo> TextFields { get; private set; } = [];
		public IReadOnlyList<FieldInfo> NumberFields { get; private set; } = [];
		public IReadOnlyList<FieldInfo> BoolFields { get; private set; } = [];
		public IReadOnlyList<FieldInfo> IdFields { get; private set; } = [];
		public bool HasText => TextFields.Count > 0;
		public bool HasNumber => NumberFields.Count > 0;
		public bool HasBool => BoolFields.Count > 0;
		public bool HasId => IdFields.Count > 0;
		public bool HasNone => !(HasText || HasNumber || HasBool || HasId);

		public SearchSyntaxViewModel() => Refilter();

		private void Refilter()
		{
			var q = FieldFilter.Trim();
			var matches = q.Length == 0
				? AllFields
				: AllFields.Where(f =>
					f.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)
					|| f.Description.Contains(q, StringComparison.OrdinalIgnoreCase)
					|| f.Synonyms.Any(s => s.Contains(q, StringComparison.OrdinalIgnoreCase))).ToList();

			TextFields = matches.Where(f => f.Type == FieldType.String).ToList();
			NumberFields = matches.Where(f => f.Type == FieldType.Number).ToList();
			BoolFields = matches.Where(f => f.Type == FieldType.Bool).ToList();
			IdFields = matches.Where(f => f.Type == FieldType.ID).ToList();

			this.RaisePropertyChanged(nameof(TextFields));
			this.RaisePropertyChanged(nameof(NumberFields));
			this.RaisePropertyChanged(nameof(BoolFields));
			this.RaisePropertyChanged(nameof(IdFields));
			this.RaisePropertyChanged(nameof(HasText));
			this.RaisePropertyChanged(nameof(HasNumber));
			this.RaisePropertyChanged(nameof(HasBool));
			this.RaisePropertyChanged(nameof(HasId));
			this.RaisePropertyChanged(nameof(HasNone));
		}
	}

	private readonly SearchSyntaxViewModel _viewModel = new();

	public SearchSyntaxDialog()
	{
		InitializeComponent();
		DataContext = _viewModel;
	}

	private void Field_Click(object? sender, RoutedEventArgs e)
	{
		if (sender is Control { DataContext: FieldInfo field })
			InsertRequested?.Invoke(this, field.InsertText);
	}

	private void Example_Click(object? sender, RoutedEventArgs e)
	{
		if (sender is Control { DataContext: ExampleRow row })
			ReplaceRequested?.Invoke(this, row.Code);
	}

	private void ClearFieldFilter_Click(object? sender, RoutedEventArgs e) => _viewModel.FieldFilter = "";
}
