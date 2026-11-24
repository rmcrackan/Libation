using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LibationAvalonia.Controls.Settings;

public partial class TemplateRow : UserControl
{
	public static readonly StyledProperty<string?> TemplateTextProperty =
		AvaloniaProperty.Register<TemplateRow, string?>(nameof(TemplateText));
	public static readonly StyledProperty<string?> ExampleProperty =
		AvaloniaProperty.Register<TemplateRow, string?>(nameof(Example));
	public static readonly RoutedEvent<RoutedEventArgs> EditClickEvent =
		RoutedEvent.Register<TemplateRow, RoutedEventArgs>(nameof(EditClick), RoutingStrategies.Bubble);

	public TemplateRow()
	{
		InitializeComponent();
	}

	public string? TemplateText { get => GetValue(TemplateTextProperty); set => SetValue(TemplateTextProperty, value); }
	public string? Example { get => GetValue(ExampleProperty); set => SetValue(ExampleProperty, value); }

	public event System.EventHandler<RoutedEventArgs> EditClick
	{
		add => AddHandler(EditClickEvent, value);
		remove => RemoveHandler(EditClickEvent, value);
	}

	private void EditButton_Click(object? sender, RoutedEventArgs e)
		=> RaiseEvent(new RoutedEventArgs(EditClickEvent));
}
