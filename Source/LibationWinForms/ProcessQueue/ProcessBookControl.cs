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
		public ProcessBookControl()
		{
			InitializeComponent();
			statusLbl.Text = "Queued";
			remainingTimeLbl.Visible = false;
			progressBar1.Visible = false;
			etaLbl.Visible = false;

			CancelBtnDistanceFromEdge = Width - cancelBtn.Location.X;
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
			progressBar1.Value = progress;
		}
		public void SetRemainingTime(TimeSpan remaining)
		{
			remainingTimeLbl.Text = $"{remaining:mm\\:ss}";
		}
		public void SetResult(ProcessBookResult result)
		{
			string statusText = default;
			switch (result)
			{
				case ProcessBookResult.Success:
					statusText = "Finished";
					Status = ProcessBookStatus.Completed;
					break;
				case ProcessBookResult.Cancelled:
					statusText = "Cancelled";
					Status = ProcessBookStatus.Cancelled;
					break;
				case ProcessBookResult.FailedRetry:
					statusText = "Queued";
					Status = ProcessBookStatus.Queued;
					break;
				case ProcessBookResult.FailedSkip:
					statusText = "Error, Skip";
					Status = ProcessBookStatus.Failed;
					break;
				case ProcessBookResult.FailedAbort:
					statusText = "Error, Abort";
					Status = ProcessBookStatus.Failed;
					break;
				case ProcessBookResult.ValidationFail:
					statusText = "Validate fail";
					Status = ProcessBookStatus.Failed;
					break;
				case ProcessBookResult.None:
					statusText = "UNKNOWN";
					Status = ProcessBookStatus.Failed;
					break;
			}

			SetStatus(Status, statusText);
		}
		public void SetStatus(ProcessBookStatus status, string statusText = null)
		{
			Color backColor = default;
			switch (status)
			{
				case ProcessBookStatus.Completed:
					backColor = Color.PaleGreen;
					Status = ProcessBookStatus.Completed;
					break;
				case ProcessBookStatus.Cancelled:
					backColor = Color.Khaki;
					Status = ProcessBookStatus.Cancelled;
					break;
				case ProcessBookStatus.Queued:
					backColor = SystemColors.Control;
					Status = ProcessBookStatus.Queued;
					break;
				case ProcessBookStatus.Working:
					backColor = SystemColors.Control;
					Status = ProcessBookStatus.Working;
					break;
				case ProcessBookStatus.Failed:
					backColor = Color.LightCoral;
					Status = ProcessBookStatus.Failed;
					break;
			}

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
				//If the last book to occupy this control before resizing was not queued,
				//the buttons were not Visible so the Anchor property was ignored.

				cancelBtn.Location = new Point(cancelBtn.Location.X + deltaX, cancelBtn.Location.Y);
				moveFirstBtn.Location = new Point(moveFirstBtn.Location.X + deltaX, moveFirstBtn.Location.Y);
				moveUpBtn.Location = new Point(moveUpBtn.Location.X + deltaX, moveUpBtn.Location.Y);
				moveDownBtn.Location = new Point(moveDownBtn.Location.X + deltaX, moveDownBtn.Location.Y);
				moveLastBtn.Location = new Point(moveLastBtn.Location.X + deltaX, moveLastBtn.Location.Y);
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
