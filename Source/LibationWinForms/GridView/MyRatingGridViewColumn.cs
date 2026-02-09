using DataLayer;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms.GridView;

public class MyRatingGridViewColumn : DataGridViewColumn
{
	public MyRatingGridViewColumn() : base(new MyRatingGridViewCell()) { }

	public override DataGridViewCell? CellTemplate
	{
		get => base.CellTemplate;
		set
		{
			if (value is not MyRatingGridViewCell)
				throw new InvalidCastException($"Must be a {nameof(MyRatingGridViewCell)}");

			base.CellTemplate = value;
		}
	}
}

internal class MyRatingGridViewCell : AccessibleDataGridViewTextBoxCell
{
	private static Rating DefaultRating => new Rating(0, 0, 0);
	public override object DefaultNewRowValue => DefaultRating;
	public override Type EditType => typeof(MyRatingCellEditor);
	public override Type ValueType => typeof(Rating);

	public MyRatingGridViewCell() : base("My Rating")
	{
		AccessibilityDescription = ReadOnly ? "" : "Click to change ratings";
	}

	public override void InitializeEditingControl(int rowIndex, object? initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
	{
		base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

		var ctl = DataGridView?.EditingControl as MyRatingCellEditor;

		ctl?.Rating = Value is Rating rating ? rating : DefaultRating;
	}

	protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object? value, object? formattedValue, string? errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
	{
		if (value is Rating rating)
		{
			var starString = rating.ToStarString();
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, starString, starString, errorText, cellStyle, advancedBorderStyle, paintParts);

			AccessibilityDescription = ReadOnly ? "" : "Click to change ratings";
		}
		else
			base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, string.Empty, string.Empty, errorText, cellStyle, advancedBorderStyle, paintParts);
	}

	protected override object? GetFormattedValue(object? value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter? valueTypeConverter, TypeConverter? formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		=> value is Rating rating ? rating.ToStarString() : value?.ToString();

	public override object? ParseFormattedValue(object? formattedValue, DataGridViewCellStyle cellStyle, TypeConverter? formattedValueTypeConverter, TypeConverter? valueTypeConverter)
		=> formattedValue;
}
