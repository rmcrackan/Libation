using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
				IsEditingMode = false,
				Margin = new Thickness(3),
				IsEnabled = false
			};

			ToolTip.SetTip(cell, "Click to change ratings");

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
				IsEditingMode = true,
				Margin = new Thickness(3)
			};

			ToolTip.SetTip(cell, null);

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
