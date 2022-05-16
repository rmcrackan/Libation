using ApplicationServices;
using Dinah.Core.DataBinding;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

// https://stackoverflow.com/a/32886415
namespace LibationWinForms
{
    public class SyncBindingSource : BindingSource
    {
        private SynchronizationContext syncContext { get; }

        public SyncBindingSource() : base()
            => syncContext = SynchronizationContext.Current;
        public SyncBindingSource(IContainer container) : base(container)
            => syncContext = SynchronizationContext.Current;
        public SyncBindingSource(object dataSource, string dataMember) : base(dataSource, dataMember)
            => syncContext = SynchronizationContext.Current;

        public override bool SupportsFiltering => true;
        public override string Filter { get => FilterString; set => SetFilter(value); }

        private string FilterString;

        private void SetFilter(string filterString)
		{
            if (filterString != FilterString)
                RemoveFilter();

            FilterString = filterString;

            var searchResults = SearchEngineCommands.Search(filterString);
            var productIds = searchResults.Docs.Select(d => d.ProductId).ToList();

            var allItems = ((SortableBindingList<GridEntry>)DataSource).InnerList;
            var filterList = productIds.Join(allItems, s => s, ge => ge.AudibleProductId, (pid, ge) => ge).ToList();

            ((SortableBindingList<GridEntry>)DataSource).SetFilteredItems(filterList);
        }

		public override void RemoveFilter()
        {
            ((SortableBindingList<GridEntry>)DataSource).RemoveFilter();
            base.RemoveFilter();
		}
		protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (syncContext is not null)
                syncContext.Send(_ => base.OnListChanged(e), null);
            else
                base.OnListChanged(e);
        }
    }
}
