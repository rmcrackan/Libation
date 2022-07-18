using Avalonia.Markup.Xaml;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
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


" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchStringFields());

			NumberFields = @"
Find books between 1-100 minutes long
     length:[1 TO 100]
Find books exactly 1 hr long
     length:60


" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchNumberFields());

			BoolFields = @"
Find books that you haven't rated:
     -IsRated


" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchBoolFields());

			IdFields = @"
Alice's Adventures in Wonderland (ID: B015D78L0U)
     id:B015D78L0U

All of these are synonyms for the ID field


" + string.Join("\r\n", LibationSearchEngine.SearchEngine.GetSearchIdFields());


			DataContext = this;

		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);

			this.HideMinMaxBtns();
		}
	}
}
