using DataLayer;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
	public class MyRatingGridViewColumn : DataGridViewColumn
	{
		public MyRatingGridViewColumn() : base(new MyRatingGridViewCell()) { }

		public override DataGridViewCell CellTemplate
		{
			get => base.CellTemplate;
			set
			{
				if (value is not MyRatingGridViewCell)
					throw new InvalidCastException("Must be a MyRatingGridViewCell");

				base.CellTemplate = value;
			}
		}
	}

	internal class MyRatingGridViewCell : DataGridViewTextBoxCell
	{
		public override void InitializeEditingControl(int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);

			var ctl = DataGridView.EditingControl as RatingPicker;

			ctl.Rating =
				Value is Rating rating
				? rating
				: (Rating)DefaultNewRowValue;
		}

		public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
		{
			const char SOLID_STAR = '★';
			if (formattedValue is string s)
			{
				int overall = 0, performance = 0, story = 0;

				foreach (var line in s.Split('\n'))
				{
					if (line.Contains("Overall"))
						overall = line.Count(c => c == SOLID_STAR);
					else if (line.Contains("Perform"))
						performance = line.Count(c => c == SOLID_STAR);
					else if (line.Contains("Story"))
						story = line.Count(c => c == SOLID_STAR);
				}

				return new Rating(overall, performance, story);
			}
			else
				return DefaultNewRowValue;
		}


		protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
			=> value is Rating rating
				? rating.ToStarString()
				: base.GetFormattedValue(value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);

		public override Type EditType => typeof(RatingPicker);
		public override object DefaultNewRowValue => new Rating(0, 0, 0);
		public override Type ValueType => typeof(Rating);
	}
}
