using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Dinah.Core;
using FileManager;
using LibationAvalonia.Dialogs;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using System.Linq;

#nullable enable
namespace LibationAvalonia.Controls.Settings
{
	public partial class Important : UserControl
	{
		private ImportantSettingsVM? ViewModel => DataContext as ImportantSettingsVM;
		public Important()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
			{
				DataContext = new ImportantSettingsVM(Configuration.CreateMockInstance());
			}

			ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
		}

		private void EditThemeColors_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
			{
				//Only allow a single instance of the theme picker
				//Show it as a window, not a dialog, so users can preview
				//their changes throughout the entire app.
				if (lifetime.Windows.OfType<ThemePickerDialog>().FirstOrDefault() is ThemePickerDialog dialog)
				{
					dialog.BringIntoView();
				}
				else
				{
					var themePicker = new ThemePickerDialog();
					themePicker.Show();
				}
			}
		}

		private void ThemeComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
		{
			//Remove the combo box before changing the theme, then re-add it.
			//This is a workaround to a crash that will happen if the theme
			//is changed while the combo box is open
			ThemeComboBox.SelectionChanged -= ThemeComboBox_SelectionChanged;
			var parent = ThemeComboBox.Parent as Panel;
			if (parent?.Children.Remove(ThemeComboBox) ?? false)
			{

				Configuration.Instance.ThemeVariant = ViewModel?.ThemeVariant.Value ?? Configuration.Theme.System;
				parent.Children.Add(ThemeComboBox);
			}
			ThemeComboBox.SelectionChanged += ThemeComboBox_SelectionChanged;
		}
	}
}
