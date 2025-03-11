using Avalonia.Collections;
using Avalonia.Media;
using ReactiveUI;
using Avalonia.Styling;
using System;
using LibationAvalonia.Themes;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace LibationAvalonia.Dialogs;

public partial class ThemePickerDialog : DialogWindow
{
	protected DataGridCollectionView ThemeColors { get; }
	private ChardonnayTheme ExistingTheme { get; } = ChardonnayTheme.GetLiveTheme();

	public ThemePickerDialog() : base(saveAndRestorePosition: false)
	{
		InitializeComponent();
		CancelOnEscape = false;
		var workingTheme = (ChardonnayTheme)ExistingTheme.Clone();
		ThemeColors = new(EnumerateThemeItemColors(workingTheme, ActualThemeVariant));

		DataContext = this;
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
		theme.ApplyTheme(ActualThemeVariant);

		foreach (var i in ThemeColors.OfType<ThemeItemColor>())
		{
			i.SuppressSet = true;
			i.ThemeColor = theme.GetColor(ActualThemeVariant, i.ThemeItemName);
			i.SuppressSet = false;
		}
	}

	private static IEnumerable<ThemeItemColor> EnumerateThemeItemColors(ChardonnayTheme workingTheme, ThemeVariant themeVariant)
		=> workingTheme
		.GetThemeColors(themeVariant)
		.Select(kvp => new ThemeItemColor
		{
			ThemeItemName = kvp.Key,
			ThemeColor = kvp.Value,
			ColorSetter = c =>
			{
				workingTheme.SetColor(themeVariant, kvp.Key, c);
				workingTheme.ApplyTheme(themeVariant);
			}
		});

	private class ThemeItemColor : ViewModels.ViewModelBase
	{
		public bool SuppressSet { get; set; }
		public required string ThemeItemName { get; init; }
		public required Action<Color> ColorSetter { get; init; }

		private Color _themeColor;
		public Color ThemeColor
		{
			get => _themeColor;
			set
			{
				var setColors = !SuppressSet && !_themeColor.Equals(value);
				this.RaiseAndSetIfChanged(ref _themeColor, value);
				if (setColors)
					ColorSetter?.Invoke(_themeColor);
			}
		}
	}
}
