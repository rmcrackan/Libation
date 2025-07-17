using LibationUiBase.ProcessQueue;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue
{
	internal partial class ProcessBookControl : UserControl
	{
		private readonly int CancelBtnDistanceFromEdge;
		private readonly int ProgressBarDistanceFromEdge;

		private static Color FailedColor { get; } = Color.LightCoral;
		private static Color CancelledColor { get; } = Color.Khaki;
		private static Color QueuedColor { get; } = SystemColors.Control;
		private static Color SuccessColor { get; } = Color.PaleGreen;

		private ProcessBookViewModelBase m_Context;
		public ProcessBookViewModelBase Context
		{
			get => m_Context;
			set
			{
				if (m_Context != value)
				{
					OnContextChanging();
					m_Context = value;
					OnContextChanged();
				}
			}
		}

		public ProcessBookControl()
		{
			InitializeComponent();
			remainingTimeLbl.Visible = false;
			progressBar1.Visible = false;
			etaLbl.Visible = false;

			CancelBtnDistanceFromEdge = Width - cancelBtn.Location.X;
			ProgressBarDistanceFromEdge = Width - progressBar1.Location.X - progressBar1.Width;
		}

		private void OnContextChanging()
		{
			if (Context is not null)
				Context.PropertyChanged -= Context_PropertyChanged;
		}

		private void OnContextChanged()
		{
			Context.PropertyChanged += Context_PropertyChanged;
			Context_PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(null));
		}

		private void Context_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			SuspendLayout();
			if (e.PropertyName is null or nameof(Context.Cover))
				SetCover(Context.Cover as Image);
			if (e.PropertyName is null or nameof(Context.Title) or nameof(Context.Author) or nameof(Context.Narrator))
				SetBookInfo($"{Context.Title}\r\nBy {Context.Author}\r\nNarrated by {Context.Narrator}");
			if (e.PropertyName is null or nameof(Context.Status) or nameof(Context.StatusText))
				SetStatus(Context.Status, Context.StatusText);
			if (e.PropertyName is null or nameof(Context.Progress))
				SetProgress(Context.Progress);
			if (e.PropertyName is null or nameof(Context.TimeRemaining))
				SetRemainingTime(Context.TimeRemaining);
			ResumeLayout();
		}

		private void SetCover(Image cover) => pictureBox1.Image = cover;
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
