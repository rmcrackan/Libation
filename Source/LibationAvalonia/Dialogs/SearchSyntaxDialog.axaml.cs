using Avalonia;
using LibationSearchEngine;
using System;
using System.Linq;
using LibationUiBase.Resources;

namespace LibationAvalonia.Dialogs;

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

		StringUsage = Strings.SearchSyntaxDialog_SearchForWizardOfOzTitle;

		NumberUsage = Strings.SearchSyntaxDialog_FindBooksBetween1100Minutes;

		BoolUsage = Strings.SearchSyntaxDialog_FindBooksThatYouHavenT;

		IdUsage = Strings.SearchSyntaxDialog_AliceSAdventuresInWonderlandID;

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
