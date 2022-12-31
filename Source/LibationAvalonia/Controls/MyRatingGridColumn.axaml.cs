using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DataLayer;

namespace LibationAvalonia.Controls
{
	public partial class MyRatingGridColumn : DataGridBoundColumn
	{
		private static Rating DefaultRating => new Rating(0, 0, 0);
		public MyRatingGridColumn()
		{
			AvaloniaXamlLoader.Load(this);
			BindingTarget = MyRatingCellEditor.RatingProperty;
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			var myRatingElement = new MyRatingCellEditor
			{
				Name = "CellMyRatingDisplay",
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
				VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
				AllRatingsVisible = false,
				Margin = new Thickness(3),
				IsEnabled = false
			};

			if (Binding != null)
			{
				myRatingElement.Bind(BindingTarget, Binding);
			}
			return myRatingElement;
		}

		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			var myRatingElement = new MyRatingCellEditor
			{
				Name = "CellMyRatingCellEditor",
				HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
				VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
				AllRatingsVisible = true,
				Margin = new Thickness(3)
			};

			return myRatingElement;
		}

		protected override object PrepareCellForEdit(IControl editingElement, RoutedEventArgs editingEventArgs)
			=> editingElement is MyRatingCellEditor myRating
			? myRating.Rating
			: DefaultRating;

		protected override void CancelCellEdit(IControl editingElement, object uneditedValue)
		{
			if (editingElement is MyRatingCellEditor myRating)
			{
				var uneditedRating = uneditedValue as Rating;
				myRating.Rating = uneditedRating ?? DefaultRating;
			}
		}
	}
}
