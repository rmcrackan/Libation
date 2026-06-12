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

		fixDuplicatesTab.VisibleChanged += fixDuplicatesTab_VisibleChanged;
		deletedTab.VisibleChanged += deletedTab_VisibleChanged;

		Load_databaseTab();
		Load_deletedTab();
	}
}
