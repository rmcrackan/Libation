using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DataLayer;
using LibationAvalonia.ViewModels;
using ReactiveUI;
using System;

namespace LibationAvalonia.Controls
{
	public class DataGridMyRatingColumn : DataGridBoundColumn
	{
		private static Rating DefaultRating => new Rating(0, 0, 0);
		public DataGridMyRatingColumn()
		{
			BindingTarget = MyRatingCellEditor.RatingProperty;
		}

		protected override IControl GenerateElement(DataGridCell cell, object dataItem)
		{
			var myRatingElement = new MyRatingCellEditor
			{
				Name = "CellMyRatingDisplay",
				IsEditingMode = false
			};

			ToolTip.SetTip(myRatingElement, "Click to change ratings");
			cell?.AttachContextMenu();

			if (Binding != null)
			{
				myRatingElement.Bind(BindingTarget, Binding);
			}

			void setControlBackground(object dataContext)
			{
				if (dataContext is GridEntry ge)
					myRatingElement.Background = ge.BackgroundBrush;
			}

			setControlBackground(cell?.DataContext);

			var subscriber =
				cell
				?.ObservableForProperty(g => g.DataContext)
				?.Subscribe(ctx => setControlBackground(ctx?.Value));

			myRatingElement.DetachedFromVisualTree += (_, _) => subscriber?.Dispose();

			return myRatingElement;
		}

		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			var myRatingElement = new MyRatingCellEditor
			{
				Name = "CellMyRatingEditor",
				IsEditingMode = true
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
