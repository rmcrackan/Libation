namespace HangoverWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            databaseTab.VisibleChanged += databaseTab_VisibleChanged;
            cliTab.VisibleChanged += cliTab_VisibleChanged;

            Load_databaseTab();
            Load_cliTab();
        }
    }
}
