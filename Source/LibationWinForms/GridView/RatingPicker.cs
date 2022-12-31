using DataLayer;
using Mpeg4Lib.Boxes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public partial class RatingPicker : UserControl, IDataGridViewEditingControl
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
				foreach (Label star in panel1.Controls)
					star.Tag = star.Text = _rating.OverallRating > rating++ ? SOLID_STAR : HOLLOW_STAR;

				rating = 0;
				foreach (Label star in panel2.Controls)
					star.Tag = star.Text = _rating.PerformanceRating > rating++ ? SOLID_STAR : HOLLOW_STAR;

				rating = 0;
				foreach (Label star in panel3.Controls)
					star.Tag = star.Text = _rating.StoryRating > rating++ ? SOLID_STAR : HOLLOW_STAR;
			}
		}
		public RatingPicker()
		{
			InitializeComponent();
		}

		private void Star_MouseEnter(object sender, EventArgs e)
		{
			var thisTbox = sender as Label;
			var stackPanel = thisTbox.Parent as Panel;
			var star = SOLID_STAR;

			foreach (Label child in stackPanel.Controls)
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

			int newRating = 0;
			foreach (var child in panel.Controls)
			{
				newRating++;
				if (child == thisTbox) break;
			}

			if (panel == panel1)
				overall = newRating;
			else if (panel == panel2)
				perform = newRating;
			else if (panel == panel3)
				story = newRating;

			if (overall + perform + story == 0f) return;

			Rating = new Rating(overall, perform, story);
			EditingControlValueChanged = true;
			EditingControlDataGridView.NotifyCurrentCellDirty(true);
		}

		DataGridView dataGridView;
		private bool valueChanged = false;
		int rowIndex;

		#region IDataGridViewEditingControl
		public DataGridView EditingControlDataGridView { get => dataGridView; set => dataGridView = value; }
		public object EditingControlFormattedValue { get => Rating.ToStarString(); set { } }
		public int EditingControlRowIndex { get => rowIndex; set => rowIndex = value; }
		public bool EditingControlValueChanged { get => valueChanged; set => valueChanged = value; }

		public Cursor EditingPanelCursor => base.Cursor;

		public bool RepositionEditingControlOnValueChange => false;

		public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
		{
			this.Font = dataGridViewCellStyle.Font;
			this.ForeColor = dataGridViewCellStyle.ForeColor;
			this.BackColor = dataGridViewCellStyle.BackColor;
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
