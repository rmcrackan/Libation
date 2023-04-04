using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using FileManager;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Dialogs
{
	public partial class EditReplacementChars : DialogWindow
	{
		Configuration config;

		private readonly List<ReplacementsExt> SOURCE = new();
		public DataGridCollectionView replacements { get; }
		public EditReplacementChars()
		{
			InitializeComponent();

			replacements = new(SOURCE);

			if (Design.IsDesignMode)
			{
				LoadTable(ReplacementCharacters.Default.Replacements);
			}

			DataContext = this;
		}

		public EditReplacementChars(Configuration config) : this()
		{
			this.config = config;
			LoadTable(config.ReplacementCharacters.Replacements);
		}

		public void Defaults()
			=> LoadTable(ReplacementCharacters.Default.Replacements);
		public void LoFiDefaults()
			=> LoadTable(ReplacementCharacters.LoFiDefault.Replacements);
		public void Barebones()
			=> LoadTable(ReplacementCharacters.Barebones.Replacements);

		protected override void SaveAndClose()
		{
			var replacements = SOURCE
				.Where(r => !r.IsDefault)
				.Select(r => new Replacement(r.Character, r.ReplacementText, r.Description) { Mandatory = r.Mandatory })
				.ToList();

			if (config is not null)
				config.ReplacementCharacters = new ReplacementCharacters { Replacements = replacements };
			base.SaveAndClose();
		}

		private void LoadTable(IReadOnlyList<Replacement> replacements)
		{
			SOURCE.Clear();
			SOURCE.AddRange(replacements.Select(r => new ReplacementsExt(r)));
			SOURCE.Add(new ReplacementsExt());
			this.replacements.Refresh();
		}

		public void ReplacementGrid_KeyDown(object sender, Avalonia.Input.KeyEventArgs e)
		{
			if (e.Key == Avalonia.Input.Key.Delete
				&& ((DataGrid)sender).SelectedItem is ReplacementsExt repl
				&& !repl.Mandatory
				&& !repl.IsDefault)
			{
				replacements.Remove(repl);
			}
		}

		public void ReplacementGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
		{
			var replacement = e.Row.DataContext as ReplacementsExt;
			var colBinding = columnBindingPath(e.Column);

			//Prevent duplicate CharacterToReplace
			if (e.EditingElement is TextBox tbox
				&& colBinding == nameof(replacement.CharacterToReplace)
				&& SOURCE.Any(r => r != replacement && r.CharacterToReplace == tbox.Text))
			{
				tbox.Text = replacement.CharacterToReplace;
			}

			//Add new blank row
			void Replacement_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (!SOURCE.Any(r => r.IsDefault))
				{
					var rewRepl = new ReplacementsExt();
					SOURCE.Add(rewRepl);
				}
				replacement.PropertyChanged -= Replacement_PropertyChanged;
			}

			replacement.PropertyChanged += Replacement_PropertyChanged;
		}

		public void ReplacementGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			var replacement = e.Row.DataContext as ReplacementsExt;

			//Disallow editing of Mandatory CharacterToReplace and Descriptions
			if (replacement.Mandatory
				&& columnBindingPath(e.Column) != nameof(replacement.ReplacementText))
				e.Cancel = true;
		}

		private static string columnBindingPath(DataGridColumn column)
			=> ((Binding)((DataGridBoundColumn)column).Binding).Path;

		public class ReplacementsExt : ViewModels.ViewModelBase
		{
			public ReplacementsExt()
			{
				_replacementText = string.Empty;
				_description = string.Empty;
				_characterToReplace = string.Empty;
				IsDefault = true;
			}
			public ReplacementsExt(Replacement replacement)
			{
				_characterToReplace = replacement.CharacterToReplace == default ? "" : replacement.CharacterToReplace.ToString();
				_replacementText = replacement.ReplacementString;
				_description = replacement.Description;
				Mandatory = replacement.Mandatory;
			}
			private string _replacementText;
			private string _description;
			private string _characterToReplace;
			public bool Mandatory { get; }
			public string ReplacementText
			{
				get => _replacementText;
				set
				{
					if (ReplacementCharacters.ContainsInvalidFilenameChar(value))
						this.RaisePropertyChanged(nameof(ReplacementText));
					else
						this.RaiseAndSetIfChanged(ref _replacementText, value);
				}
			}

			public string Description { get => _description; set => this.RaiseAndSetIfChanged(ref _description, value); }

			public string CharacterToReplace
			{
				get => _characterToReplace;

				set
				{
					if (value?.Length != 1)
						this.RaisePropertyChanged(nameof(CharacterToReplace));
					else
					{
						IsDefault = false;
						this.RaiseAndSetIfChanged(ref _characterToReplace, value);
					}
				}
			}
			public char Character => string.IsNullOrEmpty(_characterToReplace) ? default : _characterToReplace[0];
			public bool IsDefault { get; private set; }
		}
	}
}
