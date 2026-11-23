using ApplicationServices;
using Avalonia.Media;
using Avalonia.Threading;
using DataLayer;
using Dinah.Core;
using LibationFileManager;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static LibationAvalonia.Dialogs.BookDetailsDialog;

namespace LibationAvalonia.ViewModels;

public class BookInspectorViewModel : ViewModelBase
{
	private readonly DispatcherTimer tagSaveTimer = new() { Interval = TimeSpan.FromMilliseconds(600) };
	private GridEntry? entry;
	private LibraryBook? pendingTagBook;
	private bool refreshing;

	public BookInspectorViewModel()
	{
		tagSaveTimer.Tick += async (_, _) => await FlushTagsAsync();
	}

	public bool HasEntry => entry is not null;
	public bool IsMulti { get => field; private set { this.RaiseAndSetIfChanged(ref field, value); this.RaisePropertyChanged(nameof(ShowEmpty)); } }
	public bool ShowEmpty => !HasEntry && !IsMulti;
	public GridContextMenu? Bulk { get; private set; }
	public string MultiTitle { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public string MultiSubtitle { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public bool MultiSubtitleVisible { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public string LiberateText { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public bool LiberateVisible { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool SetDownloadedEnabled { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool SetNotDownloadedEnabled { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool PdfVisible { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool SetPdfDownloadedEnabled { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool SetPdfNotDownloadedEnabled { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool HasBook => entry is LibraryBookEntry;
	public IImage? Cover { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public string TitleText { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public string Byline { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public string StatusText { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public bool StatusIsSuccess { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool StatusIsWarning { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool StatusIsError { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public string PrimaryText { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "";
	public bool PrimaryVisible { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool ShowFileVisible { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); }
	public string ShowFileText { get; } = OperatingSystem.IsMacOS() ? "Show in Finder" : OperatingSystem.IsWindows() ? "Show in Explorer" : "Show in folder";
	public string Description { get => field; private set { this.RaiseAndSetIfChanged(ref field, value); this.RaisePropertyChanged(nameof(HasDescription)); this.RaisePropertyChanged(nameof(DescriptionIsLong)); } } = "";
	public bool HasDescription => Description.Length > 0;
	public bool DescriptionIsLong => Description.Length > 300;
	public bool DescriptionExpanded
	{
		get => field;
		private set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			this.RaisePropertyChanged(nameof(DescriptionToggleText));
			this.RaisePropertyChanged(nameof(DescriptionMaxLines));
		}
	}
	public string DescriptionToggleText => DescriptionExpanded ? "Show less" : "Show more";
	public int DescriptionMaxLines => DescriptionExpanded ? 0 : 6;
	public void ToggleDescription() => DescriptionExpanded = !DescriptionExpanded;
	public IReadOnlyList<DetailItem> Details { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = [];

	public string? Tags
	{
		get => field;
		set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			if (refreshing || entry is not LibraryBookEntry lbe)
				return;
			pendingTagBook = lbe.LibraryBook;
			tagSaveTimer.Stop();
			tagSaveTimer.Start();
		}
	}

	public void SetEntry(GridEntry? newEntry)
	{
		_ = FlushTagsAsync();

		if (entry is not null)
			entry.PropertyChanged -= Entry_PropertyChanged;
		entry = newEntry;
		if (entry is not null)
			entry.PropertyChanged += Entry_PropertyChanged;
		DescriptionExpanded = false;

		Refresh();
		LoadLargeCover();
	}

	public void SetSelection(IReadOnlyList<GridEntry>? selection)
	{
		if (selection is not { Count: > 1 })
		{
			Bulk = null;
			IsMulti = false;
			return;
		}

		var menu = Bulk = new GridContextMenu(selection.ToArray(), '_');
		var pending = menu.LibraryBookEntries.Select(e => e.LibraryBook).UnLiberated().Count();
		MultiTitle = $"{selection.Count} books selected";
		MultiSubtitle = $"{pending} to liberate";
		MultiSubtitleVisible = pending > 0;
		LiberateText = $"Liberate {pending}";
		LiberateVisible = pending > 0;
		SetDownloadedEnabled = menu.SetDownloadedEnabled;
		SetNotDownloadedEnabled = menu.SetNotDownloadedEnabled;
		PdfVisible = menu.ShowPdfStatusItems;
		SetPdfDownloadedEnabled = menu.SetPdfDownloadedEnabled;
		SetPdfNotDownloadedEnabled = menu.SetPdfNotDownloadedEnabled;
		IsMulti = true;
	}

	public async Task FlushTagsAsync()
	{
		tagSaveTimer.Stop();
		var book = pendingTagBook;
		pendingTagBook = null;
		if (book is null)
			return;

		try
		{
			await book.UpdateUserDefinedItemAsync(Tags ?? "");
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Failed to save tags for {AudibleProductId}", book.Book.AudibleProductId);
		}
	}

	private void Entry_PropertyChanged(object? sender, PropertyChangedEventArgs e)
		=> Dispatcher.UIThread.Post(() =>
		{
			if (sender != entry)
				return;
			Refresh();
			if (e.PropertyName == nameof(GridEntry.Cover) && Cover is null)
				Cover = entry?.Cover as IImage;
		});

	private void Refresh()
	{
		refreshing = true;
		try
		{
			this.RaisePropertyChanged(nameof(HasEntry));
			this.RaisePropertyChanged(nameof(HasBook));

			if (entry is null)
			{
				Description = "";
				return;
			}

			var book = entry.Book;
			// Rows imported before descriptions were flattened still hold Audible HTML until a rescan.
			Description = AudibleUtilities.HtmlText.ToPlainText(book.Description, "\n\n");
			TitleText = entry.Title ?? book.Title;
			Cover ??= entry.Cover as IImage;
			Byline = string.IsNullOrWhiteSpace(entry.Narrators) ? entry.Authors ?? "" : $"{entry.Authors} · narrated by {entry.Narrators}";

			var locale = AudibleApi.Localization.Get(book.Locale);
			var details = new List<DetailItem>();
			void Add(string label, string? value)
			{
				if (!string.IsNullOrWhiteSpace(value))
					details.Add(new(label, value.Trim()));
			}
			Add("Length", entry.Length);
			Add("Purchased", entry.PurchaseDate);
			Add("Account", string.IsNullOrWhiteSpace(entry.Account) ? null : $"{entry.Account} ({locale.Name})");
			Add("Series", entry.Series);
			Add("Category", entry.Category);
			Details = details;

			if (entry is not LibraryBookEntry)
			{
				StatusText = "";
				PrimaryVisible = ShowFileVisible = false;
				StatusIsSuccess = StatusIsWarning = StatusIsError = false;
				return;
			}

			var status = entry.Liberate?.BookStatus ?? default;
			var unavailable = entry.Liberate?.IsUnavailable is true;
			(StatusText, StatusIsSuccess, StatusIsWarning, StatusIsError) = status switch
			{
				_ when unavailable => ("Unavailable", false, false, false),
				LiberatedStatus.PartialDownload => ("Partially downloaded", false, true, false),
				LiberatedStatus.Liberated => ("Downloaded", true, false, false),
				LiberatedStatus.Error => ("Error", false, false, true),
				_ => ("Not downloaded", false, false, false)
			};

			ShowFileVisible = !unavailable && status is LiberatedStatus.Liberated && AudioPath() is not null;
			PrimaryVisible = !unavailable && status is not LiberatedStatus.Liberated;
			PrimaryText = status switch
			{
				LiberatedStatus.PartialDownload => "Finish download",
				LiberatedStatus.Error => "Retry",
				_ => "Liberate"
			};

			if (pendingTagBook is null)
				Tags = book.UserDefinedItem.Tags;
		}
		finally
		{
			refreshing = false;
		}
	}

	private string? AudioPath()
	{
		try
		{
			return entry is null ? null : AudibleFileStorage.Audio.GetPath(entry.AudibleProductId)?.PathWithoutPrefix;
		}
		catch
		{
			return null;
		}
	}

	public void ShowFile()
	{
		if (AudioPath() is string path)
			Go.To.File(path);
	}

	public void OpenInAudible()
	{
		if (entry is null)
			return;
		var locale = AudibleApi.Localization.Get(entry.Book.Locale);
		Go.To.Url($"https://www.audible.{locale.TopDomain}/pd/{entry.AudibleProductId}");
	}

	private void LoadLargeCover()
	{
		Cover = entry?.Cover as IImage;
		if (entry?.Book is not { PictureId: string picId } book)
			return;

		var def = new PictureDefinition(picId, PictureSize._300x300);
		var (isDefault, bytes) = PictureStorage.GetPicture(def);
		if (!isDefault)
		{
			Cover = AvaloniaUtils.TryLoadImageOrDefault(bytes, PictureSize._300x300);
			return;
		}

		void OnCached(object? _, PictureCachedEventArgs e)
		{
			if (e.Definition.PictureId != picId)
				return;
			PictureStorage.PictureCached -= OnCached;
			Dispatcher.UIThread.Post(() =>
			{
				if (entry?.Book.PictureId == picId)
					Cover = AvaloniaUtils.TryLoadImageOrDefault(e.Picture, PictureSize._300x300);
			});
		}
		PictureStorage.PictureCached += OnCached;
	}
}
