using Avalonia;
using Avalonia.Controls;
using DataLayer;
using System.Linq;

namespace LibationAvalonia.Controls
{
	public partial class MyRatingCellEditor : UserControl
	{
		private const string SOLID_STAR = "★";
		private const string HOLLOW_STAR = "☆";

		public static readonly StyledProperty<Rating> RatingProperty =
		AvaloniaProperty.Register<MyRatingCellEditor, Rating>(nameof(Rating));

		public bool IsEditingMode { get; set; }
		public Rating Rating
		{
			get { return GetValue(RatingProperty); }
			set { SetValue(RatingProperty, value); }
		}
		public MyRatingCellEditor()
		{
			InitializeComponent();
			if (Design.IsDesignMode)
				Rating = new Rating(5, 4, 3);
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			if (change.Property.Name == nameof(Rating) && Rating is not null)
			{
				var blankValue = IsEditingMode ? HOLLOW_STAR : string.Empty;

				int rating = 0;
				foreach (TextBlock star in panelOverall.Children)
					star.Tag = star.Text = Rating.OverallRating > rating++ ? SOLID_STAR : blankValue;

				rating = 0;
				foreach (TextBlock star in panelPerform.Children)
					star.Tag = star.Text = Rating.PerformanceRating > rating++ ? SOLID_STAR : blankValue;

				rating = 0;
				foreach (TextBlock star in panelStory.Children)
					star.Tag = star.Text = Rating.StoryRating > rating++ ? SOLID_STAR : blankValue;

				SetVisible(IsEditingMode);
			}
			base.OnPropertyChanged(change);
		}

		private void SetVisible(bool allVisible)
		{
			tblockOverall.IsVisible = panelOverall.IsVisible = allVisible || Rating?.OverallRating > 0;
			tblockPerform.IsVisible = panelPerform.IsVisible = allVisible || Rating?.PerformanceRating > 0;
			tblockStory.IsVisible = panelStory.IsVisible = allVisible || Rating?.StoryRating > 0;
		}

		public void Panel_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
		{
			var panel = sender as Panel;
			var stackPanel = panel.Children.OfType<StackPanel>().Single();

			//Restore defaults
			foreach (TextBlock child in stackPanel.Children)
				child.Text = (string)child.Tag;
		}

		public void Star_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
		{
			var thisTbox = sender as TextBlock;
			var stackPanel = thisTbox.Parent as StackPanel;
			var star = SOLID_STAR;

			foreach (TextBlock child in stackPanel.Children)
			{
				child.Text = star;
				if (child == thisTbox) star = HOLLOW_STAR;
			}
		}

		public void Star_Tapped(object sender, Avalonia.Input.TappedEventArgs e)
		{
			var overall = Rating.OverallRating;
			var perform = Rating.PerformanceRating;
			var story = Rating.StoryRating;

			var thisTbox = sender as TextBlock;
			var stackPanel = thisTbox.Parent as StackPanel;

			int newRatingValue = 0;
			foreach (var tbox in stackPanel.Children)
			{
				newRatingValue++;
				if (tbox == thisTbox) break;
			}

			if (stackPanel == panelOverall)
				overall = newRatingValue;
			else if (stackPanel == panelPerform)
				perform = newRatingValue;
			else if (stackPanel == panelStory)
				story = newRatingValue;

			if (overall + perform + story == 0f) return;

			Rating = new Rating(overall, perform, story);
		}
	}
}
