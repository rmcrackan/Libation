using LibationUiBase;
using LibationUiBase.ProcessQueue;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

#nullable enable
namespace LibationWinForms.ProcessQueue;

internal partial class ProcessQueueControl : UserControl
{
	public ProcessQueueViewModel ViewModel { get; } = new();
	public ToolStripButton PopoutButton { get; } = new()
	{
		DisplayStyle = ToolStripItemDisplayStyle.Text,
		Name = nameof(PopoutButton),
		Text = "Pop Out",
		TextAlign = ContentAlignment.MiddleCenter,
		Alignment = ToolStripItemAlignment.Right,
		Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
	};

	public ProcessQueueControl()
	{
		InitializeComponent();

		statusStrip1.Items.Add(PopoutButton);

		virtualFlowControl2.ButtonClicked += VirtualFlowControl2_ButtonClicked;
		virtualFlowControl2.DataContext = ViewModel.Queue;

		ViewModel.PropertyChanged += ProcessQueue_PropertyChanged;
		ViewModel.LogEntries.CollectionChanged += LogEntries_CollectionChanged;
		ProcessQueue_PropertyChanged(this, new PropertyChangedEventArgs(null));
	}

	private void LogEntries_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
	{
		if (!IsDisposed && e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
		{
			foreach (var entry in e.NewItems?.OfType<LogEntry>() ?? [])
				logDGV.Rows.Add(entry.LogDate, entry.LogMessage);
		}
	}

	private async void cancelAllBtn_Click(object? sender, EventArgs e)
	{
		ViewModel.Queue.ClearQueue();
		if (ViewModel.Queue.Current is not null)
			await ViewModel.Queue.Current.CancelAsync();
	}

	private void btnClearFinished_Click(object? sender, EventArgs e)
	{
		ViewModel.Queue.ClearCompleted();
		if (!ViewModel.Running)
			runningTimeLbl.Text = string.Empty;
	}

	private void clearLogBtn_Click(object? sender, EventArgs e)
	{
		ViewModel.LogEntries.Clear();
		logDGV.Rows.Clear();
	}

	private void LogCopyBtn_Click(object? sender, EventArgs e)
	{
		string logText = string.Join("\r\n", logDGV.Rows.Cast<DataGridViewRow>().Select(r => $"{r.Cells[0].Value}\t{r.Cells[1].Value}"));
		Clipboard.SetDataObject(logText, false, 5, 150);
	}

	private void LogDGV_Resize(object? sender, EventArgs e)
	{
		logDGV.Columns[1].Width = logDGV.Width - logDGV.Columns[0].Width;
	}

	#region View-Model update event handling

	private void ProcessQueue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is null or nameof(ViewModel.QueuedCount))
		{
			queueNumberLbl.Text = ViewModel.QueuedCount.ToString();
			queueNumberLbl.Visible = ViewModel.QueuedCount > 0;
		}
		if (e.PropertyName is null or nameof(ViewModel.ErrorCount))
		{
			errorNumberLbl.Text = ViewModel.ErrorCount.ToString();
			errorNumberLbl.Visible = ViewModel.ErrorCount > 0;
		}
		if (e.PropertyName is null or nameof(ViewModel.CompletedCount))
		{
			completedNumberLbl.Text = ViewModel.CompletedCount.ToString();
			completedNumberLbl.Visible = ViewModel.CompletedCount > 0;
		}
		if (e.PropertyName is null or nameof(ViewModel.Progress))
		{
			toolStripProgressBar1.Maximum = ViewModel.Queue.Count;
			toolStripProgressBar1.Value = ViewModel.Queue.Completed.Count;
		}
		if (e.PropertyName is null or nameof(ViewModel.ProgressBarVisible))
		{
			toolStripProgressBar1.Visible = ViewModel.ProgressBarVisible;
		}
		if (e.PropertyName is null or nameof(ViewModel.RunningTime))
		{
			runningTimeLbl.Text = ViewModel.RunningTime;
		}
		if (e.PropertyName is null or nameof(ViewModel.SpeedLimit))
		{
			numericUpDown1.Value = ViewModel.SpeedLimit;
			numericUpDown1.Increment = ViewModel.SpeedLimitIncrement;
			numericUpDown1.DecimalPlaces = ViewModel.SpeedLimit >= 10 ? 0 : ViewModel.SpeedLimit >= 1 ? 1 : 2;
		}
	}

	/// <summary>
	/// View notified the model that a button was clicked
	/// </summary>
	/// <param name="sender">the <see cref="ProcessBookControl"/> whose button was clicked</param>
	/// <param name="buttonName">The name of the button clicked</param>
	private async void VirtualFlowControl2_ButtonClicked(object? sender, string buttonName)
	{
		if (sender is not ProcessBookControl control || control.DataContext is not ProcessBookViewModel item)
			return;

		try
		{
			if (buttonName is nameof(ProcessBookControl.cancelBtn))
			{
				await item.CancelAsync();
				ViewModel.Queue.RemoveQueued(item);
			}
			else
			{
				QueuePosition? position = buttonName switch
				{
					nameof(ProcessBookControl.moveFirstBtn) => QueuePosition.First,
					nameof(ProcessBookControl.moveUpBtn) => QueuePosition.OneUp,
					nameof(ProcessBookControl.moveDownBtn) => QueuePosition.OneDown,
					nameof(ProcessBookControl.moveLastBtn) => QueuePosition.Last,
					_ => null
				};

				if (position is not null)
					ViewModel.Queue.MoveQueuePosition(item, position.Value);
			}
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error handling button click from queued item");
		}
	}

	#endregion

	private void numericUpDown1_ValueChanged(object? sender, EventArgs e)
		=> ViewModel.SpeedLimit = numericUpDown1.Value;
}

public class NumericUpDownSuffix : NumericUpDown
{
	[Description("Suffix displayed after numeric value."), Category("Data")]
	[Browsable(true)]
	[EditorBrowsable(EditorBrowsableState.Always)]
	[DisallowNull]
	public string Suffix
	{
		get => _suffix;
		set
		{
			base.Text = string.IsNullOrEmpty(_suffix) ? base.Text : base.Text.Replace(_suffix, value);
			_suffix = value;
			ChangingText = true;
		}
	}
	private string _suffix = string.Empty;

	[AllowNull]
	public override string Text
	{
		get => string.IsNullOrEmpty(Suffix) ? base.Text : base.Text.Replace(Suffix, string.Empty);
		set
		{
			if (Value == Minimum)
				base.Text = "∞";
			else
				base.Text = value + Suffix;
		}
	}
}
