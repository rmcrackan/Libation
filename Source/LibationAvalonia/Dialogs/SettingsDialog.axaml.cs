using Avalonia.Controls;
using FileManager;
using LibationAvalonia.ViewModels.Settings;
using LibationFileManager;
using LibationUiBase.Forms;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public partial class SettingsDialog : DialogWindow
	{
		private SettingsVM settingsDisp;

		private readonly Configuration config = Configuration.Instance;
		public SettingsDialog()
		{
			if (Design.IsDesignMode)
				_ = Configuration.Instance.LibationFiles;
			InitializeComponent();

			DataContext = settingsDisp = new(config);
		}

		protected override async Task SaveAndCloseAsync()
		{
			#region validation

			if (string.IsNullOrWhiteSpace(settingsDisp.ImportantSettings.BooksDirectory))
			{
				await MessageBox.Show(this.GetParentWindow(), "Cannot set Books Location to blank", "Location is blank", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			#endregion

			settingsDisp.SaveSettings(config);

			await MessageBox.VerboseLoggingWarning_ShowIfTrue();
			await base.SaveAndCloseAsync();
		}

		public async void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		{
			LongPath lonNewBooks = settingsDisp.ImportantSettings.GetBooksDirectory();
			if (!System.IO.Directory.Exists(lonNewBooks))
			{
				try
				{
					System.IO.Directory.CreateDirectory(lonNewBooks);
				}
				catch (Exception ex)
				{
					await MessageBox.Show(this, $"Error creating Books Location:\n\n{ex.Message}", "Error creating directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
			}
			await SaveAndCloseAsync();
		}
	}
}
