using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Dinah.Core.Windows.Forms;

namespace LibationWinForm
{
    public static class IRunnableDialogExt
    {
        public static DialogResult RunDialog(this IRunnableDialog dialog)
        {
            // hook up runner before dialog.ShowDialog for all
            var acceptButton = (ButtonBase)dialog.AcceptButton;
            acceptButton.Click += acceptButton_Click;

            return dialog.ShowDialog();
        }

        // running/workflow logic is in IndexDialogRunner.Run()
        private static async void acceptButton_Click(object sender, EventArgs e)
        {
            var form = ((Control)sender).FindForm();
            var iRunnableDialog = form as IRunnableDialog;

            try
            {
                await iRunnableDialog.Run();
            }
            catch (Exception ex)
            {
                throw new Exception("Did the database get created correctly? Including seed data. Eg: Update-Database", ex);
            }
        }

        public static async Task Run(this IRunnableDialog dialog)
        {
            // validate children
            //   OfType<T>() -- skips items which aren't of the required type
            //   Cast<T>() -- throws an exception
            var errorStrings = dialog
                // get children
                .Controls
                    .GetControlListRecursive()
                    .OfType<IValidatable>()
                // and self
                .Append(dialog)
                // validate. get errors
                .Select(c => c.StringBasedValidate())
                // ignore successes
                .Where(e => e != null);
            if (errorStrings.Any())
            {
                MessageBox.Show(errorStrings.Aggregate((a, b) => a + "\r\n" + b));
                return;
            }

            // get top level controls only. If Enabled, disable and push on stack
            var disabledStack = disable(dialog);

            // lazy-man's async. also violates the intent of async/await.
            // use here for now simply for UI responsiveness
            await dialog.DoMainWorkAsync().ConfigureAwait(true);

            // after running, unwind and re-enable
            enable(disabledStack);

            MessageBox.Show(dialog.SuccessMessage);

            dialog.DialogResult = DialogResult.OK;
            dialog.Close();
        }
        static Stack<Control> disable(IRunnableDialog dialog)
        {
            var disableStack = new Stack<Control>();
            foreach (Control ctrl in dialog.Controls)
            {
                if (ctrl.Enabled)
                {
                    disableStack.Push(ctrl);
                    ctrl.Enabled = false;
                }
            }
            return disableStack;
        }
        static void enable(Stack<Control> disabledStack)
        {
            while (disabledStack.Count > 0)
            {
                var ctrl = disabledStack.Pop();
                ctrl.Enabled = true;
            }
        }
    }
}
