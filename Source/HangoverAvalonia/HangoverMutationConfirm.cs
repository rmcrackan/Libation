using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using HangoverBase;
using System.Threading.Tasks;

namespace HangoverAvalonia;

internal static class HangoverMutationConfirm
{
	public static async Task<bool> ConfirmAsync(Window owner, string actionDescription)
	{
		bool? result = null;

		var yesButton = new Button { Content = "Yes", MinWidth = 75 };
		var noButton = new Button { Content = "No", MinWidth = 75 };

		var buttonPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal,
			HorizontalAlignment = HorizontalAlignment.Right,
			Spacing = 10,
			Margin = new Thickness(0, 15, 0, 0),
			Children = { noButton, yesButton },
		};

		var dialog = new Window
		{
			Title = HangoverDbMutation.ConfirmTitle,
			Width = 420,
			SizeToContent = SizeToContent.Height,
			WindowStartupLocation = WindowStartupLocation.CenterOwner,
			CanResize = false,
			Content = new Grid
			{
				RowDefinitions = new RowDefinitions("*,Auto"),
				Margin = new Thickness(10),
				Children =
				{
					new TextBlock
					{
						[Grid.RowProperty] = 0,
						Text = HangoverDbMutation.BuildConfirmMessage(actionDescription),
						TextWrapping = TextWrapping.Wrap,
					},
					buttonPanel,
				},
			},
		};

		Grid.SetRow(buttonPanel, 1);

		yesButton.Click += (_, _) => { result = true; dialog.Close(); };
		noButton.Click += (_, _) => { result = false; dialog.Close(); };
		dialog.Closed += (_, _) => result ??= false;

		await dialog.ShowDialog(owner);
		return result == true;
	}
}
