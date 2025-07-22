using LibationUiBase.ProcessQueue;
using System;
using System.Drawing;
using System.Windows.Forms;

#nullable enable
namespace LibationWinForms.ProcessQueue
{
	internal partial class ProcessBookControl : UserControl
	{
		private readonly int CancelBtnDistanceFromEdge;
		private readonly int ProgressBarDistanceFromEdge;
		private object? m_OldContext;

		private static Color FailedColor { get; } = Color.LightCoral;
		private static Color CancelledColor { get; } = Color.Khaki;
		private static Color QueuedColor { get; } = SystemColors.Control;
		private static Color SuccessColor { get; } = Color.PaleGreen;

		public ProcessBookControl()
		{
			InitializeComponent();
			remainingTimeLbl.Visible = false;
			progressBar1.Visible = false;
			etaLbl.Visible = false;

			CancelBtnDistanceFromEdge = Width - cancelBtn.Location.X;
			ProgressBarDistanceFromEdge = Width - progressBar1.Location.X - progressBar1.Width;
		}

		protected override void OnDataContextChanged(EventArgs e)
		{
			if (m_OldContext is ProcessBookViewModel oldContext)
				oldContext.PropertyChanged -= DataContext_PropertyChanged;

			if (DataContext is ProcessBookViewModel newContext)
			{
				m_OldContext = newContext;
				newContext.PropertyChanged += DataContext_PropertyChanged;
				DataContext_PropertyChanged(DataContext, new System.ComponentModel.PropertyChangedEventArgs(null));
			}

			base.OnDataContextChanged(e);
		}

		private void DataContext_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender is not ProcessBookViewModel vm)
				return;

			SuspendLayout();
			if (e.PropertyName is null or nameof(vm.Cover))
				SetCover(vm.Cover as Image);
			if (e.PropertyName is null or nameof(vm.Title) or nameof(vm.Author) or nameof(vm.Narrator))
				SetBookInfo($"{vm.Title}\r\nBy {vm.Author}\r\nNarrated by {vm.Narrator}");
			if (e.PropertyName is null or nameof(vm.Status) or nameof(vm.StatusText))
				SetStatus(vm.Status, vm.StatusText);
			if (e.PropertyName is null or nameof(vm.Progress))
				SetProgress(vm.Progress);
			if (e.PropertyName is null or nameof(vm.TimeRemaining))
				SetRemainingTime(vm.TimeRemaining);
			ResumeLayout();
		}

		private void SetCover(Image? cover) => pictureBox1.Image = cover;
		private void SetBookInfo(string title) => bookInfoLbl.Text = title;
		private void SetRemainingTime(TimeSpan remaining)
			=> remainingTimeLbl.Text = $"{remaining:mm\\:ss}";

		private void SetProgress(int progress)
		{
			//Disable slow fill
			//https://stackoverflow.com/a/5332770/3335599
			if (progress < progressBar1.Maximum)
				progressBar1.Value = progress + 1;
			progressBar1.Value = progress;
		}

		private void SetStatus(ProcessBookStatus status, string statusText)
		{
			Color backColor = status switch
			{
				ProcessBookStatus.Completed => SuccessColor,
				ProcessBookStatus.Cancelled => CancelledColor,
				ProcessBookStatus.Queued => QueuedColor,
				ProcessBookStatus.Working => QueuedColor,
				_ => FailedColor
			};

			cancelBtn.Visible = status is ProcessBookStatus.Queued or ProcessBookStatus.Working;
			moveLastBtn.Visible = status == ProcessBookStatus.Queued;
			moveDownBtn.Visible = status == ProcessBookStatus.Queued;
			moveUpBtn.Visible = status == ProcessBookStatus.Queued;
			moveFirstBtn.Visible = status == ProcessBookStatus.Queued;
			remainingTimeLbl.Visible = status == ProcessBookStatus.Working;
			progressBar1.Visible = status == ProcessBookStatus.Working;
			etaLbl.Visible = status == ProcessBookStatus.Working;
			statusLbl.Visible = status != ProcessBookStatus.Working;
			statusLbl.Text = statusText;
			BackColor = backColor;

			int deltaX = Width - cancelBtn.Location.X - CancelBtnDistanceFromEdge;

			if (status is ProcessBookStatus.Queued or ProcessBookStatus.Working && deltaX != 0)
			{
				//If the last book to occupy this control before resizing was not
				//queued, the buttons were not Visible so the Anchor property was
				//ignored. Manually resize and reposition everything

				cancelBtn.Location = new Point(cancelBtn.Location.X + deltaX, cancelBtn.Location.Y);
				moveFirstBtn.Location = new Point(moveFirstBtn.Location.X + deltaX, moveFirstBtn.Location.Y);
				moveUpBtn.Location = new Point(moveUpBtn.Location.X + deltaX, moveUpBtn.Location.Y);
				moveDownBtn.Location = new Point(moveDownBtn.Location.X + deltaX, moveDownBtn.Location.Y);
				moveLastBtn.Location = new Point(moveLastBtn.Location.X + deltaX, moveLastBtn.Location.Y);
				etaLbl.Location = new Point(etaLbl.Location.X + deltaX, etaLbl.Location.Y);
				remainingTimeLbl.Location = new Point(remainingTimeLbl.Location.X + deltaX, remainingTimeLbl.Location.Y);
				progressBar1.Width = Width - ProgressBarDistanceFromEdge - progressBar1.Location.X;
			}

			if (status == ProcessBookStatus.Working)
			{
				bookInfoLbl.Width = cancelBtn.Location.X - bookInfoLbl.Location.X - bookInfoLbl.Padding.Left + cancelBtn.Padding.Right;
			}
			else
			{
				bookInfoLbl.Width = moveLastBtn.Location.X - bookInfoLbl.Location.X - bookInfoLbl.Padding.Left + moveLastBtn.Padding.Right;
			}
		}

		public override string ToString() => bookInfoLbl.Text ?? "[NO TITLE]";
	}
}
