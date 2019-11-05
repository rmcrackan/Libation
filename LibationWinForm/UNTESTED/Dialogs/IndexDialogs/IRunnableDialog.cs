using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibationWinForm
{
    public interface IRunnableDialog
    {
        IButtonControl AcceptButton { get; set; }
        Control.ControlCollection Controls { get; }
        Task DoMainWorkAsync();
        string SuccessMessage { get; }
        DialogResult ShowDialog();
        DialogResult DialogResult { get; set; }
        void Close();
    }
}
