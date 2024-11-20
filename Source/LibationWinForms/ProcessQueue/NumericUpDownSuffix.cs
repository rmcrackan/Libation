using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace LibationWinForms.ProcessQueue;

public class NumericUpDownSuffix : NumericUpDown
{
    [Description("Suffix displayed after numeric value."), Category("Data")]
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [DisallowNull]
    public string Suffix
    {
        get => _suffix;
        set
        {
            base.Text = string.IsNullOrEmpty(_suffix) ? base.Text : base.Text.Replace(_suffix, value);
            _suffix = value;
            ChangingText = true;
        }
    }
    private string _suffix = string.Empty;
    public override string Text
    {
        get => string.IsNullOrEmpty(Suffix) ? base.Text : base.Text.Replace(Suffix, string.Empty);
        set
        {
            if (Value == Minimum)
                base.Text = "∞";
            else
                base.Text = value + Suffix;
        }
    }
}