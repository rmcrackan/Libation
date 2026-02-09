using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using LibationAvalonia.ViewModels;
using ReactiveUI;

namespace LibationAvalonia.Controls;

public partial class CheckedListBox : UserControl
{
	public static readonly StyledProperty<AvaloniaList<CheckBoxViewModel>> ItemsProperty =
	AvaloniaProperty.Register<CheckedListBox, AvaloniaList<CheckBoxViewModel>>(nameof(Items));

	public AvaloniaList<CheckBoxViewModel> Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }

	public CheckedListBox()
	{
		InitializeComponent();
	}
}

public class CheckBoxViewModel : ViewModelBase
{
	public bool IsChecked { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
	public object? Item { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
}
