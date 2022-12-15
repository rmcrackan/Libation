using AppScaffolding;

namespace HangoverWinForms
{
    public partial class Form1
    {
        private void Load_cliTab()
        {

        }

        private void cliTab_VisibleChanged(object sender, EventArgs e)
        {
            if (!cliTab.Visible)
                return;
        }
    }
}
