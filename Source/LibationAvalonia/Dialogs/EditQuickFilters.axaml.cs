using AudibleUtilities;
using Avalonia.Controls;
using LibationFileManager;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace LibationAvalonia.Dialogs
{
	public partial class EditQuickFilters : DialogWindow
	{
		public ObservableCollection<Filter> Filters { get; } = new();

		public class Filter : ViewModels.ViewModelBase
		{
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
		}
		public EditQuickFilters()
		{
			InitializeComponent();

			// WARNING: accounts persister will write ANY EDIT to object immediately to file
			// here: copy strings and dispose of persister
			// only persist in 'save' step
			using var persister = AudibleApiStorage.GetAccountsSettingsPersister();
			var accounts = persister.AccountsSettings.Accounts;
			if (!accounts.Any())
				return;

			ControlToFocusOnShow = this.FindControl<Button>(nameof(SaveButton_Clicked));

			var allFilters = QuickFilters.Filters.Select(f => new Filter { FilterString = f }).ToList();
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
		}

		protected override void SaveAndClose()
		{
			QuickFilters.ReplaceAll(Filters.Where(f => !f.IsDefault).Select(f => f.FilterString));
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
			}
		}

		public void MoveUpButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is Button btn && btn.DataContext is Filter filter)
			{
				var index = Filters.IndexOf(filter);
				if (index < 1) return;

				Filters.Remove(filter);
				Filters.Insert(index - 1, filter);
			}
		}

		public void MoveDownButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (e.Source is Button btn && btn.DataContext is Filter filter)
			{
				var index = Filters.IndexOf(filter);
				if (index >= Filters.Count - 2) return;

				Filters.Remove(filter);
				Filters.Insert(index + 1, filter);
			}
		}

		public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			SaveAndClose();
		}
	}
}
