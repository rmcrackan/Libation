using Avalonia;
using Avalonia.Controls;
using DataLayer;
using NPOI.POIFS.Storage;
using System.Linq;

namespace LibationAvalonia.Controls
{
	public partial class RatingBox : UserControl
	{
		private const string SOLID_STAR = "★";
		private const string HOLLOW_STAR = "☆";
		private static readonly char[] FIVE_STARS = { '★', '★', '★', '★', '★' };

		public static readonly StyledProperty<Rating> RatingProperty =
		AvaloniaProperty.Register<RatingBox, Rating>(nameof(Rating));

		public Rating Rating
		{
			get { return GetValue(RatingProperty); }
			set { SetValue(RatingProperty, value); }
		}

		protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
		{
			if (change.Property.Name == nameof(Rating) && Rating is not null)
			{
				tblockOverallRating.Text = StarRating((int)Rating.OverallRating);
				tblockPerformRating.Text = StarRating((int)Rating.PerformanceRating);
				tblockStoryRating.Text = StarRating((int)Rating.StoryRating);

				if (this.IsPointerOver)
					RatingBox_PointerEntered(this, null);
				else
					RatingBox_PointerExited(this, null);
			}
			base.OnPropertyChanged(change);
		}
		public RatingBox()
		{
			InitializeComponent();
			PointerEntered += RatingBox_PointerEntered;
			PointerExited += RatingBox_PointerExited;
		}

		private void RatingBox_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
		{
			tblockOverall.IsVisible = Rating?.OverallRating > 0;
			tblockPerform.IsVisible = Rating?.PerformanceRating > 0;
			tblockStory.IsVisible = Rating?.StoryRating > 0;
		}

		private void RatingBox_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
		{
			tblockOverall.IsVisible = true;
			tblockPerform.IsVisible = true;
			tblockStory.IsVisible = true;
		}

		private static string StarRating(int rating) => new string(FIVE_STARS, 0, rating);
		public void Panel_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
		{
			var panel = sender as Panel;
			var stackPanel = panel.Children.OfType<StackPanel>().Single();

			panel.Children.OfType<TextBlock>().Single().IsVisible = true;
			stackPanel.IsVisible = false;

			foreach (TextBlock child in stackPanel.Children)
				child.Text = HOLLOW_STAR;
		}

		public void Panel_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
		{
			var panel = sender as Panel;

			panel.Children.OfType<TextBlock>().Single().IsVisible = false;
			panel.Children.OfType<StackPanel>().Single().IsVisible = true;
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

			int newRating = 0;
			foreach (var tbox in stackPanel.Children)
			{
				newRating++;
				if (tbox == thisTbox) break;
			}

			var ratingName = ((Panel)stackPanel.Parent).Children.OfType<TextBlock>().Single().Name;

			if (ratingName == tblockOverallRating.Name)
				overall = newRating;
			else if (ratingName == tblockPerformRating.Name)
				perform = newRating;
			else if (ratingName == tblockStoryRating.Name)
				story = newRating;

			if (overall + perform + story == 0f) return;

			Rating = new Rating(overall, perform, story);
		}
	}
}
