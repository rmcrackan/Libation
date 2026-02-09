using AppScaffolding;

namespace HangoverWinForms;

public partial class Form1 : Form
{
	public Form1()
	{
		InitializeComponent();

		var config = LibationScaffolding.RunPreConfigMigrations();
		LibationScaffolding.RunPostConfigMigrations(config);
		LibationScaffolding.RunPostMigrationScaffolding(Variety.Classic, config);

		databaseTab.VisibleChanged += databaseTab_VisibleChanged;
		cliTab.VisibleChanged += cliTab_VisibleChanged;
		deletedTab.VisibleChanged += deletedTab_VisibleChanged;

		Load_databaseTab();
		Load_cliTab();
		Load_deletedTab();
	}
}
