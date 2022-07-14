using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LibationWinForms.AvaloniaUI.ViewModels.Dialogs;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{

	public enum DialogResult
	{
		None = 0,
		OK = 1,
		Cancel = 2,
		Abort = 3,
		Retry = 4,
		Ignore = 5,
		Yes = 6,
		No = 7,
		TryAgain = 10,
		Continue = 11
	}


	public enum MessageBoxIcon
	{
		None = 0,
		Error = 16,
		Hand = 16,
		Stop = 16,
		Question = 32,
		Exclamation = 48,
		Warning = 48,
		Asterisk = 64,
		Information = 64
	}
	public enum MessageBoxButtons
	{
		OK,
		OKCancel,
		AbortRetryIgnore,
		YesNoCancel,
		YesNo,
		RetryCancel,
		CancelTryContinue
	}

	public enum MessageBoxDefaultButton
	{
		Button1,
		Button2 = 256,
		Button3 = 512,
	}
	public partial class MessageBoxWindow : ReactiveWindow<MessageBoxViewModel>
	{
		public MessageBoxWindow()
		{
			InitializeComponent();
#if DEBUG
			this.AttachDevTools();
#endif
		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}

		public DialogResult DialogResult { get; private set; }

		public void Button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var vm = DataContext as MessageBoxViewModel;
			DialogResult = vm.Buttons switch
			{
				MessageBoxButtons.OK => DialogResult.OK,
				MessageBoxButtons.OKCancel => DialogResult.OK,
				MessageBoxButtons.AbortRetryIgnore => DialogResult.Abort,
				MessageBoxButtons.YesNoCancel => DialogResult.Yes,
				MessageBoxButtons.YesNo => DialogResult.Yes,
				MessageBoxButtons.RetryCancel => DialogResult.Retry,
				MessageBoxButtons.CancelTryContinue => DialogResult.Cancel,
				_ => DialogResult.None
			};
			Close(DialogResult);
		}
		public void Button2_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var vm = DataContext as MessageBoxViewModel;
			DialogResult = vm.Buttons switch
			{
				MessageBoxButtons.OKCancel => DialogResult.Cancel,
				MessageBoxButtons.AbortRetryIgnore => DialogResult.Retry,
				MessageBoxButtons.YesNoCancel => DialogResult.No,
				MessageBoxButtons.YesNo => DialogResult.No,
				MessageBoxButtons.RetryCancel => DialogResult.Cancel,
				MessageBoxButtons.CancelTryContinue => DialogResult.TryAgain,
				_ => DialogResult.None
			};
			Close(DialogResult);
		}
		public void Button3_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		{
			var vm = DataContext as MessageBoxViewModel;
			DialogResult = vm.Buttons switch
			{
				MessageBoxButtons.AbortRetryIgnore => DialogResult.Ignore,
				MessageBoxButtons.YesNoCancel => DialogResult.Cancel,
				MessageBoxButtons.CancelTryContinue => DialogResult.Continue,
				_ => DialogResult.None
			};
			Close(DialogResult);
		}
	}
}
