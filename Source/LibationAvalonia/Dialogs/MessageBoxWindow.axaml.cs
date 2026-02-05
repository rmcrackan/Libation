using LibationAvalonia.ViewModels.Dialogs;
using LibationUiBase.Forms;

namespace LibationAvalonia.Dialogs;


public partial class MessageBoxWindow : DialogWindow
{
	public MessageBoxWindow()
	{
		InitializeComponent();
	}
	public MessageBoxWindow(bool saveAndRestorePosition) : base(saveAndRestorePosition)
	{
		InitializeComponent();
	}

	protected override void CancelAndClose() => Close(DialogResult.None);

	protected override void SaveAndClose() { }

	public void Button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
	{
		var vm = DataContext as MessageBoxViewModel;
		var dialogResult = vm?.Buttons switch
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
		Close(dialogResult);
	}
	public void Button2_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
	{
		var vm = DataContext as MessageBoxViewModel;
		var dialogResult = vm?.Buttons switch
		{
			MessageBoxButtons.OKCancel => DialogResult.Cancel,
			MessageBoxButtons.AbortRetryIgnore => DialogResult.Retry,
			MessageBoxButtons.YesNoCancel => DialogResult.No,
			MessageBoxButtons.YesNo => DialogResult.No,
			MessageBoxButtons.RetryCancel => DialogResult.Cancel,
			MessageBoxButtons.CancelTryContinue => DialogResult.TryAgain,
			_ => DialogResult.None
		};
		Close(dialogResult);
	}
	public void Button3_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
	{
		var vm = DataContext as MessageBoxViewModel;
		var dialogResult = vm?.Buttons switch
		{
			MessageBoxButtons.AbortRetryIgnore => DialogResult.Ignore,
			MessageBoxButtons.YesNoCancel => DialogResult.Cancel,
			MessageBoxButtons.CancelTryContinue => DialogResult.Continue,
			_ => DialogResult.None
		};
		Close(dialogResult);
	}
}
