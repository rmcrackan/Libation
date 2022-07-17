using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System;
using System.Collections;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Controls
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
				SelectedIndex--;
			else if (dir == -1 && SelectedIndex < ItemCount - 1)
				SelectedIndex++;

			base.OnPointerWheelChanged(e);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
