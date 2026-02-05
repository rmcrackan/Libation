using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using LibationFileManager;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels;

partial class MainVM
{
	private string lastGoodSearch = string.Empty;
	private QuickFilters.NamedFilter? lastGoodFilter => new(lastGoodSearch, null);

	/// <summary> Library filterting query </summary>
	public QuickFilters.NamedFilter? SelectedNamedFilter { get => field; set => this.RaiseAndSetIfChanged(ref field, value); } = new(string.Empty, null);
	public AvaloniaList<Control> QuickFilterMenuItems { get; } = new();
	/// <summary> Indicates if the first quick filter is the default filter </summary>
	public bool FirstFilterIsDefault { get => field; set => QuickFilters.UseDefault = this.RaiseAndSetIfChanged(ref field, value); }

	private void Configure_Filters()
	{
		FirstFilterIsDefault = QuickFilters.UseDefault;
		MainWindow.Initialized += updateFiltersMenu;
		QuickFilters.Updated += updateFiltersMenu;

		//We need to be able to dynamically add and remove menu items from the Quick Filters menu.
		//To do that, we need quick filter's menu items source to be writable, which we can only
		//achieve by creating the list ourselves (instead of allowing Avalonia to create it from the xaml)

		QuickFilterMenuItems.Add(new MenuItem
		{

			Header = "Start Libation with 1st filter _Default",
			Command = ReactiveCommand.Create(ToggleFirstFilterIsDefault),
			Icon = new CheckBox
			{
				BorderThickness = new Thickness(0),
				IsHitTestVisible = false,
				[!CheckBox.IsCheckedProperty] = new Binding(nameof(FirstFilterIsDefault))
			}
		});
		QuickFilterMenuItems.Add(new MenuItem { Header = "_Edit quick filters...", Command = ReactiveCommand.Create(EditQuickFiltersAsync) });
		QuickFilterMenuItems.Add(new Separator());
	}

	public void AddQuickFilterBtn() { if (SelectedNamedFilter != null) QuickFilters.Add(SelectedNamedFilter); }
	public async Task FilterBtn(string filterString) => await PerformFilter(new(filterString, null));
	public void FilterHelpBtn() => MainWindow.ShowSearchSyntaxDialog();
	public void ToggleFirstFilterIsDefault() => FirstFilterIsDefault = !FirstFilterIsDefault;
	public async Task EditQuickFiltersAsync() => await new LibationAvalonia.Dialogs.EditQuickFilters().ShowDialog(MainWindow);
	public async Task PerformFilter(QuickFilters.NamedFilter? namedFilter)
	{
		SelectedNamedFilter = namedFilter;
		var tryFilter = namedFilter?.Filter;

		try
		{
			await ProductsDisplay.Filter(tryFilter);
			lastGoodSearch = namedFilter?.Filter ?? "";
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error performing filtering. {@namedFilter} {@lastGoodFilter}", namedFilter, lastGoodFilter);
			await MessageBox.Show($"Bad filter string: \"{tryFilter}\"\r\n\r\n{ex.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

			// re-apply last good filter
			namedFilter = (namedFilter ?? new(string.Empty, null)) with { Filter = lastGoodSearch };
			await PerformFilter(namedFilter);
		}
	}

	private void updateFiltersMenu(object? _ = null, object? __ = null)
	{
		if (NativeMenu.GetMenu(MainWindow)?.Items[3] is not NativeMenuItem ss ||
			ss.Menu is not NativeMenu quickFilterNativeMenu)
		{
			Serilog.Log.Logger.Error($"Unable to find {nameof(quickFilterNativeMenu)}");
			return;
		}

		//Clear all filters
		for (int i = quickFilterNativeMenu.Items.Count - 1; i >= 3; i--)
		{
			var command = ((NativeMenuItem)quickFilterNativeMenu.Items[i]).Command as IDisposable;
			if (command != null)
			{
				var existingBinding = MainWindow.KeyBindings.FirstOrDefault(kb => kb.Command == command);
				if (existingBinding != null)
					MainWindow.KeyBindings.Remove(existingBinding);

				command.Dispose();
			}

			quickFilterNativeMenu.Items.RemoveAt(i);
			QuickFilterMenuItems.RemoveAt(i);
		}

		// re-populate
		var index = 0;
		foreach (var filter in QuickFilters.Filters)
		{
			var command = ReactiveCommand.Create(async () => await PerformFilter(filter));

			var menuItem = new MenuItem { Header = $"{++index}: {(string.IsNullOrWhiteSpace(filter.Name) ? filter.Filter : filter.Name)}", Command = command };
			var nativeMenuItem = new NativeMenuItem { Header = $"{index}: {(string.IsNullOrWhiteSpace(filter.Name) ? filter.Filter : filter.Name)}", Command = command };

			if (Configuration.IsMacOs && index <= 10)
			{
				//Register hotkeys Command + 1 - 0 for quick filters
				var key = index == 10 ? Key.D0 : Key.D0 + index;
				nativeMenuItem.Gesture = new KeyGesture(key, KeyModifiers.Meta);
			}
			else if (!Configuration.IsMacOs && index <= 12)
			{
				//Register hotkeys F1 - F12 for quick filters
				menuItem.InputGesture = new KeyGesture(Key.F1 + index - 1);
				MainWindow.KeyBindings.Add(new KeyBinding { Command = command, Gesture = menuItem.InputGesture });
			}

			QuickFilterMenuItems.Add(menuItem);
			quickFilterNativeMenu.Items.Add(nativeMenuItem);
		}
	}
}
