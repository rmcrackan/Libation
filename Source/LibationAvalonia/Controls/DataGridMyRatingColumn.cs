using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using DataLayer;
using ReactiveUI;
using System;

namespace LibationAvalonia.Controls
{
	public class DataGridMyRatingColumn : DataGridBoundColumn
	{
		[AssignBinding] public IBinding BackgroundBinding { get; set; }
		[AssignBinding] public IBinding OpacityBinding { get; set; }
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
				myRatingElement.Bind(BindingTarget, Binding);
			if (BackgroundBinding != null)
				myRatingElement.Bind(MyRatingCellEditor.BackgroundProperty, BackgroundBinding);
			if (OpacityBinding != null)
				myRatingElement.Bind(MyRatingCellEditor.OpacityProperty, OpacityBinding);

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
				myRatingElement.Bind(MyRatingCellEditor.BackgroundProperty, BackgroundBinding);
			if (OpacityBinding != null)
				myRatingElement.Bind(MyRatingCellEditor.OpacityProperty, OpacityBinding);

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
