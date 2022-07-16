using Avalonia;
using Avalonia.Controls;
using System;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public abstract class DialogWindow : Window
	{
		public Control ControlToFocusOnShow { get; set; }
		public DialogWindow()
		{
			this.HideMinMaxBtns();
			this.KeyDown += DialogWindow_KeyDown;
			this.Opened += DialogWindow_Opened;

#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void DialogWindow_Opened(object sender, EventArgs e)
		{
			ControlToFocusOnShow?.Focus();
		}

		protected virtual void SaveAndClose() => Close(DialogResult.OK);
		protected virtual void CancelAndClose() => Close(DialogResult.Cancel);

		private void DialogWindow_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
		{
			if (e.Key == Avalonia.Input.Key.Escape)
				CancelAndClose();
			else if (e.Key == Avalonia.Input.Key.Return)
				SaveAndClose();
		}
	}
}
