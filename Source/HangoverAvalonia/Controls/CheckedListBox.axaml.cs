using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using HangoverAvalonia.ViewModels;
using ReactiveUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HangoverAvalonia.Controls
{
	public partial class CheckedListBox : UserControl
	{
		public event EventHandler<ItemCheckEventArgs> ItemCheck;

		public static readonly StyledProperty<IEnumerable> ItemsProperty =
		AvaloniaProperty.Register<CheckedListBox, IEnumerable>(nameof(Items));

		public IEnumerable Items { get => GetValue(ItemsProperty); set => SetValue(ItemsProperty, value); }
		private CheckedListBoxViewModel _viewModel = new();

		public IEnumerable<object> CheckedItems =>
			_viewModel
			.CheckboxItems
			.Where(i => i.IsChecked)
			.Select(i => i.Item);

		public void SetItemChecked(int i, bool isChecked) => _viewModel.CheckboxItems[i].IsChecked = isChecked;
		public void SetItemChecked(object item, bool isChecked)
		{
			var obj = _viewModel.CheckboxItems.SingleOrDefault(i => i.Item == item);
			if (obj is not null)
				obj.IsChecked = isChecked;
		}

		public CheckedListBox()
		{
			InitializeComponent();
			scroller.DataContext = _viewModel;
			_viewModel.CheckedChanged += _viewModel_CheckedChanged;
		}

		private void _viewModel_CheckedChanged(object sender, CheckBoxViewModel e)
		{
			var args = new ItemCheckEventArgs { Item = e.Item, ItemIndex = _viewModel.CheckboxItems.IndexOf(e), IsChecked = e.IsChecked };
			ItemCheck?.Invoke(this, args);
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			if (change.Property.Name == nameof(Items) && Items != null)
				_viewModel.SetItems(Items);
			base.OnPropertyChanged(change);
		}

		public class CheckedListBoxViewModel : ViewModelBase
		{
			public event EventHandler<CheckBoxViewModel> CheckedChanged;
			public AvaloniaList<CheckBoxViewModel> CheckboxItems { get; private set; }

			public void SetItems(IEnumerable items)
			{
				UnsubscribeFromItems(CheckboxItems);
				CheckboxItems = new(items.OfType<object>().Select(o => new CheckBoxViewModel { Item = o }));
				SubscribeToItems(CheckboxItems);
				this.RaisePropertyChanged(nameof(CheckboxItems));
			}

			private void SubscribeToItems(IEnumerable objects)
			{
				foreach (var i in objects.OfType<INotifyPropertyChanged>())
					i.PropertyChanged += I_PropertyChanged;
			}

			private void UnsubscribeFromItems(AvaloniaList<CheckBoxViewModel> objects)
			{
				if (objects is null) return;

				foreach (var i in objects)
					i.PropertyChanged -= I_PropertyChanged;
			}
			private void I_PropertyChanged(object sender, PropertyChangedEventArgs e)
			{
				CheckedChanged?.Invoke(this, (CheckBoxViewModel)sender);
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

	public class ItemCheckEventArgs : EventArgs
	{
		public int ItemIndex { get; init; }
		public bool IsChecked { get; init; }
		public object Item { get; init; }
	}
}
