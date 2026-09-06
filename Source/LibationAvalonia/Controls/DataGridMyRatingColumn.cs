using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using DataLayer;

namespace LibationAvalonia.Controls;

public class DataGridMyRatingColumn : DataGridTemplateColumn
{
	[AssignBinding] public BindingBase? BackgroundBinding { get; set; }
	[AssignBinding] public BindingBase? OpacityBinding { get; set; }
	[AssignBinding] public BindingBase? RatingBinding { get; set; }
	private static Rating DefaultRating => new Rating(0, 0, 0);
	public DataGridMyRatingColumn()
	{
		this.IsReadOnly = false;
		//Must set the CellEditingTemplate to enable cell editing in a DataGridTemplateColumn.
		CellEditingTemplate = new FuncDataTemplate(typeof(Rating), (value, _) =>
		{
			var myRatingElement = CreateControl();
			myRatingElement.Name = "CellMyRatingEditor";
			myRatingElement.IsEditingMode = true;
			return myRatingElement;
		});
	}

	protected override Control GenerateElement(DataGridCell cell, object dataItem)
	{
		var myRatingElement = CreateControl();
		myRatingElement.Name = "CellMyRatingDisplay";
		myRatingElement.IsEditingMode = false;

		cell.Tag = this;

		if (!IsReadOnly)
			ToolTip.SetTip(myRatingElement, "Click to change ratings");

		return myRatingElement;
	}

	private MyRatingCellEditor CreateControl()
	{
		var myRatingElement = new MyRatingCellEditor();

		if (RatingBinding != null)
			myRatingElement.Bind(MyRatingCellEditor.RatingProperty, RatingBinding);
		if (BackgroundBinding != null)
			myRatingElement.Bind(MyRatingCellEditor.BackgroundProperty, BackgroundBinding);
		if (OpacityBinding != null)
			myRatingElement.Bind(MyRatingCellEditor.OpacityProperty, OpacityBinding);
		return myRatingElement;
	}

	protected override object PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
		=> editingElement is MyRatingCellEditor myRating
		? myRating.Rating
		: DefaultRating;

	protected override void CancelCellEdit(Control editingElement, object uneditedValue)
	{
		if (editingElement is MyRatingCellEditor myRating)
		{
			var uneditedRating = uneditedValue as Rating;
			myRating.Rating = uneditedRating ?? DefaultRating;
		}
	}
}
