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

		private void LoadTable(List<Replacement> replacements)
		{
			dataGridView1.Rows.Clear();
			foreach (var r in replacements)
			{
				int row = dataGridView1.Rows.Add(r.CharacterToReplace, r.ReplacementString, r.Description);
				dataGridView1.Rows[row].Tag = r;

				if (ReplacementCharacters.Default.Replacements.Any(rep => rep.CharacterToReplace == r.CharacterToReplace))
				{
					r.Mandatory = true;
					dataGridView1.Rows[row].Cells[charToReplaceCol.Index].ReadOnly = true;
					dataGridView1.Rows[row].Cells[descriptionCol.Index].ReadOnly = true;
				}
			}
		}

		private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
		{
			if (e.Row?.Tag is Replacement r && r.Mandatory)
				e.Cancel = true;
		}

		private void loFiDefaultsBtn_Click(object sender, EventArgs e)
		{
			LoadTable(ReplacementCharacters.LoFiDefault.Replacements);
		}

		private void defaultsBtn_Click(object sender, EventArgs e)
		{
			LoadTable(ReplacementCharacters.Default.Replacements);
		}

		private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
		{
			if (e.RowIndex < 0) return;

			var cellValue = e.FormattedValue?.ToString();

			if (dataGridView1.Rows[e.RowIndex].Tag is Replacement row && row.Mandatory)
			{
				if (e.ColumnIndex == replacementStringCol.Index)
				{
					//Ensure replacement string doesn't contain an illegal character.
					var replaceString = cellValue ?? string.Empty;
					if (replaceString != string.Empty && replaceString.Any(c => FileUtility.invalidChars.Contains(c)))
					{
						dataGridView1.Rows[e.RowIndex].ErrorText = $"{replaceString} contains an illegal path character";
						e.Cancel = true;
					}
				}
				return;
			}



			if (e.ColumnIndex == charToReplaceCol.Index)
			{
				if (cellValue.Length != 1)
				{
					dataGridView1.Rows[e.RowIndex].ErrorText = "Only 1 character to replace per entry";
					e.Cancel = true;
				}
				else if (
					dataGridView1.Rows
					.Cast<DataGridViewRow>()
					.Where(r => r.Index != e.RowIndex)
					.OfType<Replacement>()
					.Any(r => r.CharacterToReplace == cellValue[0])
					)
				{
					dataGridView1.Rows[e.RowIndex].ErrorText = $"The {cellValue[0]} character is already being replaced";
					e.Cancel = true;
				}
			}
			else if (e.ColumnIndex == descriptionCol.Index || e.ColumnIndex == replacementStringCol.Index)
			{
				var value = dataGridView1.Rows[e.RowIndex].Cells[charToReplaceCol.Index].Value;
				if (value is null || value is string str && string.IsNullOrEmpty(str))
				{
					dataGridView1.Rows[e.RowIndex].ErrorText = $"You must choose a character to replace";
					e.Cancel = true;
				}
			}
		}

		private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex < 0) return;

			dataGridView1.Rows[e.RowIndex].ErrorText = string.Empty;

			var cellValue = dataGridView1.Rows[e.RowIndex].Cells[charToReplaceCol.Index].Value?.ToString();

			if (string.IsNullOrEmpty(cellValue) || cellValue.Length > 1)
			{
				var row = dataGridView1.Rows[e.RowIndex];
				if (!row.IsNewRow)
				{
					BeginInvoke(new MethodInvoker(delegate
					{
						dataGridView1.Rows.Remove(row);
					}));
				}
			}
			else
			{
				char charToReplace = cellValue[0];
				string description = dataGridView1.Rows[e.RowIndex].Cells[descriptionCol.Index].Value?.ToString() ?? string.Empty;
				string replacement = dataGridView1.Rows[e.RowIndex].Cells[replacementStringCol.Index].Value?.ToString() ?? string.Empty;

				var mandatory = false;
				if (dataGridView1.Rows[e.RowIndex].Tag is Replacement existing)
				{
					mandatory = existing.Mandatory;
				}

				dataGridView1.Rows[e.RowIndex].Tag =
					new Replacement()
					{
						CharacterToReplace = charToReplace,
						ReplacementString = replacement,
						Description = description,
						Mandatory = mandatory
					};
			}
		}

		private void saveBtn_Click(object sender, EventArgs e)
		{
			var replacements = dataGridView1.Rows
				.Cast<DataGridViewRow>()
				.Select(r => r.Tag)
				.OfType<Replacement>()
				.Where(r => r.ReplacementString != null && (r.ReplacementString == string.Empty || !r.ReplacementString.Any(c => FileUtility.invalidChars.Contains(c))))
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
