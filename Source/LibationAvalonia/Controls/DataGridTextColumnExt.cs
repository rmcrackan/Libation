using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace LibationAvalonia.Controls;
internal class DataGridTextColumnExt : DataGridTextColumn
{
	public static readonly StyledProperty<int> MaxLengthProperty =
	AvaloniaProperty.Register<DataGridTextColumnExt, int>(nameof(MaxLength));

	public int MaxLength
	{
		get => GetValue(MaxLengthProperty);
		set => SetValue(MaxLengthProperty, value);
	}

	protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
	{
		if (editingElement is TextBox textBox)
		{
			textBox.MaxLength = MaxLength;
		}
		return base.PrepareCellForEdit(editingElement, editingEventArgs);
	}
}
