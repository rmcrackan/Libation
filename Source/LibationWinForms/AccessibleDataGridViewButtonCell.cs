using System.Windows.Forms;

namespace LibationWinForms;

public class AccessibleDataGridViewButtonCell : DataGridViewButtonCell
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

	protected override AccessibleObject CreateAccessibilityInstance() => new ButtonCellAccessibilityObject(this, name: AccessibilityName, description: AccessibilityDescription);

	public AccessibleDataGridViewButtonCell(string accessibilityName) : base()
	{
		AccessibilityName = accessibilityName;
		FlatStyle = Application.IsDarkModeEnabled ? FlatStyle.Flat : FlatStyle.System;
	}

	protected class ButtonCellAccessibilityObject : DataGridViewButtonCellAccessibleObject
	{
		private string _name;
		public override string Name => _name;
		public override string? Description { get; }

		public ButtonCellAccessibilityObject(DataGridViewCell owner, string name, string? description) : base(owner)
		{
			_name = name;
			Description = description;
		}
	}
}
