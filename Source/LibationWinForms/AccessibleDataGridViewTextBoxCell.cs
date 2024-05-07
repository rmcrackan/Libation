using System.Windows.Forms;

namespace LibationWinForms
{
    internal class AccessibleDataGridViewTextBoxCell : DataGridViewTextBoxCell
    {
        protected virtual string AccessibilityName
        {
            get => MyAccessibilityObject.AccessibilityName;
            set => MyAccessibilityObject.AccessibilityName = value;
        }

        /// <summary>
        /// Get or set description for accessibility. eg: screen readers. Also sets the ToolTipText
        /// </summary>
        protected string AccessibilityDescription
        {
            get => MyAccessibilityObject.AccessibilityDescription;
            set
            {
                MyAccessibilityObject.AccessibilityDescription = value;
                MyAccessibilityObject.Owner.ToolTipText = value;
            }
        }

        protected ButtonCellAccessibilityObject MyAccessibilityObject { get; set; }
        protected override AccessibleObject CreateAccessibilityInstance() => MyAccessibilityObject;

        public AccessibleDataGridViewTextBoxCell(string accessibilityName) : base()
        {
            MyAccessibilityObject = new(this, name: accessibilityName, description: "");
        }

        protected class ButtonCellAccessibilityObject : DataGridViewTextBoxCellAccessibleObject
        {
            public string AccessibilityName { get; set; }
            public string AccessibilityDescription { get; set; }

            public override string Name => AccessibilityName;
            public override string Description => AccessibilityDescription;

            public ButtonCellAccessibilityObject(DataGridViewCell owner, string name, string description) : base(owner)
            {
                AccessibilityName = name;
                AccessibilityDescription = description;
            }
        }
    }
}
