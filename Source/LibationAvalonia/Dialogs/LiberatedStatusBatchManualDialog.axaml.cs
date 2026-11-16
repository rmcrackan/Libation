using DataLayer;
using System.Collections;
using System.Collections.Generic;
using LibationUiBase.Resources;

namespace LibationAvalonia.Dialogs;

public partial class LiberatedStatusBatchManualDialog : DialogWindow
{
	public class liberatedComboBoxItem
	{
		public LiberatedStatus Status { get; set; }
		public string? Text { get; set; }
		public override string? ToString() => Text;
	}

	public LiberatedStatus BookLiberatedStatus { get; private set; }

	private liberatedComboBoxItem? _selectedStatus;
	public object? SelectedItem
	{
		get => _selectedStatus;
		set
		{
			_selectedStatus = value as liberatedComboBoxItem;

			BookLiberatedStatus = _selectedStatus?.Status ?? default;
		}
	}

	public List<liberatedComboBoxItem> BookStatuses { get; } =
	[
		new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = Strings.LiberatedStatusBatchManualDialog_Downloaded },
		new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = Strings.LiberatedStatusBatchManualDialog_DownloadPending },
	];

	public LiberatedStatusBatchManualDialog(bool isPdf) : this()
	{
		if (isPdf)
			Title = Strings.LiberatedStatusBatchManualDialog_LiberatedStatusWhetherThePdfHas;
	}

	public LiberatedStatusBatchManualDialog()
	{
		InitializeComponent();
		SelectedItem = BookStatuses[0];
		DataContext = this;
		ControlToFocusOnShow = SaveButton;
	}

	public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
		=> SaveAndClose();
}
