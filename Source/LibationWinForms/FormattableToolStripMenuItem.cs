using System;
using System.Drawing;
using System.Windows.Forms;

namespace LibationWinForms
{
    public class FormattableToolStripMenuItem : ToolStripMenuItem
    {
        public string FormatText { get; set; }

        /// <summary>Text set: first non-null, non-whitespace <see cref="Text"/> set is also saved as <see cref="FormatText"/></summary>
        public override string Text
        {
            get => base.Text;
            set
            {
                if (string.IsNullOrWhiteSpace(FormatText))
                    FormatText = value;

                base.Text = value;
            }
        }

        #region ctor.s
        public FormattableToolStripMenuItem() : base() { }
        public FormattableToolStripMenuItem(string text) : base(text) => FormatText = text;
        public FormattableToolStripMenuItem(Image image) : base(image) { }
        public FormattableToolStripMenuItem(string text, Image image) : base(text, image) => FormatText = text;
        public FormattableToolStripMenuItem(string text, Image image, EventHandler onClick) : base(text, image, onClick) => FormatText = text;
        public FormattableToolStripMenuItem(string text, Image image, params ToolStripItem[] dropDownItems) : base(text, image, dropDownItems) => FormatText = text;
        public FormattableToolStripMenuItem(string text, Image image, EventHandler onClick, Keys shortcutKeys) : base(text, image, onClick, shortcutKeys) => FormatText = text;
        public FormattableToolStripMenuItem(string text, Image image, EventHandler onClick, string name) : base(text, image, onClick, name) => FormatText = text;
        #endregion

        /// <summary>Replaces the format item in a specified string with the string representation of a corresponding object in a specified array. Returns <see cref="Text"/> for convenience.</summary>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public string Format(params object[] args) => Text = string.Format(FormatText, args);
    }
}
