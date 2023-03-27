using DataLayer;
using System.Collections;
using System.Collections.Generic;

namespace LibationAvalonia.Dialogs
{
	public partial class LiberatedStatusBatchManualDialog : DialogWindow
	{
		private class liberatedComboBoxItem
		{
			public LiberatedStatus Status { get; set; }
			public string Text { get; set; }
			public override string ToString() => Text;
		}

		public LiberatedStatus BookLiberatedStatus { get; private set; }

		private liberatedComboBoxItem _selectedStatus;
		public object SelectedItem
		{
			get => _selectedStatus;
			set
			{
				_selectedStatus = value as liberatedComboBoxItem;

				BookLiberatedStatus = _selectedStatus.Status;
			}
		}

		public IList BookStatuses { get; } = new List<liberatedComboBoxItem>
		{
			new liberatedComboBoxItem { Status = LiberatedStatus.Liberated, Text = "Downloaded" },
			new liberatedComboBoxItem { Status = LiberatedStatus.NotLiberated, Text = "Not Downloaded" },
		};

		public LiberatedStatusBatchManualDialog(bool isPdf) : this()
		{
			if (isPdf)
				this.Title = this.Title.Replace("book", "PDF");
		}

		public LiberatedStatusBatchManualDialog()
		{
			InitializeComponent();
			SelectedItem = BookStatuses[0] as liberatedComboBoxItem;
			DataContext = this;
		}

		public void SaveButton_Clicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
			=> SaveAndClose();
	}
}
