using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using LibationAvalonia.Themes;
using LibationUiBase.Forms;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibationUiBase.Resources;

namespace LibationAvalonia.Dialogs;

public partial class ThemePickerDialog : DialogWindow
{
	public AvaloniaList<ThemeItemColor> ThemeColors { get; }
	private ChardonnayTheme ExistingTheme { get; } = ChardonnayTheme.GetLiveTheme();
	private ChardonnayTheme WorkingTheme { get; set; }

	/// <summary>The number of <see cref="Controls.ColorPickerExt"/> flyouts which are currently open</summary>
	private int OpenColorPickerFlyouts;

	public ThemePickerDialog()
	{
		InitializeComponent();
		CancelOnEscape = false;
		WorkingTheme = (ChardonnayTheme)ExistingTheme.Clone();
		ThemeColors = new(EnumerateThemeItemColors());

		DataContext = this;
		Closing += ThemePickerDialog_Closing;
		Closed += ThemePickerDialog_Closed;
	}

	private void ThemePickerDialog_Closing(object? sender, Avalonia.Controls.WindowClosingEventArgs e)
	{
		if (!e.IsProgrammatic)
		{
			CancelAndClose();
			e.Cancel = true;
		}
	}

	private void ThemePickerDialog_Closed(object? sender, EventArgs e)
		//This window's color pickers are gone, so any deferred reload is now safe to perform.
		=> ChardonnayTheme.ReloadDeferredFluentTheme();

	public async Task ImportTheme()
	{
		try
		{
			var openFileDialogOptions = new FilePickerOpenOptions
			{
				Title = Strings.ThemePickerDialog_SelectTheChardonnayThemeJsonFile,
				AllowMultiple = false,
				FileTypeFilter =
				[
					new(Strings.ThemePickerDialog_JSONFilesJson)
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

			await MessageBox.Show(this, Strings.ThemePickerDialog_ThemeImportedAndApplied, Strings.ThemePickerDialog_ThemeImported);
		}
		catch (Exception ex)
		{
			await MessageBox.ShowAdminAlert(this, Strings.ThemePickerDialog_ErrorAttemptingToImportYourChardonnay, Strings.ThemePickerDialog_ErrorImporting, ex);
		}
	}

	public async Task ExportTheme()
	{
		try
		{
			var options = new FilePickerSaveOptions
			{
				Title = Strings.ThemePickerDialog_WhereToExportLibrary,
				SuggestedFileName = $"ChardonnayTheme",
				DefaultExtension = "json",
				ShowOverwritePrompt = true,
				FileTypeChoices =
				[
					new(Strings.ThemePickerDialog_JSONFilesJson)
					{
						Patterns = ["*.json"],
						AppleUniformTypeIdentifiers = ["public.json"]
					},
					new(Strings.ThemePickerDialog_AllFiles) { Patterns = ["*"] }
				]
			};

			var selectedFile = (await StorageProvider.SaveFilePickerAsync(options))?.TryGetLocalPath();

			if (selectedFile is null) return;

			using (var theme = new ChardonnayThemePersister(WorkingTheme, selectedFile))
				theme.Target.Save();

			await MessageBox.Show(this, string.Format(Strings.ThemePickerDialog_ThemeExportedTo, selectedFile), Strings.ThemePickerDialog_ThemeExported);
		}
		catch (Exception ex)
		{
			await MessageBox.ShowAdminAlert(this, Strings.ThemePickerDialog_ErrorAttemptingToExportYourChardonnay, Strings.ThemePickerDialog_ErrorExporting, ex);
		}
	}

	public new void CancelAndClose()
	{
		ExistingTheme.ApplyTheme(ActualThemeVariant);
		base.CancelAndClose();
	}



	public void ResetColors()
		=> ResetTheme(ExistingTheme);

	public void LoadDefaultColors()
	{
		if (App.DefaultThemeColors is ChardonnayTheme defaults)
			ResetTheme(defaults);
	}

	public new async Task SaveAndCloseAsync()
	{
		using (var themePersister = ChardonnayThemePersister.Create())
		{
			if (themePersister is null)
			{
				await MessageBox.Show(this, Strings.ThemePickerDialog_FailedToSaveTheTheme, Strings.ThemePickerDialog_ErrorSavingTheme, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
		WorkingTheme.ApplyTheme(ActualThemeVariant, deferFluentReload: OpenColorPickerFlyouts > 0);
	}

	private void ColorPicker_FlyoutOpened(object? sender, EventArgs e)
		=> OpenColorPickerFlyouts++;

	private void ColorPicker_FlyoutClosed(object? sender, EventArgs e)
	{
		if (--OpenColorPickerFlyouts <= 0)
		{
			OpenColorPickerFlyouts = 0;
			ChardonnayTheme.ReloadDeferredFluentTheme();
		}
	}

	public class ThemeItemColor : ViewModels.ViewModelBase
	{
		public required string ThemeItemName { get; init; }
		public required Action<Color, string>? ColorSetter { get; set; }

		public Color ThemeColor
		{
			get => field;
			set
			{
				var setColors = !field.Equals(value);
				this.RaiseAndSetIfChanged(ref field, value);
				if (setColors)
					ColorSetter?.Invoke(field, ThemeItemName);
			}
		}
	}
}
