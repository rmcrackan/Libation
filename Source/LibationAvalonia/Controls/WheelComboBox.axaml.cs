using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using System;

namespace LibationAvalonia.Controls
{
	public partial class WheelComboBox : ComboBox, IStyleable
	{
		Type IStyleable.StyleKey => typeof(ComboBox);
		public WheelComboBox()
		{
			InitializeComponent();
		}
		protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
		{
			var dir = Math.Sign(e.Delta.Y);
			if (dir == 1 && SelectedIndex > 0)
			{
				SelectedIndex--;
				e.Handled = true;
			}
			else if (dir == -1 && SelectedIndex < ItemCount - 1)
			{
				SelectedIndex++;
				e.Handled = true;
			}

			base.OnPointerWheelChanged(e);
		}
	}
}
