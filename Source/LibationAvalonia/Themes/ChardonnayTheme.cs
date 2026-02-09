using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Dinah.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace LibationAvalonia;

public class ChardonnayTheme : IUpdatable, ICloneable
{
	public event EventHandler? Updated;

	/// <summary>Theme color overrides</summary>
	[JsonProperty]
	private readonly Dictionary<ThemeVariant, Dictionary<string, Color>> ThemeColors;

	/// <summary>The two theme variants supported by Fluent themes</summary>
	private static readonly FrozenSet<ThemeVariant> FluentVariants = [ThemeVariant.Light, ThemeVariant.Dark];

	/// <summary>Reusable color pallets for the two theme variants</summary>
	private static readonly FrozenDictionary<ThemeVariant, ColorPaletteResources> ColorPalettes
		= FluentVariants.ToFrozenDictionary(t => t, _ => new ColorPaletteResources());

	private ChardonnayTheme()
	{
		ThemeColors = FluentVariants.ToDictionary(t => t, _ => new Dictionary<string, Color>());
	}

	/// <summary> Invoke <see cref="IUpdatable.Updated"/> </summary>
	public void Save() => Updated?.Invoke(this, EventArgs.Empty);

	public Color GetColor(LibationFileManager.Configuration.Theme themeVariant, string itemName)
		=> GetColor(FromVariantName(themeVariant), itemName);

	public Color GetColor(ThemeVariant themeVariant, string itemName)
	{
		ValidateThemeVariant(themeVariant);
		return ThemeColors[themeVariant].TryGetValue(itemName, out var color) ? color : default;
	}

	public ChardonnayTheme SetColor(LibationFileManager.Configuration.Theme themeVariant, Expression<Func<ColorPaletteResources, Color>> colorSelector, Color color)
		=> SetColor(FromVariantName(themeVariant), colorSelector, color);

	public ChardonnayTheme SetColor(ThemeVariant themeVariant, Expression<Func<ColorPaletteResources, Color>> colorSelector, Color color)
	{
		if (colorSelector.Body.NodeType is ExpressionType.MemberAccess &&
			colorSelector.Body is MemberExpression memberExpression &&
			memberExpression.Member is PropertyInfo colorProperty &&
			colorProperty.DeclaringType == typeof(ColorPaletteResources))
			return SetColor(themeVariant, colorProperty.Name, color);
		return this;
	}

	public ChardonnayTheme SetColor(LibationFileManager.Configuration.Theme themeVariant, string itemName, Color itemColor)
		=> SetColor(FromVariantName(themeVariant), itemName, itemColor);

	public ChardonnayTheme SetColor(ThemeVariant themeVariant, string itemName, Color itemColor)
	{
		ValidateThemeVariant(themeVariant);
		ThemeColors[themeVariant][itemName] = itemColor;
		return this;
	}

	public FrozenDictionary<string, Color> GetThemeColors(LibationFileManager.Configuration.Theme themeVariant)
		=> GetThemeColors(FromVariantName(themeVariant));

	public FrozenDictionary<string, Color> GetThemeColors(ThemeVariant themeVariant)
	{
		ValidateThemeVariant(themeVariant);
		return ThemeColors[themeVariant].ToFrozenDictionary();
	}

	public void ApplyTheme(LibationFileManager.Configuration.Theme themeVariant)
		=> ApplyTheme(FromVariantName(themeVariant));

	public void ApplyTheme(ThemeVariant themeVariant)
	{
		App.Current.RequestedThemeVariant = themeVariant;
		themeVariant = App.Current.ActualThemeVariant;
		ValidateThemeVariant(themeVariant);

		bool fluentColorChanged = false;

		//Set the Libation-specific brushes
		var themeBrushes = (ResourceDictionary)App.Current.Resources.ThemeDictionaries[themeVariant];
		foreach (var colorName in themeBrushes.Keys.OfType<string>())
		{
			if (ThemeColors[themeVariant].TryGetValue(colorName, out var color) && color != default)
			{
				if (themeBrushes[colorName] is ISolidColorBrush brush && brush.Color != color)
					themeBrushes[colorName] = new SolidColorBrush(color);
			}
		}

		//Set the fluent theme colors
		foreach (var p in GetColorResourceProperties())
		{
			if (ThemeColors[themeVariant].TryGetValue(p.Name, out var color) && color != default)
			{
				if (p.GetValue(ColorPalettes[themeVariant]) is not Color c || c != color)
				{
					p.SetValue(ColorPalettes[themeVariant], color);
					fluentColorChanged = true;
				}
			}
		}

		if (fluentColorChanged)
		{
			var oldFluent = App.Current.Styles.OfType<FluentTheme>().Single();
			App.Current.Styles.Remove(oldFluent);

			//We must make a new fluent theme and add it to the app for
			//the changes to the ColorPaletteResources to take effect.
			//Changes to the Libation-specific resources are instant.
			var newFluent = new FluentTheme();

			foreach (var kvp in ColorPalettes)
				newFluent.Palettes[kvp.Key] = kvp.Value;

			App.Current.Styles.Add(newFluent);
		}
	}

	/// <summary> Get the currently-active theme colors. </summary>
	public static ChardonnayTheme GetLiveTheme()
	{
		var theme = new ChardonnayTheme();

		foreach (var themeVariant in FluentVariants)
		{
			//Get the Libation-specific brushes
			var themeBrushes = (ResourceDictionary)App.Current.Resources.ThemeDictionaries[themeVariant];
			foreach (var colorName in themeBrushes.Keys.OfType<string>())
			{
				if (themeBrushes[colorName] is ISolidColorBrush brush)
				{
					//We're only working with colors, so convert the Brush's opacity to an alpha value
					var color = Color.FromArgb
						(
							(byte)Math.Round(brush.Color.A * brush.Opacity, 0),
							brush.Color.R,
							brush.Color.G,
							brush.Color.B
						);

					theme.ThemeColors[themeVariant][colorName] = color;
				}
			}

			//Get the fluent theme colors
			foreach (var p in GetColorResourceProperties())
			{
				var color = (Color)p.GetValue(ColorPalettes[themeVariant])!;

				//The color isn't being overridden, so get the static resource value.
				if (color == default)
				{
					var staticResourceName = p.Name == nameof(ColorPaletteResources.RegionColor) ? "SystemRegionColor" : $"System{p.Name}Color";
					if (App.Current.TryGetResource(staticResourceName, themeVariant, out var colorObj) && colorObj is Color c)
						color = c;
				}

				theme.ThemeColors[themeVariant][p.Name] = color;
			}
		}
		return theme;
	}

	public object Clone()
	{
		var clone = new ChardonnayTheme();
		foreach (var t in ThemeColors)
		{
			clone.ThemeColors[t.Key] = t.Value.ToDictionary();
		}
		return clone;
	}

	private static IEnumerable<PropertyInfo> GetColorResourceProperties()
		=> typeof(ColorPaletteResources).GetProperties().Where(p => p.PropertyType == typeof(Color));

	[System.Diagnostics.StackTraceHidden]
	private static void ValidateThemeVariant(ThemeVariant themeVariant)
	{
		if (!FluentVariants.Contains(themeVariant))
			throw new InvalidOperationException("FluentTheme.Palettes only supports Light and Dark variants.");
	}

	private static ThemeVariant FromVariantName(LibationFileManager.Configuration.Theme variantName)
		=> variantName switch
		{
			LibationFileManager.Configuration.Theme.Dark => ThemeVariant.Dark,
			LibationFileManager.Configuration.Theme.Light => ThemeVariant.Light,
			// "System"
			_ => ThemeVariant.Default
		};
}
