using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
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
		}
		protected override void OnPointerEnter(PointerEventArgs e)
		{
			this.Cursor = HandCursor;
			base.OnPointerEnter(e);
		}
		protected override void OnPointerLeave(PointerEventArgs e)
		{
			this.Cursor = Cursor.Default;
			base.OnPointerLeave(e);
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}
	}
}
