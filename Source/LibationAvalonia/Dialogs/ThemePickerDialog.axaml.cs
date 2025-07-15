using Avalonia.Collections;
using Avalonia.Media;
using ReactiveUI;
using Avalonia.Styling;
using System;
using LibationAvalonia.Themes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Platform.Storage;
using LibationUiBase.Forms;

#nullable enable
namespace LibationAvalonia.Dialogs;

public partial class ThemePickerDialog : DialogWindow
{
	protected DataGridCollectionView ThemeColors { get; }
	private ChardonnayTheme ExistingTheme { get; } = ChardonnayTheme.GetLiveTheme();
	private ChardonnayTheme WorkingTheme { get; set; }

	public ThemePickerDialog()
	{
		InitializeComponent();
		CancelOnEscape = false;
		WorkingTheme = (ChardonnayTheme)ExistingTheme.Clone();
		ThemeColors = new(EnumerateThemeItemColors());

		DataContext = this;
		Closing += ThemePickerDialog_Closing;
	}

	private void ThemePickerDialog_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
	{
		if (!e.IsProgrammatic)
		{
			CancelAndClose();
			e.Cancel = true;
		}
	}

	protected async Task ImportTheme()
	{
		try
		{
			var openFileDialogOptions = new FilePickerOpenOptions
			{
				Title = $"Select the ChardonnayTheme.json file",
				AllowMultiple = false,
				FileTypeFilter =
				[
					new("JSON files (*.json)")
					{
						Patterns = ["*.json"],
						AppleUniformTypeIdentifiers  = ["public.json"]
					}
				]
			};

			var selectedFiles = await StorageProvider.OpenFilePickerAsync(openFileDialogOptions);
			var selectedFile = selectedFiles.SingleOrDefault()?.TryGetLocalPath();

			if (selectedFile is null) return;

			using (var theme = new ChardonnayThemePersister(selectedFile))
			{
				ResetTheme(theme.Target);
			}

			await MessageBox.Show(this, "Theme imported and applied", "Theme Imported");
		}
		catch (Exception ex)
		{
			await MessageBox.ShowAdminAlert(this, "Error attempting to import your chardonnay theme.", "Error Importing", ex);
		}
	}

	protected async Task ExportTheme()
	{
		try
		{
			var options = new FilePickerSaveOptions
			{
				Title = "Where to export Library",
				SuggestedFileName = $"ChardonnayTheme",
				DefaultExtension = "json",
				ShowOverwritePrompt = true,
				FileTypeChoices =
				[
					new("JSON files (*.json)")
					{
						Patterns = ["*.json"],
						AppleUniformTypeIdentifiers = ["public.json"]
					},
					new("All files (*.*)") { Patterns = ["*"] }
				]
			};

			var selectedFile = (await StorageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();

			if (selectedFile is null) return;

			using (var theme = new ChardonnayThemePersister(WorkingTheme, selectedFile))
				theme.Target.Save();

			await MessageBox.Show(this, "Theme exported to:\r\n" + selectedFile, "Theme Exported");
		}
		catch (Exception ex)
		{
			await MessageBox.ShowAdminAlert(this, "Error attempting to export your chardonnay theme.", "Error Exporting", ex);
		}
	}

	protected override void CancelAndClose()
	{
		ExistingTheme.ApplyTheme(ActualThemeVariant);
		base.CancelAndClose();
	}

	protected void ResetColors()
		=> ResetTheme(ExistingTheme);

	protected void LoadDefaultColors()
	{
		if (App.DefaultThemeColors is ChardonnayTheme defaults)
			ResetTheme(defaults);
	}

	protected override async Task SaveAndCloseAsync()
	{
		using (var themePersister = ChardonnayThemePersister.Create())
		{
			if (themePersister is null)
			{
				await MessageBox.Show(this, "Failed to save the theme.", "Error saving theme", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			else
			{
				foreach (var i in ThemeColors.OfType<ThemeItemColor>())
				{
					themePersister.Target.SetColor(ActualThemeVariant, i.ThemeItemName, i.ThemeColor);
				}
				themePersister.Target.Save();
			}
		}
		await base.SaveAndCloseAsync();
	}

	private void ResetTheme(ChardonnayTheme theme)
	{
		WorkingTheme = (ChardonnayTheme)theme.Clone();
		WorkingTheme.ApplyTheme(ActualThemeVariant);

		foreach (var i in ThemeColors.OfType<ThemeItemColor>())
		{
			i.ColorSetter = null;
			i.ThemeColor = WorkingTheme.GetColor(ActualThemeVariant, i.ThemeItemName);
			i.ColorSetter = ColorSetter;
		}
	}

	private IEnumerable<ThemeItemColor> EnumerateThemeItemColors()
		=> WorkingTheme
		.GetThemeColors(ActualThemeVariant)
		.Select(kvp => new ThemeItemColor
		{
			ThemeItemName = kvp.Key,
			ThemeColor = kvp.Value,
			ColorSetter = ColorSetter
		});

	private void ColorSetter(Color color, string colorName)
	{
		WorkingTheme.SetColor(ActualThemeVariant, colorName, color);
		WorkingTheme.ApplyTheme(ActualThemeVariant);
	}

	private class ThemeItemColor : ViewModels.ViewModelBase
	{
		public required string ThemeItemName { get; init; }
		public required Action<Color, string>? ColorSetter { get; set; }

		private Color _themeColor;
		public Color ThemeColor
		{
			get => _themeColor;
			set
			{
				var setColors = !_themeColor.Equals(value);
				this.RaiseAndSetIfChanged(ref _themeColor, value);
				if (setColors)
					ColorSetter?.Invoke(_themeColor, ThemeItemName);
			}
		}
	}
}
