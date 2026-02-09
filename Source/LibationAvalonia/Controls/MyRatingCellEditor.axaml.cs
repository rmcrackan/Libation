using Avalonia;
using Avalonia.Controls;
using DataLayer;
using ReactiveUI;
using System;
using System.Linq;

namespace LibationAvalonia.Controls;

public partial class MyRatingCellEditor : UserControl
{
	private const string SOLID_STAR = "★";
	private const string HOLLOW_STAR = "☆";
	private const string HALF_STAR = "½";

	public static readonly StyledProperty<Rating> RatingProperty =
	AvaloniaProperty.Register<MyRatingCellEditor, Rating>(nameof(Rating));

	public bool IsEditingMode { get; set; }
	public Rating Rating { get => GetValue(RatingProperty); set => SetValue(RatingProperty, value); }

	public MyRatingCellEditor()
	{
		InitializeComponent();

		var subscriber = this.ObservableForProperty(p => p.Rating).Subscribe(o => DisplayStarRating(o.Value ?? new Rating(0, 0, 0)));
		Unloaded += (_, _) => subscriber.Dispose();

		if (Design.IsDesignMode)
			Rating = new Rating(5, 4, 3);
	}

	private void DisplayStarRating(Rating rating)
	{
		var blankValue = IsEditingMode ? HOLLOW_STAR : string.Empty;

		string getStar(float score, int starIndex)
			=> Math.Floor(score) > starIndex ? SOLID_STAR
				: score < starIndex ? blankValue
				: score - starIndex < 0.25 ? blankValue
				: score - starIndex > 0.75 ? SOLID_STAR
				: HALF_STAR;

		int starIndex = 0;
		foreach (TextBlock star in panelOverall.Children)
			star.Tag = star.Text = getStar(rating.OverallRating, starIndex++);

		starIndex = 0;
		foreach (TextBlock star in panelPerform.Children)
			star.Tag = star.Text = getStar(rating.PerformanceRating, starIndex++);

		starIndex = 0;
		foreach (TextBlock star in panelStory.Children)
			star.Tag = star.Text = getStar(rating.StoryRating, starIndex++);

		ratingsGrid.IsEnabled = IsEditingMode;
		tblockOverall.IsVisible = panelOverall.IsVisible = IsEditingMode || rating.OverallRating > 0;
		tblockPerform.IsVisible = panelPerform.IsVisible = IsEditingMode || rating.PerformanceRating > 0;
		tblockStory.IsVisible = panelStory.IsVisible = IsEditingMode || rating.StoryRating > 0;
	}

	public void Panel_PointerExited(object sender, Avalonia.Input.PointerEventArgs e)
	{
		if (sender is not Panel panel)
			return;
		var stackPanel = panel.Children.OfType<StackPanel>().Single();

		//Restore defaults
		foreach (TextBlock child in stackPanel.Children)
			child.Text = child.Tag as string;
	}

	public void Star_PointerEntered(object sender, Avalonia.Input.PointerEventArgs e)
	{
		var thisTbox = sender as TextBlock;
		if (thisTbox?.Parent is not StackPanel stackPanel)
			return;
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
		if (thisTbox?.Parent is not StackPanel stackPanel)
			return;

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
