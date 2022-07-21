using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileManager;
using LibationFileManager;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public partial class EditReplacementChars : DialogWindow
	{
		Configuration config = Configuration.Instance;
		public ObservableCollection<Replacement> replacements { get; }
		public EditReplacementChars()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
				AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();

			replacements = new(config.ReplacementCharacters.Replacements);
			DataContext = this;
		}

		public void Tb_GotFocus(object sender, Avalonia.Input.GotFocusEventArgs e)
		{

		}

		private void InitializeComponent()
		{
			AvaloniaXamlLoader.Load(this);
		}


		private void LoadTable(IReadOnlyList<Replacement> replacements)
		{
			
		}
	}
}
