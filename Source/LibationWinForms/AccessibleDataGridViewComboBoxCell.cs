using System.Windows.Forms;

namespace LibationWinForms;

public class AccessibleDataGridViewComboBoxCell : DataGridViewComboBoxCell
{
	protected string AccessibilityName { get; }

	/// <summary>
	/// Get or set description for accessibility. eg: screen readers. Also sets the ToolTipText
	/// </summary>
	protected string? AccessibilityDescription
	{
		get => field;
		set
		{
			field = value;
			ToolTipText = value;
		}
	}

	protected override AccessibleObject CreateAccessibilityInstance() => new ComboBoxCellAccessibilityObject(this, name: AccessibilityName, description: AccessibilityDescription);

	public AccessibleDataGridViewComboBoxCell(string accessibilityName) : base()
	{
		FlatStyle = Application.IsDarkModeEnabled ? FlatStyle.Flat : FlatStyle.Standard;
		AccessibilityName = accessibilityName;
	}

	protected class ComboBoxCellAccessibilityObject : DataGridViewComboBoxCellAccessibleObject
	{
		private readonly string _name;
		public override string Name => _name;
		public override string? Description { get; }

		public ComboBoxCellAccessibilityObject(DataGridViewCell owner, string name, string? description) : base(owner)
		{
			_name = name;
			Description = description;
		}
	}
}
