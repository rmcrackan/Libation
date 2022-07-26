using Avalonia;
using Avalonia.Controls;
using LibationFileManager;
using System;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs
{
	public abstract class DialogWindow : Window
	{
		public bool SaveAndRestorePosition { get; set; } = true;
		public Control ControlToFocusOnShow { get; set; }
		public DialogWindow()
		{
			this.HideMinMaxBtns();
			this.KeyDown += DialogWindow_KeyDown;
			this.Initialized += DialogWindow_Initialized;
			this.Opened += DialogWindow_Opened;
			this.Closing += DialogWindow_Closing;

#if DEBUG
			this.AttachDevTools();
#endif
		}
		public DialogWindow(bool saveAndRestorePosition) : this()
		{
			SaveAndRestorePosition = saveAndRestorePosition;
		}

		private void DialogWindow_Initialized(object sender, EventArgs e)
		{
			this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			if (SaveAndRestorePosition)
				this.RestoreSizeAndLocation(Configuration.Instance);
		}

		private void DialogWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (SaveAndRestorePosition)
				this.SaveSizeAndLocation(Configuration.Instance);
		}

		private void DialogWindow_Opened(object sender, EventArgs e)
		{
			ControlToFocusOnShow?.Focus();
		}

		protected virtual void SaveAndClose() => Close(DialogResult.OK);
		protected virtual Task SaveAndCloseAsync() => Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(SaveAndClose);
		protected virtual void CancelAndClose() => Close(DialogResult.Cancel);
		protected virtual Task CancelAndCloseAsync() => Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(CancelAndClose);

		private async void DialogWindow_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
		{
			if (e.Key == Avalonia.Input.Key.Escape)
				await CancelAndCloseAsync();
			else if (e.Key == Avalonia.Input.Key.Return)
				await SaveAndCloseAsync();
		}
	}
}
