using ApplicationServices;
using Dinah.Core;
using Avalonia.Collections;
using Avalonia.Threading;
using LibationFileManager;
using LibationUiBase.GridView;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

public class SidebarItem(string label, Func<Task> activate) : ViewModelBase
{
	public string Label { get; } = label;
	public Func<Task> Activate { get; } = activate;
	public string? Count { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public bool IsSelected { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
}

partial class MainVM
{
	private readonly Dictionary<LibraryScope, SidebarItem> scopeItems = new();
	private LibraryScope scopeBeforeRemoval;

	public IReadOnlyList<SidebarItem> LibraryScopeItems { get; private set; } = [];
	public IReadOnlyList<SidebarItem> BrowseScopeItems { get; private set; } = [];
	public AvaloniaList<SidebarItem> QuickFilterItems { get; } = new();

	public string AccountsLine { get => field; private set => this.RaiseAndSetIfChanged(ref field, value); } = "No accounts";
	public string TrashCountText => BooksInTrash > 0 ? BooksInTrash.ToString() : "";

	/// <summary> The right-hand panel (Details or Queue) is open. </summary>
	public bool InspectorOpen
	{
		get => field;
		set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			this.RaisePropertyChanged(nameof(QueueOpen));
			this.RaisePropertyChanged(nameof(DetailsOpen));
			Configuration.Instance.SetNonString(value, nameof(InspectorOpen));
		}
	}

	public bool SidebarOpen
	{
		get => field;
		set
		{
			this.RaiseAndSetIfChanged(ref field, value);
			Configuration.Instance.SetNonString(value, nameof(SidebarOpen));
		}
	}

	public int SelectedCount => ProductsDisplay.SelectedEntries.Count;
	public bool SelectedCountVisible => SelectedCount > 0;
	public string SelectedCountText => $"· {SelectedCount} selected";
	public GridEntry? SelectedEntry => ProductsDisplay.SelectedEntries.Count == 1 ? ProductsDisplay.SelectedEntries[0] : null;
	public string InspectorEmptyText => SelectedCount > 1 ? $"{SelectedCount} books selected" : "Select a book to see its details";

	public bool LiberateAllVisible => LibraryStats?.PendingBooks > 0;
	public string LiberateAllText => $"Liberate {LibraryStats?.PendingBooks ?? 0}";

	private void Configure_Sidebar()
	{
		InspectorOpen = Configuration.Instance.GetNonString(defaultValue: true, nameof(InspectorOpen));
		SidebarOpen = Configuration.Instance.GetNonString(defaultValue: true, nameof(SidebarOpen));

		SidebarItem scope(LibraryScope s, string label)
			=> scopeItems[s] = new(label, () => SelectScopeAsync(s)) { IsSelected = s is LibraryScope.All };

		LibraryScopeItems =
		[
			scope(LibraryScope.All, "All books"),
			scope(LibraryScope.NotDownloaded, "Not downloaded"),
			scope(LibraryScope.InProgress, "In progress"),
			scope(LibraryScope.Downloaded, "Downloaded"),
			scope(LibraryScope.NeedsAttention, "Needs attention"),
			scope(LibraryScope.Unavailable, "Unavailable")
		];
		BrowseScopeItems =
		[
			scope(LibraryScope.Series, "Series"),
			scope(LibraryScope.Podcasts, "Podcasts"),
			scope(LibraryScope.AudiblePlus, "Audible Plus")
		];

		MainWindow.Loaded += rebuildQuickFilterItems;
		QuickFilters.Updated += rebuildQuickFilterItems;

		ProductsDisplay.SelectionChanged += (_, _) =>
		{
			foreach (var name in new[] { nameof(SelectedCount), nameof(SelectedCountVisible), nameof(SelectedCountText), nameof(SelectedEntry), nameof(InspectorEmptyText) })
				this.RaisePropertyChanged(name);
		};
	}

	public async Task SelectScopeAsync(LibraryScope scope)
	{
		if (ProductsDisplay.Scope == scope)
		{
			if (scope is LibraryScope.All)
				return;
			scope = LibraryScope.All;
		}

		foreach (var (key, item) in scopeItems)
			item.IsSelected = key == scope;

		await ProductsDisplay.SetScopeAsync(scope);
		this.RaisePropertyChanged(nameof(HasActiveFilter));
		RaiseGettingStartedChanged();
		await RefreshNoMatchesStateAsync(SelectedNamedFilter?.Filter);
	}

	public void ToggleInspectorBtn() => InspectorOpen = !InspectorOpen;
	public void ToggleSidebarBtn() => SidebarOpen = !SidebarOpen;

	private void updateSidebarCounts()
	{
		var s = LibraryStats;
		string? count(int? n) => s is null ? null : n.ToString();

		scopeItems[LibraryScope.All].Count = count(LibraryTotal);
		scopeItems[LibraryScope.NotDownloaded].Count = count(s?.booksNoProgress);
		scopeItems[LibraryScope.InProgress].Count = count(s?.booksDownloadedOnly);
		scopeItems[LibraryScope.Downloaded].Count = count(s?.booksFullyBackedUp);
		scopeItems[LibraryScope.NeedsAttention].Count = count(s?.booksError);
		scopeItems[LibraryScope.Unavailable].Count = count(s?.booksUnavailable);

		this.RaisePropertyChanged(nameof(LiberateAllVisible));
		this.RaisePropertyChanged(nameof(LiberateAllText));
	}

	private void rebuildQuickFilterItems(object? _ = null, object? __ = null)
		=> Dispatcher.UIThread.Post(() =>
		{
			QuickFilterItems.Clear();
			QuickFilterItems.AddRange(QuickFilters.Filters.Select(filter => new SidebarItem(
				string.IsNullOrWhiteSpace(filter.Name) ? filter.Filter : filter.Name,
				() => SelectedNamedFilter?.Filter == filter.Filter ? FilterBtn(string.Empty) : PerformFilter(filter))));
			markSelectedQuickFilter();
		});

	private void markSelectedQuickFilter()
	{
		var current = SelectedNamedFilter?.Filter;
		var filters = QuickFilters.Filters.ToList();
		for (var i = 0; i < QuickFilterItems.Count && i < filters.Count; i++)
			QuickFilterItems[i].IsSelected = !string.IsNullOrWhiteSpace(current) && filters[i].Filter == current;
	}

	private void updateAccountsLine(IReadOnlyCollection<AudibleUtilities.Account> accounts)
		=> AccountsLine = accounts.Count == 0
			? "No accounts"
			: $"{"account".PluralizeWithCount(accounts.Count)} · {string.Join(", ", accounts.Select(a => string.IsNullOrWhiteSpace(a.AccountName) ? a.AccountId : a.AccountName))}";
}
