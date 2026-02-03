using System;
using System.Linq;
using System.Windows.Forms;
using LibationFileManager;
using LibationWinForms.Dialogs;

#nullable enable
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
		}

		private object quickFilterTag { get; } = new();
		private void updateFiltersMenu(object? _ = null, object? __ = null)
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
					Text = $"&{++index}: {(string.IsNullOrWhiteSpace(filter.Name) ? filter.Filter : filter.Name)}"
				};
				quickFilterMenuItem.Click += (_, __) => performFilter(filter.Filter);
				quickFiltersToolStripMenuItem.DropDownItems.Add(quickFilterMenuItem);
			}
		}

		private void updateFirstFilterIsDefaultToolStripMenuItem(object? sender, EventArgs e)
			=> firstFilterIsDefaultToolStripMenuItem.Checked = QuickFilters.UseDefault;

		private void firstFilterIsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
			=> QuickFilters.UseDefault = !firstFilterIsDefaultToolStripMenuItem.Checked;

		private void addQuickFilterBtn_Click(object sender, EventArgs e)
        {
            QuickFilters.Add(new QuickFilters.NamedFilter(this.filterSearchTb.Text, null));
        }

        private void editQuickFiltersToolStripMenuItem_Click(object sender, EventArgs e) => new EditQuickFilters().ShowDialog();

		private void productsDisplay_InitialLoaded(object sender, EventArgs e)
		{
			if (QuickFilters.UseDefault)
            {
                // begin verbose null checking. shouldn't be possible, yet NRE in #1578
                var f = QuickFilters.Filters;
                if (f is null)
					Serilog.Log.Logger.Error("Unexpected exception. QuickFilters.Filters is null");

				var first = f.FirstOrDefault();
                if (first is null)
                    Serilog.Log.Logger.Information("QuickFilters.Filters.FirstOrDefault() is null");

				var filter = first?.Filter;
                if (filter is null)
                    Serilog.Log.Logger.Information("QuickFilters.Filters.FirstOrDefault()?.Filter is null");
                // end verbose null checking

                performFilter(filter);
            }
		}
	}
}
