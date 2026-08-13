using SkiaSharp;

namespace LibationUiBase.StatusIcons;

/// <summary>
/// The Liberate icon colors for one theme variant. These mirror the brushes both UIs' themes
/// declare, so the generated icons match the surrounding chrome.
/// </summary>
internal readonly struct LiberateIconPalette
{
	public required SKColor IconFill { get; init; }
	public required SKColor Red { get; init; }
	public required SKColor Yellow { get; init; }
	public required SKColor Green { get; init; }
	public required SKColor Error { get; init; }

	/// <summary>The Audible Plus badge's circle, and the plus drawn on top of it.</summary>
	public required SKColor PlusBadge { get; init; }
	public required SKColor PlusBadgeGlyph { get; init; }

	public SKColor Lamp(StoplightLamp lamp) => lamp switch
	{
		StoplightLamp.Red => Red,
		StoplightLamp.Yellow => Yellow,
		StoplightLamp.Green => Green,
		_ => IconFill
	};

	public static LiberateIconPalette For(bool isDark) => isDark ? Dark : Light;

	private static readonly LiberateIconPalette Light = new()
	{
		IconFill = new SKColor(0x23, 0x1F, 0x20),
		Red = new SKColor(0xF0, 0x60, 0x60),
		Yellow = new SKColor(0xF0, 0xE1, 0x60),
		Green = new SKColor(0x70, 0xFA, 0x70),
		//FireBrick
		Error = new SKColor(0xB2, 0x22, 0x22),
		PlusBadge = new SKColor(0xE8, 0x72, 0x0C),
		PlusBadgeGlyph = SKColors.White
	};

	private static readonly LiberateIconPalette Dark = new()
	{
		IconFill = new SKColor(0xDC, 0xE0, 0xDF),
		Red = new SKColor(0x7D, 0x1F, 0x1F),
		Yellow = new SKColor(0x7D, 0x7D, 0x1F),
		Green = new SKColor(0x1F, 0x7D, 0x1F),
		Error = new SKColor(0x80, 0x27, 0x27),
		//Unlike the lamps, this is not muted for dark mode: the plus drawn on it is black, so the
		//circle has to stay bright enough to read against.
		PlusBadge = new SKColor(0xF0, 0x91, 0x3C),
		PlusBadgeGlyph = SKColors.Black
	};
}
