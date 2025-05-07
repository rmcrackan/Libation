using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using LibationAvalonia.ViewModels;
using ReactiveUI;

namespace LibationAvalonia.Controls
{
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
		private bool _isChecked;
		public bool IsChecked { get => _isChecked; set => this.RaiseAndSetIfChanged(ref _isChecked, value); }
		private object _bookText;
		public object Item { get => _bookText; set => this.RaiseAndSetIfChanged(ref _bookText, value); }
	}
}
