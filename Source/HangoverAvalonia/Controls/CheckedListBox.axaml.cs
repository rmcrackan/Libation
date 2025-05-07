using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using HangoverAvalonia.ViewModels;

namespace HangoverAvalonia.Controls;

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
