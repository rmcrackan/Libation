using Avalonia.Collections;
using Avalonia.Controls;
using FileManager;
using LibationFileManager;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;

namespace LibationAvalonia.Dialogs
{
	public partial class EditReplacementChars : DialogWindow
	{
		private Configuration? Config { get; }

		public bool EnvironmentIsWindows => Configuration.IsWindows;

		private readonly AvaloniaList<ReplacementsExt> SOURCE = new();
		public DataGridCollectionView Replacements { get; }
		public EditReplacementChars()
		{
			InitializeComponent();

			Replacements = new(SOURCE);

			if (Design.IsDesignMode)
			{
				LoadTable(ReplacementCharacters.Default(true).Replacements);
			}

			DataContext = this;
		}

		public EditReplacementChars(Configuration config) : this()
		{
			Config = config;
			LoadTable(config.ReplacementCharacters.Replacements);
		}

		public void Defaults(bool isNtfs)
			=> LoadTable(ReplacementCharacters.Default(isNtfs).Replacements);
		public void LoFiDefaults(bool isNtfs)
			=> LoadTable(ReplacementCharacters.LoFiDefault(isNtfs).Replacements);
		public void Barebones(bool isNtfs)
			=> LoadTable(ReplacementCharacters.Barebones(isNtfs).Replacements);

		public new void Close() => base.Close();
		public new void SaveAndClose()
		{
			if (Config is not null)
			{
				var replacements = SOURCE.Where(r => !r.IsDefault).Select(r => r.ToReplacement()).ToArray();
				Config.ReplacementCharacters = new ReplacementCharacters { Replacements = replacements };
			}
			base.SaveAndClose();
		}

		private void LoadTable(IReadOnlyList<Replacement> replacements)
		{
			SOURCE.Clear();
			SOURCE.AddRange(replacements.Select(r => new ReplacementsExt(r)));
			SOURCE.Add(new ReplacementsExt());
		}

		private bool ColumnIsCharacter(DataGridColumn column)
			=> column.DisplayIndex is 0;

		private bool ColumnIsReplacement(DataGridColumn column)
			=> column.DisplayIndex is 1;

		private bool RowIsReadOnly(DataGridRow row)
			=> row.DataContext is ReplacementsExt rep && rep.Mandatory;

		private bool CanDeleteSelectedItem(ReplacementsExt selectedItem)
			=> !selectedItem.Mandatory && (!selectedItem.IsDefault || SOURCE[^1] != selectedItem);

		private void replacementGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
		{
			e.Cancel = RowIsReadOnly(e.Row) && !ColumnIsReplacement(e.Column);
		}

		private void replacementGrid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
		{
			//Disallow duplicates of CharacterToReplace
			if (ColumnIsCharacter(e.Column) && e.Row.DataContext is ReplacementsExt r && r.CharacterToReplace.Length > 0 && SOURCE.Count(rep => rep.CharacterToReplace == r.CharacterToReplace) > 1)
			{
				r.CharacterToReplace = "";
				e.Cancel = true;
			}
		}

		private void replacementGrid_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
		{
			if (ColumnIsCharacter(e.Column) && e.Row.DataContext is ReplacementsExt r && r.CharacterToReplace.Length > 0 && !SOURCE[^1].IsDefault)
			{
				Replacements.AddNew();
			}
		}

		private void replacementGrid_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
		{
			if (e.Key == Avalonia.Input.Key.Delete && (sender as DataGrid)?.SelectedItem is ReplacementsExt r && CanDeleteSelectedItem(r))
			{
				if (Replacements.IsEditingItem)
				{
					if (Replacements.CanCancelEdit)
						Replacements.CancelEdit();
					else
						Replacements.CommitEdit();
				}
				if (Replacements.IsAddingNew)
				{
					Replacements.CancelNew();
				}
				if (Replacements.CanRemove)
				{
					Replacements.Remove(r);
				}
			}
		}

		public class ReplacementsExt : ViewModels.ViewModelBase
		{
			public ReplacementsExt()
			{
				ReplacementText = string.Empty;
				Description = string.Empty;
				CharacterToReplace = string.Empty;
			}
			public ReplacementsExt(Replacement replacement)
			{
				CharacterToReplace = replacement.CharacterToReplace == default ? "" : replacement.CharacterToReplace.ToString();
				ReplacementText = replacement.ReplacementString;
				Description = replacement.Description;
				Mandatory = replacement.Mandatory;
			}

			public string ReplacementText { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
			public string Description { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
			public string CharacterToReplace { get => field; set => this.RaiseAndSetIfChanged(ref field, value); }
			public char Character => string.IsNullOrEmpty(CharacterToReplace) ? default : CharacterToReplace[0];
			public bool IsDefault => !Mandatory && string.IsNullOrEmpty(CharacterToReplace);
			public bool Mandatory { get; }

			public Replacement ToReplacement()
				=> new(Character, ReplacementText, Description) { Mandatory = Mandatory };
		}
	}
}
