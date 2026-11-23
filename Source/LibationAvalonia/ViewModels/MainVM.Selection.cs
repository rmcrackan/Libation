using DataLayer;
using LibationUiBase.GridView;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	public IReadOnlyList<GridEntry> SelectedEntries => ProductsDisplay.SelectedEntries;

	private void Configure_Selection()
		=> ProductsDisplay.SelectionChanged += (_, _) => this.RaisePropertyChanged(nameof(SelectedEntries));

	public Task ReplaceSelectionTagsAsync()
		=> ReplaceTagsAsync(() => SelectedEntries
			.SelectMany<GridEntry, LibraryBookEntry>(e => e is SeriesEntry s ? s.Children : e is LibraryBookEntry b ? [b] : [])
			.Select(e => e.LibraryBook)
			.ToList());
}
