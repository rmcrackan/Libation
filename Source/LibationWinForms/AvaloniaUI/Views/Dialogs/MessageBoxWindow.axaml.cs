using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LibationWinForms.AvaloniaUI.ViewModels.Dialogs;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{

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
