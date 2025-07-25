using AudibleUtilities;
using Avalonia.Collections;
using Avalonia.Controls;
using LibationFileManager;
using ReactiveUI;
using System.Linq;

namespace LibationAvalonia.Dialogs
{
	public partial class EditQuickFilters : DialogWindow
	{
		public AvaloniaList<Filter> Filters { get; } = new();

		public class Filter : ViewModels.ViewModelBase
		{ 
            private string _name;
            public string Name
            {
                get => _name;
                set => this.RaiseAndSetIfChanged(ref _name, value);
			}

            private string _filterString;
			public string FilterString
			{
				get => _filterString;
				set
				{
					IsDefault = string.IsNullOrEmpty(value);
					this.RaiseAndSetIfChanged(ref _filterString, value);
					this.RaisePropertyChanged(nameof(IsDefault));
				}
			}
			public bool IsDefault { get; private set; } = true;
			private bool _isTop;
			private bool _isBottom;
			public bool IsTop { get => _isTop; set => this.RaiseAndSetIfChanged(ref _isTop, value); }
			public bool IsBottom { get => _isBottom; set => this.RaiseAndSetIfChanged(ref _isBottom, value); }

			public QuickFilters.NamedFilter AsNamedFilter() => new(FilterString, Name);

		}
		public EditQuickFilters()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
			{
				Filters = [
					new Filter { Name = "Filter 1", FilterString = "[filter1 string]", IsTop = true },
					new Filter { Name = "Filter 2", FilterString = "[filter2 string]" },
					new Filter { Name = "Filter 3", FilterString = "[filter3 string]" },
					new Filter { Name = "Filter 4", FilterString = "[filter4 string]", IsBottom = true },
					new Filter()];
				DataContext = this;
				return;
			}

			// WARNING: accounts persister will write ANY EDIT to object immediately to file
			// here: copy strings and dispose of persister
			// only persist in 'save' step
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.Accounts;
			if (!accounts.Any())
				return;

			ControlToFocusOnShow = this.FindControl<Button>(nameof(saveBtn));

			var allFilters = QuickFilters.Filters.Select(f => new Filter { FilterString = f.Filter, Name = f.Name }).ToList();
			allFilters[0].IsTop = true;
			allFilters[^1].IsBottom = true;
			allFilters.Add(new Filter());

			foreach (var f in allFilters)
				f.PropertyChanged += Filter_PropertyChanged;

			Filters = new(allFilters);
			DataContext = this;
		}

		private void Filter_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (Filters.Any(f => f.IsDefault))
				return;
			var newBlank = new Filter();
			newBlank.PropertyChanged += Filter_PropertyChanged;
			Filters.Insert(Filters.Count, newBlank);
			ReIndexFilters();
		}

		protected override void SaveAndClose()
		{
			QuickFilters.ReplaceAll(Filters.Where(f => !f.IsDefault).Select(x => x.AsNamedFilter()));
			base.SaveAndClose();
		}

		public void DeleteButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is Button btn && btn.DataContext is Filter filter)
			{
				var index = Filters.IndexOf(filter);
				if (index < 0) return;

				filter.PropertyChanged -= Filter_PropertyChanged;
				Filters.Remove(filter);
				ReIndexFilters();
			}
		}

		public void MoveUpButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is not Button btn || btn.DataContext is not Filter filter || filter.IsDefault)
				return;

			var oldIndex = Filters.IndexOf(filter);
			if (oldIndex < 1) return;

			var filterCount = Filters.Count(f => !f.IsDefault);

			MoveFilter(oldIndex, oldIndex - 1, filterCount);
		}

		public void MoveDownButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is not Button btn || btn.DataContext is not Filter filter || filter.IsDefault)
				return;

			var filterCount = Filters.Count(f => !f.IsDefault);
			var oldIndex = Filters.IndexOf(filter);
			if (oldIndex >= filterCount - 1) return;

			MoveFilter(oldIndex, oldIndex + 1, filterCount);
		}

		private void MoveFilter(int oldIndex, int newIndex, int filterCount)
		{
			var filter = Filters[oldIndex];
			Filters.RemoveAt(oldIndex);
			Filters.Insert(newIndex, filter);

			Filters[oldIndex].IsTop = oldIndex == 0;
			Filters[newIndex].IsTop = newIndex == 0;
			Filters[newIndex].IsBottom = newIndex == filterCount - 1;
			Filters[oldIndex].IsBottom = oldIndex == filterCount - 1;
		}

		private void ReIndexFilters()
		{
			var filterCount = Filters.Count(f => !f.IsDefault);
			for (int i = filterCount - 1; i >= 0; i--)
			{
				Filters[i].IsTop = i == 0;
				Filters[i].IsBottom = i == filterCount - 1;
			}
		}
	}
}
