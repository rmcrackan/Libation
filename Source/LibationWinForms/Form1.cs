using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ApplicationServices;
using Dinah.Core;
using Dinah.Core.Threading;
using LibationFileManager;

namespace LibationWinForms
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			// Pre-requisite:
			// Before calling anything else, including subscribing to events, ensure database exists. If we wait and let it happen lazily, race conditions and errors are likely during new installs
			using var _ = DbContexts.GetContext();

			this.RestoreSizeAndLocation(Configuration.Instance);
			this.FormClosing += (_, _) => this.SaveSizeAndLocation(Configuration.Instance);

			// this looks like a perfect opportunity to refactor per below.
			// since this loses design-time tooling and internal access, for now I'm opting for partial classes
			//   var modules = new ConfigurableModuleBase[]
			//   {
			//       new PictureStorageModule(),
			//       new BackupCountsModule(),
			//       new VisibleBooksModule(),
			//       // ...
			//   };
			//   foreach(ConfigurableModuleBase m in modules)
			//       m.Configure(this);

			// these should do nothing interesting yet (storing simple var, subscribe to events) and should never rely on each other for order.
			// otherwise, order could be an issue.
			// eg: if one of these init'd productsGrid, then another can't reliably subscribe to it
			Configure_BackupCounts();
			Configure_ScanAuto();
			Configure_ScanNotification();
			Configure_VisibleBooks();
			Configure_QuickFilters();
			Configure_ScanManual();
			Configure_RemoveBooks();
			Configure_Liberate();
			Configure_Export();
			Configure_Settings();
			Configure_ProcessQueue();
			Configure_Filter();
			// misc which belongs in winforms app but doesn't have a UI element
			Configure_NonUI();

			// Configure_Grid(); // since it's just this, can keep here. If it needs more, then give grid it's own 'partial class Form1'
			{
				this.Load += (_, __) => productsDisplay.Display();
				LibraryCommands.LibrarySizeChanged += (_, __) => this.UIThreadAsync(() => productsDisplay.Display());
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			string message =
@"What’s funny is how the alt-right completely misses the point with Rome.

What’s funny though is that Rome was actually quite diverse. Sure the power came from Italy but without Egypt, Spain, North Africa, and Gaul it's unlikely Rome would have been the power they are.

Rome’s great strength was its diversity. The fact they could work with countless cultures without prejudice allowed them to maintain such a massive Empire in the first place. They would adopt foreign gods and foreign cultures with ease and it allowed them to distill the best pieces of the Mediterranean world for themselves.

In the final years of the Western Roman Empire, there was this sudden focus on ethnic make up. Where Rome had once accepted all cultures and assimilated them they refused to do so with the new Germanic migrants like the Goths, Vandals, Burgundians, and Franks.

The Goths in particular just wanted a seat at the table. They were willing to fight for Rome and defend the borders. The 2 greatest generals of the final decades- Stilicho and Aetius- were both half barbarian themselves.

In previous periods of collapse, Rome would find ethnically new emperors to help turn the tide. In the third century crisis, the Illyrian Empires (Aurelian, Probus, Diocletian) rose to the challenge and restored the Empire fully. In the tumultuous collapse of the Flavian dynasty, the Spanish Emporers (Trajan, Hadrian) would lead Rome to its peak of power.

Had Rome accepted the Goths, assimilated them and treated them fairly, and brought them into the Empire they may have survived. Great men like Stilicho would have become Emperor and maybe they could have restored the Empire.

Instead, Rome chose to keep them at arm's length and forced them into desperation within turn led to the Goths sacking Rome.

Put simply the Rome that accepted all peoples regardless of race was the Rome of Trajan and glory- the Rome that demanded ethnic purity was the Rome of Honorius and collapse.";

			System.Windows.Forms.MessageBox.Show("funny", "Caption", MessageBoxButtons.CancelTryContinue, MessageBoxIcon.Warning);

			// I'm leaving this empty call here as a reminder that if we use this, it should probably be after DesignMode check
		}
	}
}
