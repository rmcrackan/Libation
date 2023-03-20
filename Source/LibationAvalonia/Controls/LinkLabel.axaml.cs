using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System;

namespace LibationAvalonia.Controls
{
	public partial class LinkLabel : TextBlock, IStyleable
	{
		Type IStyleable.StyleKey => typeof(TextBlock);
		private static readonly Cursor HandCursor = new Cursor(StandardCursorType.Hand);
		public LinkLabel()
		{
			InitializeComponent();
			Tapped += LinkLabel_Tapped;
		}

		private void LinkLabel_Tapped(object sender, TappedEventArgs e)
		{
			Foreground = Brushes.Purple;
		}

		protected override void OnPointerEntered(PointerEventArgs e)
		{
			this.Cursor = HandCursor;
			base.OnPointerEntered(e);
		}
		protected override void OnPointerExited(PointerEventArgs e)
		{
			this.Cursor = Cursor.Default;
			base.OnPointerExited(e);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
