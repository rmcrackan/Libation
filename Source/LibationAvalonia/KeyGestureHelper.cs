using Avalonia.Input;
using LibationFileManager;

namespace LibationAvalonia;

/// <summary>Cross-platform keyboard modifier helpers. Use these instead of hardcoding Control/Alt/Meta per OS.</summary>
public static class KeyGestureHelper
{
	/// <summary>Primary "command" modifier: Control on Windows/Linux, Meta (Command) on macOS.</summary>
	public static KeyModifiers CommandModifier => Configuration.IsMacOs ? KeyModifiers.Meta : KeyModifiers.Control;

	/// <summary>Menu accelerator modifier that works on all platforms: Alt on Windows/Linux, Meta on macOS. Use (Alt|Meta) so one binding accepts either.</summary>
	public static KeyModifiers MenuModifier => KeyModifiers.Alt | KeyModifiers.Meta;
}
