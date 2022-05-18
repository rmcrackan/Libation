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
		private ProductsGrid productsGrid { get; }

		public Form1()
		{
			InitializeComponent();

			if (this.DesignMode)
				return;

			{
				// I'd actually like these lines to be handled in the designer, but I'm currently getting this error when I try:
				//   Failed to create component 'ProductsGrid'.  The error message follows:
				//   'Microsoft.DotNet.DesignTools.Client.DesignToolsServerException: Object reference not set to an instance of an object.
				// Since the designer's choking on it, I'm keeping it below the DesignMode check to be safe
				productsGrid = new ProductsGrid { Dock = DockStyle.Fill };
				gridPanel.Controls.Add(productsGrid);
			}

			// Pre-requisite:
			// Before calling anything else, including subscribing to events, ensure database exists. If we wait and let it happen lazily, race conditions and errors are likely during new installs
			using var _ = DbContexts.GetContext();

			this.Load += (_, _) => this.RestoreSizeAndLocation(Configuration.Instance);
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
			Configure_PictureStorage();
			Configure_BackupCounts();
			Configure_ScanAuto();
			Configure_ScanNotification();
			Configure_VisibleBooks();
			Configure_QuickFilters();
			Configure_ScanManual();
			Configure_Liberate();
			Configure_Export();
			Configure_Settings();
			Configure_ProcessQueue();
			Configure_Filter();

			// Configure_Grid(); // since it's just this, can keep here. If it needs more, then give grid it's own 'partial class Form1'
			{
				this.Load += (_, __) => productsGrid.Display();
				LibraryCommands.LibrarySizeChanged += (_, __) => this.UIThreadAsync(() => productsGrid.Display());
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			if (this.DesignMode)
				return;

			// I'm leaving this empty call here as a reminder that if we use this, it should probably be after DesignMode check
		}
	}
}
