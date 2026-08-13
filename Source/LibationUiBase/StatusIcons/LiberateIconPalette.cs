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
		Error = new SKColor(0xB2, 0x22, 0x22)
	};

	private static readonly LiberateIconPalette Dark = new()
	{
		IconFill = new SKColor(0xDC, 0xE0, 0xDF),
		Red = new SKColor(0x7D, 0x1F, 0x1F),
		Yellow = new SKColor(0x7D, 0x7D, 0x1F),
		Green = new SKColor(0x1F, 0x7D, 0x1F),
		Error = new SKColor(0x80, 0x27, 0x27)
	};
}
