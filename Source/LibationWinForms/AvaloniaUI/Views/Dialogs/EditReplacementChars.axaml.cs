using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileManager;
using LibationFileManager;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveUI;
using System.Linq;

namespace LibationWinForms.AvaloniaUI.Views.Dialogs
{
	public partial class EditReplacementChars : DialogWindow
	{
		Configuration config = Configuration.Instance;
		public ObservableCollection<ReplacementsExt> replacements { get; }
		public EditReplacementChars()
		{
			InitializeComponent();

			if (Design.IsDesignMode)
				AudibleUtilities.AudibleApiStorage.EnsureAccountsSettingsFileExists();

			replacements = new(config.ReplacementCharacters.Replacements.Select(r => new ReplacementsExt { Replacement = r }));
			DataContext = this;
		}


		public class ReplacementsExt : ViewModels.ViewModelBase
		{
			public Replacement Replacement { get; init; }
			public string ReplacementText
			{
				get => Replacement.ReplacementString;
				set
				{
					Replacement.ReplacementString = value;
					this.RaisePropertyChanged(nameof(ReplacementText));
				}
			}
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
