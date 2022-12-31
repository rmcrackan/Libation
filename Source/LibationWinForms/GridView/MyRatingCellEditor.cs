using DataLayer;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public partial class MyRatingCellEditor : UserControl, IDataGridViewEditingControl
	{
		private const string SOLID_STAR = "★";
		private const string HOLLOW_STAR = "☆";

		private Rating _rating;
		public Rating Rating { 
			get => _rating;
			set
			{
				_rating = value;
				int rating = 0;
				foreach (Label star in panelOverall.Controls)
					star.Tag = star.Text = _rating.OverallRating > rating++ ? SOLID_STAR : HOLLOW_STAR;

				rating = 0;
				foreach (Label star in panelPerform.Controls)
					star.Tag = star.Text = _rating.PerformanceRating > rating++ ? SOLID_STAR : HOLLOW_STAR;

				rating = 0;
				foreach (Label star in panelStory.Controls)
					star.Tag = star.Text = _rating.StoryRating > rating++ ? SOLID_STAR : HOLLOW_STAR;
			}
		}
		public MyRatingCellEditor()
		{
			InitializeComponent();
			this.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
		}

		private void Star_MouseEnter(object sender, EventArgs e)
		{
			var thisTbox = sender as Label;
			var panel = thisTbox.Parent as Panel;
			var star = SOLID_STAR;

			foreach (Label child in panel.Controls)
			{
				child.Text = star;
				if (child == thisTbox) star = HOLLOW_STAR;
			}
		}

		private void Star_MouseLeave(object sender, EventArgs e)
		{
			var thisTbox = sender as Label;
			var panel = thisTbox.Parent as Panel;

			//Artifically shrink rectangle to guarantee mouse is outside when exiting from the left (negative X)
			var clientPt = panel.PointToClient(MousePosition);
			var rect = new Rectangle(0, 0, panel.ClientRectangle.Width - 2, panel.ClientRectangle.Height);
			if (!rect.Contains(clientPt.X - 2, clientPt.Y))
			{
				//Restore defaults
				foreach (Label child in panel.Controls)
					child.Text = (string)child.Tag;
			}
		}

		private void Star_MouseClick(object sender, MouseEventArgs e)
		{
			var overall = Rating.OverallRating;
			var perform = Rating.PerformanceRating;
			var story = Rating.StoryRating;

			var thisTbox = sender as Label;
			var panel = thisTbox.Parent as Panel;

			int newRatingValue = 0;
			foreach (var child in panel.Controls)
			{
				newRatingValue++;
				if (child == thisTbox) break;
			}

			if (panel == panelOverall)
				overall = newRatingValue;
			else if (panel == panelPerform)
				perform = newRatingValue;
			else if (panel == panelStory)
				story = newRatingValue;

			if (overall + perform + story == 0f) return;

			var newRating = new Rating(overall, perform, story);

			if (newRating == Rating) return;

			Rating = newRating;
			EditingControlValueChanged = true;
			EditingControlDataGridView.NotifyCurrentCellDirty(true);
		}

		#region IDataGridViewEditingControl

		DataGridView dataGridView;
		private bool valueChanged = false;
		int rowIndex;

		public DataGridView EditingControlDataGridView { get => dataGridView; set => dataGridView = value; }
		public int EditingControlRowIndex { get => rowIndex; set => rowIndex = value; }
		public bool EditingControlValueChanged { get => valueChanged; set => valueChanged = value; }
		public object EditingControlFormattedValue { get => Rating; set => Rating = (Rating)value; }
		public Cursor EditingPanelCursor => base.Cursor;
		public bool RepositionEditingControlOnValueChange => false;

		public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			ForeColor = dataGridViewCellStyle.ForeColor;
			BackColor = dataGridViewCellStyle.BackColor;
		}

		public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
		{
			switch (keyData & Keys.KeyCode)
			{
				case Keys.Enter:
				case Keys.Escape:
					return true;
				default:
					return !dataGridViewWantsInputKey;
			}
		}

		public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context) => EditingControlFormattedValue;

		public void PrepareEditingControlForEdit(bool selectAll) { }

		#endregion
	}
}
