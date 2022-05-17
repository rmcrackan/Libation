using System;
using System.ComponentModel;
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

		protected override void OnListChanged(ListChangedEventArgs e)
        {
            if (syncContext is not null)
                syncContext.Send(_ => base.OnListChanged(e), null);
            else
                base.OnListChanged(e);
        }
    }
}
