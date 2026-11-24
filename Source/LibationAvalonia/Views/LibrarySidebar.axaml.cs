using Avalonia.Controls;
using LibationAvalonia.ViewModels;

namespace LibationAvalonia.Views;

public partial class LibrarySidebar : UserControl
{
	public LibrarySidebar()
	{
		InitializeComponent();
	}

	private async void Item_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		if (sender is not Control { DataContext: SidebarItem item })
			return;

		try
		{
			await item.Activate();
		}
		catch (System.Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Sidebar item {Label} failed", item.Label);
		}
	}
}
