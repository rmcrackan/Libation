using Avalonia;
using LibationSearchEngine;
using System;
using System.Linq;

namespace LibationAvalonia.Dialogs
{
	public partial class SearchSyntaxDialog : DialogWindow
	{
		public event EventHandler<string>? TagDoubleClicked;
		public string StringUsage { get; }
		public string NumberUsage { get; }
		public string BoolUsage { get; }
		public string IdUsage { get; }
		public string[] StringFields { get; } = SearchEngine.FieldIndexRules.StringFieldNames.ToArray();
		public string[] NumberFields { get; } = SearchEngine.FieldIndexRules.NumberFieldNames.ToArray();
		public string[] BoolFields { get; } = SearchEngine.FieldIndexRules.BoolFieldNames.ToArray();
		public string[] IdFields { get; } = SearchEngine.FieldIndexRules.IdFieldNames.ToArray();

		public SearchSyntaxDialog()
		{
			InitializeComponent();

			StringUsage = """
			Search for wizard of oz:
				 title:oz
				 title:"wizard of oz"
			""";

			NumberUsage = """
			Find books between 1-100 minutes long
				 length:[1 TO 100]
			Find books exactly 1 hr long
				 length:60
			Find books published from 2020-1-1 to
			2023-12-31
				 datepublished:[20200101 TO 20231231]
			""";

			BoolUsage = """
			Find books that you haven't rated:
				 -IsRated
			""";

			IdUsage = """
			Alice's Adventures in
			  Wonderland (ID: B015D78L0U)

				 id:B015D78L0U

			All of these are synonyms
			for the ID field
			""";

			DataContext = this;
		}

		private void ListBox_DoubleTapped(object? sender, Avalonia.Input.TappedEventArgs e)
		{
			if (e.Source is StyledElement { DataContext: string tag })
			{
				TagDoubleClicked?.Invoke(this, tag);
			}
		}
	}
}
