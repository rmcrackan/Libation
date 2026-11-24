using Avalonia;
using Avalonia.Controls;

namespace LibationAvalonia.Controls;

/// <summary>Titled card of <see cref="SettingRow"/>s; each row draws its own top divider, clipped on the first.</summary>
public partial class SettingGroup : ItemsControl
{
	public static readonly StyledProperty<string?> HeaderProperty =
		AvaloniaProperty.Register<SettingGroup, string?>(nameof(Header));

	public SettingGroup()
	{
		InitializeComponent();
	}

	public string? Header { get => GetValue(HeaderProperty); set => SetValue(HeaderProperty, value); }
}
