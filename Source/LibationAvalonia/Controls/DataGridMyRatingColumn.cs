using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DataLayer;
using ReactiveUI;
using System;

namespace LibationAvalonia.Controls
{
	public class StarStringConverter : Avalonia.Data.Converters.IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			=> value is Rating rating ? rating.ToStarString() : string.Empty;

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			=> throw new NotImplementedException();
	}

	public class DataGridMyRatingColumn : DataGridBoundColumn
	{
		[Avalonia.Data.AssignBinding]
		public Avalonia.Data.IBinding BackgroundBinding { get; set; }
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

			cell?.AttachContextMenu();

			if (!IsReadOnly)
				ToolTip.SetTip(myRatingElement, "Click to change ratings");

			if (Binding != null)
			{
				myRatingElement.Bind(BindingTarget, Binding);
			}
			if (BackgroundBinding != null)
			{
				myRatingElement.Bind(MyRatingCellEditor.BackgroundProperty, BackgroundBinding);
			}

			return myRatingElement;
		}

		protected override IControl GenerateEditingElementDirect(DataGridCell cell, object dataItem)
		{
			var myRatingElement = new MyRatingCellEditor
			{
				Name = "CellMyRatingEditor",
				IsEditingMode = true
			};
			if (BackgroundBinding != null)
			{
				myRatingElement.Bind(MyRatingCellEditor.BackgroundProperty, BackgroundBinding);
			}

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
