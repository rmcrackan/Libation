using System;
using System.Linq;
using System.Windows.Forms;
using LibationFileManager;
using LibationWinForms.Dialogs;

namespace LibationWinForms
{
    public partial class Form1
    {
        private void Configure_QuickFilters()
		{
			Load += updateFirstFilterIsDefaultToolStripMenuItem;
			Load += updateFiltersMenu;
			QuickFilters.UseDefaultChanged += updateFirstFilterIsDefaultToolStripMenuItem;
			QuickFilters.Updated += updateFiltersMenu;

			productsGrid.InitialLoaded += (_, __) =>
			{
				if (QuickFilters.UseDefault)
					performFilter(QuickFilters.Filters.FirstOrDefault());
			};
		}

		private object quickFilterTag { get; } = new();
		private void updateFiltersMenu(object _ = null, object __ = null)
		{
			// remove old
			var removeUs = quickFiltersToolStripMenuItem.DropDownItems
				.Cast<ToolStripItem>()
				.Where(item => item.Tag == quickFilterTag)
				.ToList();
			foreach (var item in removeUs)
				quickFiltersToolStripMenuItem.DropDownItems.Remove(item);

			// re-populate
			var index = 0;
			foreach (var filter in QuickFilters.Filters)
			{
				var quickFilterMenuItem = new ToolStripMenuItem
				{
					Tag = quickFilterTag,
					Text = $"&{++index}: {filter}"
				};
				quickFilterMenuItem.Click += (_, __) => performFilter(filter);
				quickFiltersToolStripMenuItem.DropDownItems.Add(quickFilterMenuItem);
			}
		}

		private void updateFirstFilterIsDefaultToolStripMenuItem(object sender, EventArgs e)
			=> firstFilterIsDefaultToolStripMenuItem.Checked = QuickFilters.UseDefault;

		private void firstFilterIsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
			=> QuickFilters.UseDefault = !firstFilterIsDefaultToolStripMenuItem.Checked;

		private void addQuickFilterBtn_Click(object sender, EventArgs e) => QuickFilters.Add(this.filterSearchTb.Text);

		private void editQuickFiltersToolStripMenuItem_Click(object sender, EventArgs e) => new EditQuickFilters().ShowDialog();
	}
}
