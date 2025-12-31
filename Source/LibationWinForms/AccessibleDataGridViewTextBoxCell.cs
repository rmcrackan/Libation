using System.ComponentModel;
using System.Windows.Forms;

namespace LibationWinForms
{
	public class AccessibleDataGridViewColumn : DataGridViewColumn
	{
		[DefaultValue(null)]
		[Category("Accessibility")]
		[Description("Accessibility Object Name")]
		public string AccessibilityName { get => field; set { field = value; cellTemplate.AccessibilityName = value; } }

		[DefaultValue(null)]
		[Category("Accessibility")]
		[Description("Accessibility Object Description")]
		public string AccessibilityDescription { get => field; set { field = value; cellTemplate.AccessibilityDescription = value; } }
		private readonly AccessibleDataGridViewTextBoxCell cellTemplate;

		public AccessibleDataGridViewColumn()
		{
			CellTemplate = cellTemplate = new AccessibleDataGridViewTextBoxCell();
		}
		public AccessibleDataGridViewColumn(AccessibleDataGridViewTextBoxCell cellTemplate) : base(cellTemplate)
		{
			this.cellTemplate = cellTemplate;
		}

		public override object Clone()
		{
			//This is necessary for the designer to work properly
			var col = (AccessibleDataGridViewColumn)base.Clone();
			col.AccessibilityDescription = AccessibilityDescription;
			col.AccessibilityName = AccessibilityName;
			return col;
		}
	}

	public class AccessibleDataGridViewTextBoxCell : DataGridViewTextBoxCell
    {
        private string _accessibilityName;

		public string AccessibilityName
        {
            get => _accessibilityName;
            set
            {
				_accessibilityName = value;
				(AccessibilityObject as TextBoxCellAccessibilityObject).SetName(_accessibilityName);
			}
        }

		/// <summary>
		/// Get or set description for accessibility. eg: screen readers. Also sets the ToolTipText
		/// </summary>
		public string AccessibilityDescription
        {
            get => field;
            set
            {
                field = value;
				(AccessibilityObject as TextBoxCellAccessibilityObject).SetDescription(field);
				ToolTipText = value;
            }
        }

        protected override AccessibleObject CreateAccessibilityInstance() => new TextBoxCellAccessibilityObject(this, name: AccessibilityName, description: AccessibilityDescription);

        public AccessibleDataGridViewTextBoxCell(string accessibilityName) : base()
        {
			_accessibilityName = accessibilityName;
        }

        public AccessibleDataGridViewTextBoxCell() { }

        protected class TextBoxCellAccessibilityObject : DataGridViewTextBoxCellAccessibleObject
        {
            private string _name;
            public override string Name => _name;

            private string _description;
            public override string Description => _description;

            public void SetName(string name) => _name = name;
            public void SetDescription(string description) => _description = description;

            public TextBoxCellAccessibilityObject(DataGridViewCell owner, string name, string description) : base(owner)
            {
                _name = name;
                _description = description;
            }
        }
    }
}
