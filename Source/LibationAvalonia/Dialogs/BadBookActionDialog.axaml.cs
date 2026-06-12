using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using LibationUiBase;
using LibationUiBase.Forms;
using System.Threading.Tasks;

namespace LibationAvalonia.Dialogs;

public partial class BadBookActionDialog : DialogWindow
{
	public BadBookDialogResult Result { get; private set; } = new(DialogResult.Retry, false, false);

	public BadBookActionDialog()
	{
		InitializeComponent();
		SaveOnEnter = false;
		CancelOnEscape = false;
	}

	public BadBookActionDialog(string message, string caption) : this()
	{
		Title = caption;
		messageTextBlock.Text = message;
		ControlToFocusOnShow = retryButton;
	}

	private void CloseWith(DialogResult action)
	{
		Result = new BadBookDialogResult(
			action,
			applyToAllCheckBox.IsChecked == true,
			rememberInSettingsCheckBox.IsChecked == true);
		Close(DialogResult.None);
	}

	public void AbortButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		=> CloseWith(DialogResult.Abort);

	public void RetryButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		=> CloseWith(DialogResult.Retry);

	public void IgnoreButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs args)
		=> CloseWith(DialogResult.Ignore);

	public static Task<BadBookDialogResult> ShowAsync(Window? owner, string message, string caption)
		=> Dispatcher.UIThread.InvokeAsync(async () =>
		{
			owner = owner?.IsLoaded is true ? owner : null;
			var dialog = new BadBookActionDialog(message, caption);
			await DisplayDialogAsync(dialog, owner);
			return dialog.Result;
		});

	private static async Task DisplayDialogAsync(BadBookActionDialog dialog, Window? owner)
	{
		if (owner is null)
		{
			if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				if (desktop.MainWindow?.IsLoaded is true)
					await dialog.ShowDialog(desktop.MainWindow);
				else
				{
					var tcs = new TaskCompletionSource();
					desktop.MainWindow = dialog;
					dialog.Closed += (_, _) => tcs.SetResult();
					dialog.Show();
					await tcs.Task;
				}
			}
			else
			{
				var window = new Window
				{
					IsVisible = false,
					Height = 1,
					Width = 1,
					WindowDecorations = WindowDecorations.None,
					ShowInTaskbar = false
				};

				window.Show();
				await dialog.ShowDialog(window);
				window.Close();
			}
		}
		else
		{
			await dialog.ShowDialog(owner);
		}
	}
}
