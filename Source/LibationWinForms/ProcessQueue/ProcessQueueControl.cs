using LibationFileManager;
using LibationUiBase;
using LibationUiBase.ProcessQueue;
using System;
using System.Collections;
using System.Collections.Generic;
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

		var speedLimitMBps = Configuration.Instance.DownloadSpeedLimit / 1024m / 1024;
		numericUpDown1.Value = speedLimitMBps > numericUpDown1.Maximum || speedLimitMBps < numericUpDown1.Minimum ? 0 : speedLimitMBps;
		statusStrip1.Items.Add(PopoutButton);

		virtualFlowControl2.RequestData += VirtualFlowControl1_RequestData;
		virtualFlowControl2.ButtonClicked += VirtualFlowControl2_ButtonClicked;

		ViewModel.LogWritten += (_, text) => WriteLine(text);
		ViewModel.PropertyChanged += ProcessQueue_PropertyChanged;
		ViewModel.BookPropertyChanged += ProcessBook_PropertyChanged;
		Load += ProcessQueueControl_Load;
	}

	private void ProcessQueueControl_Load(object? sender, EventArgs e)
	{
		if (DesignMode) return;
		ProcessQueue_PropertyChanged(this, new PropertyChangedEventArgs(null));
	}

	public void WriteLine(string text)
	{
		if (!IsDisposed)
			logDGV.Rows.Add(DateTime.Now, text.Trim());
	}

	private async void cancelAllBtn_Click(object? sender, EventArgs e)
	{
		ViewModel.Queue.ClearQueue();
		if (ViewModel.Queue.Current is not null)
			await ViewModel.Queue.Current.CancelAsync();
		virtualFlowControl2.VirtualControlCount = ViewModel.Queue.Count;
		UpdateAllControls();
	}

	private void btnClearFinished_Click(object? sender, EventArgs e)
	{
		ViewModel.Queue.ClearCompleted();
		virtualFlowControl2.VirtualControlCount = ViewModel.Queue.Count;
		UpdateAllControls();

		if (!ViewModel.Running)
			runningTimeLbl.Text = string.Empty;
	}

	private void clearLogBtn_Click(object? sender, EventArgs e)
	{
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

	private void ProcessBook_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (sender is not ProcessBookViewModel pbvm)
			return;

		int index = ViewModel.Queue.IndexOf(pbvm);
		UpdateControl(index, e.PropertyName);
	}

	private void ProcessQueue_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName is null or nameof(ViewModel.QueuedCount))
		{
			queueNumberLbl.Text = ViewModel.QueuedCount.ToString();
			queueNumberLbl.Visible = ViewModel.QueuedCount > 0;
			virtualFlowControl2.VirtualControlCount = ViewModel.Queue.Count;
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
	}

	/// <summary>
	/// Index of the first <see cref="ProcessBookViewModel"/> visible in the <see cref="VirtualFlowControl"/>
	/// </summary>
	private int FirstVisible = 0;
	/// <summary>
	/// Number of <see cref="ProcessBookViewModel"/> visible in the <see cref="VirtualFlowControl"/>
	/// </summary>
	private int NumVisible = 0;
	/// <summary>
	/// Controls displaying the <see cref="ProcessBookViewModel"/> state, starting with <see cref="FirstVisible"/> 
	/// </summary>
	private IReadOnlyList<ProcessBookControl>? Panels;

	/// <summary>
	/// Updates the display of a single <see cref="ProcessBookControl"/> at <paramref name="queueIndex"/> within <see cref="Queue"/>
	/// </summary>
	/// <param name="queueIndex">index of the <see cref="ProcessBookViewModel"/> within the <see cref="Queue"/></param>
	/// <param name="propertyName">The nme of the property that needs updating. If null, all properties are updated.</param>
	private void UpdateControl(int queueIndex, string? propertyName = null)
	{
		try
		{
			int i = queueIndex - FirstVisible;

			if (Panels is null || i > NumVisible || i < 0) return;

			var proc = ViewModel.Queue[queueIndex];

			Invoke(() =>
			{
				Panels[i].SuspendLayout();
				if (propertyName is null or nameof(proc.Cover))
					Panels[i].SetCover(proc.Cover as Image);
				if (propertyName is null or nameof(proc.Title) or nameof(proc.Author) or nameof(proc.Narrator))
					Panels[i].SetBookInfo($"{proc.Title}\r\nBy {proc.Author}\r\nNarrated by {proc.Narrator}");
				if (propertyName is null or nameof(proc.Status) or nameof(proc.StatusText))
					Panels[i].SetStatus(proc.Status, proc.StatusText);
				if (propertyName is null or nameof(proc.Progress))
					Panels[i].SetProgress(proc.Progress);
				if (propertyName is null or nameof(proc.TimeRemaining))
					Panels[i].SetRemainingTime(proc.TimeRemaining);
				Panels[i].ResumeLayout();
			});
		}
		catch (Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error updating the queued item's display.");
		}
	}

	private void UpdateAllControls()
	{
		int numToShow = Math.Min(NumVisible, ViewModel.Queue.Count - FirstVisible);

		for (int i = 0; i < numToShow; i++)
			UpdateControl(FirstVisible + i);
	}

	/// <summary>
	/// View notified the model that a botton was clicked
	/// </summary>
	/// <param name="queueIndex">index of the <see cref="ProcessBookViewModel"/> within <see cref="Queue"/></param>
	/// <param name="panelClicked">The clicked control to update</param>
	private async void VirtualFlowControl2_ButtonClicked(int queueIndex, string buttonName, ProcessBookControl panelClicked)
	{
		try
		{
			var item = ViewModel.Queue[queueIndex];
			if (buttonName == nameof(panelClicked.cancelBtn))
			{
				if (item is not null)
				{
					await item.CancelAsync();
					if (ViewModel.Queue.RemoveQueued(item))
						virtualFlowControl2.VirtualControlCount = ViewModel.Queue.Count;
				}
			}
			else if (buttonName == nameof(panelClicked.moveFirstBtn))
			{
				ViewModel.Queue.MoveQueuePosition(item, QueuePosition.Fisrt);
				UpdateAllControls();
			}
			else if (buttonName == nameof(panelClicked.moveUpBtn))
			{
				ViewModel.Queue.MoveQueuePosition(item, QueuePosition.OneUp);
				UpdateControl(queueIndex);
				if (queueIndex > 0)
					UpdateControl(queueIndex - 1);
			}
			else if (buttonName == nameof(panelClicked.moveDownBtn))
			{
				ViewModel.Queue.MoveQueuePosition(item, QueuePosition.OneDown);
				UpdateControl(queueIndex);
				if (queueIndex + 1 < ViewModel.Queue.Count)
					UpdateControl(queueIndex + 1);
			}
			else if (buttonName == nameof(panelClicked.moveLastBtn))
			{
				ViewModel.Queue.MoveQueuePosition(item, QueuePosition.Last);
				UpdateAllControls();
			}
		}
		catch(Exception ex)
		{
			Serilog.Log.Logger.Error(ex, "Error handling button click from queued item");
		}
	}

	/// <summary>
	/// View needs updating
	/// </summary>
	private void VirtualFlowControl1_RequestData(int firstIndex, int numVisible, IReadOnlyList<ProcessBookControl> panelsToFill)
	{
		FirstVisible = firstIndex;
		NumVisible = numVisible;
		Panels = panelsToFill;
		UpdateAllControls();
	}

	#endregion

	private void numericUpDown1_ValueChanged(object? sender, EventArgs e)
	{
		var newValue = (long)(numericUpDown1.Value * 1024 * 1024);

		var config = Configuration.Instance;
		config.DownloadSpeedLimit = newValue;
		if (config.DownloadSpeedLimit > newValue)
			numericUpDown1.Value =
				numericUpDown1.Value == 0.01m ? config.DownloadSpeedLimit / 1024m / 1024
				: 0;

		numericUpDown1.Increment =
			numericUpDown1.Value > 100 ? 10
			: numericUpDown1.Value > 10 ? 1
			: numericUpDown1.Value > 1 ? 0.1m
			: 0.01m;

		numericUpDown1.DecimalPlaces =
			numericUpDown1.Value >= 10 ? 0
			: numericUpDown1.Value >= 1 ? 1
			: 2;
	}
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
