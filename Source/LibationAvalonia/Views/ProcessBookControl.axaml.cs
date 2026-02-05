using Avalonia;
using Avalonia.Controls;
using DataLayer;
using LibationUiBase;
using LibationUiBase.ProcessQueue;

namespace LibationAvalonia.Views;

public delegate void QueueItemPositionButtonClicked(ProcessBookViewModel? item, QueuePosition queueButton);
public delegate void QueueItemCancelButtonClicked(ProcessBookViewModel? item);
public partial class ProcessBookControl : UserControl
{
	public static event QueueItemPositionButtonClicked? PositionButtonClicked;
	public static event QueueItemCancelButtonClicked? CancelButtonClicked;

	public static readonly StyledProperty<ProcessBookStatus> ProcessBookStatusProperty =
		AvaloniaProperty.Register<ProcessBookControl, ProcessBookStatus>(nameof(ProcessBookStatus), enableDataValidation: true);

	public ProcessBookStatus ProcessBookStatus
	{
		get => GetValue(ProcessBookStatusProperty);
		set => SetValue(ProcessBookStatusProperty, value);
	}

	public ProcessBookControl()
	{
		InitializeComponent();

		if (Design.IsDesignMode)
		{
			ViewModels.MainVM.Configure_NonUI();
			DataContext = new ProcessBookViewModel(MockLibraryBook.CreateBook(), LibationFileManager.Configuration.Instance);
			return;
		}
	}

	private ProcessBookViewModel? DataItem => DataContext as ProcessBookViewModel;

	public void Cancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> CancelButtonClicked?.Invoke(DataItem);
	public void MoveFirst_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.First);
	public void MoveUp_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.OneUp);
	public void MoveDown_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.OneDown);
	public void MoveLast_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> PositionButtonClicked?.Invoke(DataItem, QueuePosition.Last);
}
