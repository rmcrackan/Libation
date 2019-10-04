using System;
using System.Windows.Forms;
using AudibleDotComAutomation;

namespace LibationWinForm
{
    public partial class WebsiteProcessorControl : UserControl, IValidatable
    {
        public event EventHandler<KeyPressEventArgs> KeyPressSubmit;
        
        public WebsiteProcessorControl()
        {
            InitializeComponent();
        }

        public IPageRetriever GetPageRetriever()
            => AuthRb_UseCanonicalChrome.Checked ? new UserDataSeleniumRetriever()
            : AuthRb_Browserless.Checked ? (IPageRetriever)new BrowserlessRetriever()
            : new ManualLoginSeleniumRetriever(UsernameTb.Text, PasswordTb.Text);

        public string StringBasedValidate()
        {
            if (AuthRb_ManualLogin.Checked && (string.IsNullOrWhiteSpace(UsernameTb.Text) || string.IsNullOrWhiteSpace(PasswordTb.Text)))
                return "must fill in username and password";

            return null;
        }

        private void UsernamePasswordTb_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                KeyPressSubmit?.Invoke(sender, e);
                // call your method for action on enter
                e.Handled = true; // suppress default handling
            }
        }

        private void UserIsEnteringLoginInfo(object sender, EventArgs e) => AuthRb_ManualLogin.Checked = true;

        private void AuthRb_UseCanonicalChrome_CheckedChanged(object sender, EventArgs e)
        {
            if (AuthRb_UseCanonicalChrome.Checked)
                MessageBox.Show(@"A canonical version of Chrome will be used including User Data, cookies. etc. Selenium chromedriver won't launch URL if another Chrome instance is open");
        }
    }
}
