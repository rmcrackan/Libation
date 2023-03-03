using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using LibationAvalonia.ViewModels;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Controls
{
	public partial class CheckedListBox : UserControl
	{
		public static readonly StyledProperty<AvaloniaList<CheckBoxViewModel>> ItemsProperty =
		AvaloniaProperty.Register<CheckedListBox, AvaloniaList<CheckBoxViewModel>>(nameof(Items));

		public AvaloniaList<CheckBoxViewModel> Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
		private CheckedListBoxViewModel _viewModel = new();	

		public CheckedListBox()
		{
			InitializeComponent();
			scroller.DataContext = _viewModel;
		}
		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			if (change.Property.Name == nameof(Items) && Items != null)
				_viewModel.CheckboxItems = Items;
			base.OnPropertyChanged(change);
		}

		private class CheckedListBoxViewModel : ViewModelBase
		{
			private AvaloniaList<CheckBoxViewModel> _checkboxItems;
			public AvaloniaList<CheckBoxViewModel> CheckboxItems { get => _checkboxItems; set => this.RaiseAndSetIfChanged(ref _checkboxItems, value); }
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
