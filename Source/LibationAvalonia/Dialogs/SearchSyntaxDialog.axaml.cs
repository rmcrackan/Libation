using LibationSearchEngine;

namespace LibationAvalonia.Dialogs
{
	public partial class SearchSyntaxDialog : DialogWindow
	{
		public string StringFields { get; init; }
		public string NumberFields { get; init; }
		public string BoolFields { get; init; }
		public string IdFields { get; init; }
		public SearchSyntaxDialog()
		{
			InitializeComponent();

			StringFields = @"
Search for wizard of oz:
     title:oz
     title:""wizard of oz""


" + string.Join("\r\n", SearchEngine.FieldIndexRules.StringFieldNames);

			NumberFields = @"
Find books between 1-100 minutes long
     length:[1 TO 100]
Find books exactly 1 hr long
     length:60
Find books published from 2020-1-1 to
2023-12-31
     datepublished:[20200101 TO 20231231]


" + string.Join("\r\n", SearchEngine.FieldIndexRules.NumberFieldNames);

			BoolFields = @"
Find books that you haven't rated:
     -IsRated


" + string.Join("\r\n", SearchEngine.FieldIndexRules.BoolFieldNames);

			IdFields = @"
Alice's Adventures in
  Wonderland (ID: B015D78L0U)

     id:B015D78L0U

All of these are synonyms
for the ID field


" + string.Join("\r\n", SearchEngine.FieldIndexRules.IdFieldNames);


			DataContext = this;

		}
	}
}
