using Avalonia;
using Avalonia.Controls;

namespace LibationAvalonia.Controls;

/// <summary>Label and description on the left, the control on the right (or below when <see cref="Stacked"/>).</summary>
public partial class SettingRow : ContentControl
{
	public static readonly StyledProperty<string?> LabelProperty =
		AvaloniaProperty.Register<SettingRow, string?>(nameof(Label));
	public static readonly StyledProperty<string?> DescriptionProperty =
		AvaloniaProperty.Register<SettingRow, string?>(nameof(Description));
	public static readonly StyledProperty<bool> StackedProperty =
		AvaloniaProperty.Register<SettingRow, bool>(nameof(Stacked));

	public SettingRow()
	{
		InitializeComponent();
	}

	public string? Label { get => GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
	public string? Description { get => GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
	public bool Stacked { get => GetValue(StackedProperty); set => SetValue(StackedProperty, value); }
}
