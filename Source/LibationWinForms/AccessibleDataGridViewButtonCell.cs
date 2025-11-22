using System.Windows.Forms;

namespace LibationWinForms
{
    public class AccessibleDataGridViewButtonCell : DataGridViewButtonCell
    {
        protected string AccessibilityName { get; }

        /// <summary>
        /// Get or set description for accessibility. eg: screen readers. Also sets the ToolTipText
        /// </summary>
        protected string AccessibilityDescription
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
        }

        protected class ButtonCellAccessibilityObject : DataGridViewButtonCellAccessibleObject
        {
            private string _name;
            public override string Name => _name;

            private string _description;
            public override string Description => _description;

            public ButtonCellAccessibilityObject(DataGridViewCell owner, string name, string description) : base(owner)
            {
                _name = name;
                _description = description;
            }
        }
    }
}
