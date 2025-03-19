using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{
	internal partial class ProcessBookControl : UserControl
	{
		private static int ControlNumberCounter = 0;

		/// <summary>
		/// The contol's position within <see cref="VirtualFlowControl"/>
		/// </summary>
		public int ControlNumber { get; }
		private ProcessBookStatus Status { get; set; } = ProcessBookStatus.Queued;
		private readonly int CancelBtnDistanceFromEdge;
		private readonly int ProgressBarDistanceFromEdge;

		public static Color FailedColor = Color.LightCoral;
		public static Color CancelledColor = Color.Khaki;
		public static Color QueuedColor = SystemColors.Control;
		public static Color SuccessColor = Color.PaleGreen;

		public ProcessBookControl()
		{
			InitializeComponent();
			statusLbl.Text = "Queued";
			remainingTimeLbl.Visible = false;
			progressBar1.Visible = false;
			etaLbl.Visible = false;

			CancelBtnDistanceFromEdge = Width - cancelBtn.Location.X;
			ProgressBarDistanceFromEdge = Width - progressBar1.Location.X - progressBar1.Width;
			ControlNumber = ControlNumberCounter++;
		}

		public void SetCover(Image cover)
		{
			pictureBox1.Image = cover;
		}

		public void SetBookInfo(string title)
		{
			bookInfoLbl.Text = title;
		}

		public void SetProgrss(int progress)
		{
			//Disable slow fill
			//https://stackoverflow.com/a/5332770/3335599
			if (progress < progressBar1.Maximum)
				progressBar1.Value = progress + 1;
			progressBar1.Value = progress;
		}

		public void SetRemainingTime(TimeSpan remaining)
		{
			remainingTimeLbl.Text = $"{remaining:mm\\:ss}";
		}

		public void SetResult(ProcessBookResult result)
		{
			(string statusText, ProcessBookStatus status) = result switch
			{
				ProcessBookResult.Success => ("Finished", ProcessBookStatus.Completed),
				ProcessBookResult.Cancelled => ("Cancelled", ProcessBookStatus.Cancelled),
				ProcessBookResult.FailedRetry => ("Error, will retry later", ProcessBookStatus.Failed),
				ProcessBookResult.FailedSkip => ("Error, Skipping", ProcessBookStatus.Failed),
				ProcessBookResult.FailedAbort => ("Error, Abort", ProcessBookStatus.Failed),
				ProcessBookResult.ValidationFail => ("Validation fail", ProcessBookStatus.Failed),
				ProcessBookResult.LicenseDenied => ("License Denied", ProcessBookStatus.Failed),
				ProcessBookResult.LicenseDeniedPossibleOutage => ("Possible Service Interruption", ProcessBookStatus.Failed),
				_ => ("UNKNOWN", ProcessBookStatus.Failed),
			};

			SetStatus(status, statusText);
		}

		public void SetStatus(ProcessBookStatus status, string statusText = null)
		{
			Status = status;

			Color backColor = Status switch
			{
				ProcessBookStatus.Completed => SuccessColor,
				ProcessBookStatus.Cancelled => CancelledColor,
				ProcessBookStatus.Queued => QueuedColor,
				ProcessBookStatus.Working => QueuedColor,
				_ => FailedColor
			};

			SuspendLayout();

			cancelBtn.Visible = Status is ProcessBookStatus.Queued or ProcessBookStatus.Working;
			moveLastBtn.Visible = Status == ProcessBookStatus.Queued;
			moveDownBtn.Visible = Status == ProcessBookStatus.Queued;
			moveUpBtn.Visible = Status == ProcessBookStatus.Queued;
			moveFirstBtn.Visible = Status == ProcessBookStatus.Queued;
			remainingTimeLbl.Visible = Status == ProcessBookStatus.Working;
			progressBar1.Visible = Status == ProcessBookStatus.Working;
			etaLbl.Visible = Status == ProcessBookStatus.Working;
			statusLbl.Visible = Status != ProcessBookStatus.Working;
			statusLbl.Text = statusText ?? Status.ToString();
			BackColor = backColor;

			int deltaX = Width - cancelBtn.Location.X - CancelBtnDistanceFromEdge;

			if (Status is ProcessBookStatus.Queued or ProcessBookStatus.Working && deltaX != 0)
			{
				//If the last book to occupy this control before resizing was not
				//queued, the buttons were not Visible so the Anchor property was
				//ignored. Manually resize and reposition everyhting

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

			ResumeLayout();
		}

		public override string ToString()
		{
			return bookInfoLbl.Text ?? "[NO TITLE]";
		}
	}
}
