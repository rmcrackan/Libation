using FileManager;
using LibationFileManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace LibationWinForms.Dialogs
{
	public partial class EditReplacementChars : Form
	{
		Configuration config;
		public EditReplacementChars()
		{
			InitializeComponent();
			dataGridView1_Resize(this, EventArgs.Empty);
		}

		public EditReplacementChars(Configuration config) : this()
		{
			this.config = config;
			LoadTable(config.ReplacementCharacters.Replacements);
		}

		private void LoadTable(IReadOnlyList<Replacement> replacements)
		{
			dataGridView1.Rows.Clear();
			for (int i = 0; i < replacements.Count; i++)
			{
				var r = replacements[i];

				int row = dataGridView1.Rows.Add(r.CharacterToReplace.ToString(), r.ReplacementString, r.Description);
				dataGridView1.Rows[row].Tag = r with { };


				if (r.Mandatory)
				{
					dataGridView1.Rows[row].Cells[charToReplaceCol.Index].ReadOnly = true;
					dataGridView1.Rows[row].Cells[descriptionCol.Index].ReadOnly = true;
					dataGridView1.Rows[row].Cells[charToReplaceCol.Index].Style.BackColor = System.Drawing.Color.LightGray;
					dataGridView1.Rows[row].Cells[descriptionCol.Index].Style.BackColor = System.Drawing.Color.LightGray;
				}
			}
		}

		private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			if (e.Row?.Tag is Replacement r && r.Mandatory)
				e.Cancel = true;
		}

		private void loFiDefaultsBtn_Click(object sender, EventArgs e)
			=> LoadTable(ReplacementCharacters.LoFiDefault.Replacements);

		private void defaultsBtn_Click(object sender, EventArgs e)
			=> LoadTable(ReplacementCharacters.Default.Replacements);

		private void minDefaultBtn_Click(object sender, EventArgs e)
			=> LoadTable(ReplacementCharacters.Barebones.Replacements);


		private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			dataGridView1.Rows[e.RowIndex].ErrorText = string.Empty;

			var charToReplaceStr = dataGridView1.Rows[e.RowIndex].Cells[charToReplaceCol.Index].Value?.ToString();
			var replacement = dataGridView1.Rows[e.RowIndex].Cells[replacementStringCol.Index].Value?.ToString() ?? string.Empty;
			var description = dataGridView1.Rows[e.RowIndex].Cells[descriptionCol.Index].Value?.ToString() ?? string.Empty;

			//Validate the whole row.  If it passes all validation, add or update the row's tag.
			if (string.IsNullOrEmpty(charToReplaceStr) && replacement == string.Empty && description == string.Empty)
			{
				//Invalid entry, so delete row
				var row = dataGridView1.Rows[e.RowIndex];
				if (!row.IsNewRow)
				{
					BeginInvoke(new MethodInvoker(delegate
					{
						dataGridView1.Rows.Remove(row);
					}));
				}
			}
			else if (string.IsNullOrEmpty(charToReplaceStr))
			{
				dataGridView1.Rows[e.RowIndex].ErrorText = $"You must choose a character to replace";
			}
			else if (charToReplaceStr.Length > 1)
			{
				dataGridView1.Rows[e.RowIndex].ErrorText = $"Only 1 {charToReplaceCol.HeaderText} per entry";
			}
			else if (dataGridView1.Rows[e.RowIndex].Tag is Replacement repl && !repl.Mandatory &&
				   dataGridView1.Rows
				   .Cast<DataGridViewRow>()
				   .Where(r => r.Index != e.RowIndex)
				   .Select(r => r.Tag)
				   .OfType<Replacement>()
				   .Any(r => r.CharacterToReplace == charToReplaceStr[0])
				   )
			{
				dataGridView1.Rows[e.RowIndex].ErrorText = $"The {charToReplaceStr[0]} character is already being replaced";
			}
			else if (ReplacementCharacters.ContainsInvalidFilenameChar(replacement))
			{
				dataGridView1.Rows[e.RowIndex].ErrorText = $"Your {replacementStringCol.HeaderText} contains illegal characters";
			}
			else
			{
				//valid entry. Add or update Replacement in row's Tag
				var charToReplace = charToReplaceStr[0];

				if (dataGridView1.Rows[e.RowIndex].Tag is Replacement existing)
					existing.Update(charToReplace, replacement, description);
				else
					dataGridView1.Rows[e.RowIndex].Tag = new Replacement(charToReplace, replacement, description);
			}
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var replacements = dataGridView1.Rows
				.Cast<DataGridViewRow>()
				.Select(r => r.Tag)
				.OfType<Replacement>()
				.ToList();

			config.ReplacementCharacters = new ReplacementCharacters { Replacements = replacements };
			DialogResult = DialogResult.OK;
			Close();
		}

		private void cancelBtn_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void dataGridView1_Resize(object sender, EventArgs e)
		{
			dataGridView1.Columns[^1].Width = dataGridView1.Width - dataGridView1.Columns.Cast<DataGridViewColumn>().Sum(c => c == dataGridView1.Columns[^1] ? 0 : c.Width) - dataGridView1.RowHeadersWidth - 2;
		}
	}
}
