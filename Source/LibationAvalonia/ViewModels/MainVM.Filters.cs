using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using LibationFileManager;
using ReactiveUI;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LibationAvalonia.ViewModels
{
	partial class MainVM
	{
		private string lastGoodFilter = "";
		private string _filterString;
		private bool _firstFilterIsDefault = true;

		/// <summary> Library filterting query </summary>
		public string FilterString { get => _filterString; set => this.RaiseAndSetIfChanged(ref _filterString, value); }
		public AvaloniaList<Control> QuickFilterMenuItems { get; } = new();

		/// <summary> Indicates if the first quick filter is the default filter </summary>
		public bool FirstFilterIsDefault
		{
			get => _firstFilterIsDefault;
			set
			{
				if (value != _firstFilterIsDefault)
					QuickFilters.UseDefault = value;
				this.RaiseAndSetIfChanged(ref _firstFilterIsDefault, value);
			}
		}


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
				Command = ReactiveCommand.Create(FirstFilterIsDefaultToggle),
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

		public void AddQuickFilterBtn() => QuickFilters.Add(FilterString);
		public async Task FilterBtn() => await PerformFilter(FilterString);
		public async Task FilterHelpBtn() => await new LibationAvalonia.Dialogs.SearchSyntaxDialog().ShowDialog(MainWindow);
		public void FirstFilterIsDefaultToggle() => FirstFilterIsDefault = !FirstFilterIsDefault;
		public async Task EditQuickFiltersAsync() => await new LibationAvalonia.Dialogs.EditQuickFilters().ShowDialog(MainWindow);
		public async Task PerformFilter(string filterString)
		{
			FilterString = filterString;

			try
			{
				await ProductsDisplay.Filter(filterString);
				lastGoodFilter = filterString;
			}
			catch (Exception ex)
			{
				await MessageBox.Show($"Bad filter string:\r\n\r\n{ex.Message}", "Bad filter string", MessageBoxButtons.OK, MessageBoxIcon.Error);

				// re-apply last good filter
				await PerformFilter(lastGoodFilter);
			}
		}

		private void updateFiltersMenu(object _ = null, object __ = null)
		{
			//Clear all filters
			var quickFilterNativeMenu = (NativeMenuItem)NativeMenu.GetMenu(MainWindow).Items[3];
			for (int i = quickFilterNativeMenu.Menu.Items.Count - 1; i >= 3; i--)
			{
				var command = ((NativeMenuItem)quickFilterNativeMenu.Menu.Items[i]).Command as IDisposable;
				if (command != null)
				{
					var existingBinding = MainWindow.KeyBindings.FirstOrDefault(kb => kb.Command == command);
					if (existingBinding != null)
						MainWindow.KeyBindings.Remove(existingBinding);

					command.Dispose();
				}

				quickFilterNativeMenu.Menu.Items.RemoveAt(i);
				QuickFilterMenuItems.RemoveAt(i);
			}

			// re-populate
			var index = 0;
			foreach (var filter in QuickFilters.Filters)
			{
				var command = ReactiveCommand.Create(async () => await PerformFilter(filter));

				var menuItem = new MenuItem { Header = $"{++index}: {filter}", Command = command };
				var nativeMenuItem = new NativeMenuItem { Header = $"{index}: {filter}", Command = command };

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
				quickFilterNativeMenu.Menu.Items.Add(nativeMenuItem);
			}
		}
	}
}
