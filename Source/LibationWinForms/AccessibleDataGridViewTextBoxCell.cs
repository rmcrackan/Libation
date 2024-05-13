using System.Windows.Forms;

namespace LibationWinForms
{
    internal class AccessibleDataGridViewTextBoxCell : DataGridViewTextBoxCell
    {
        private string accessibilityDescription;

        protected string AccessibilityName { get; }

        /// <summary>
        /// Get or set description for accessibility. eg: screen readers. Also sets the ToolTipText
        /// </summary>
        protected string AccessibilityDescription
        {
            get => accessibilityDescription;
            set
            {
                accessibilityDescription = value;
                ToolTipText = value;
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance() => new TextBoxCellAccessibilityObject(this, name: AccessibilityName, description: AccessibilityDescription);

        public AccessibleDataGridViewTextBoxCell(string accessibilityName) : base()
        {
            AccessibilityName = accessibilityName;
        }

        protected class TextBoxCellAccessibilityObject : DataGridViewTextBoxCellAccessibleObject
        {
            public string AccessibilityName { get; set; }
            public string AccessibilityDescription { get; set; }

            public override string Name => AccessibilityName;
            public override string Description => AccessibilityDescription;

            public TextBoxCellAccessibilityObject(DataGridViewCell owner, string name, string description) : base(owner)
            {
                AccessibilityName = name;
                AccessibilityDescription = description;
            }
        }
    }
}
