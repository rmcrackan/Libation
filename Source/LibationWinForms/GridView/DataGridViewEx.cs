using NPOI.SS.Formula.Functions;
using System;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace LibationWinForms.GridView
{
    public class DataGridViewEx : DataGridView
    {
        private BindingSource bindingSource;

        // Can bind to this!
        public object SelectedItem
        {
            get => GetCurrent();
            set => SetCurrentItem(value);
        }

        public DataGridViewEx()
        {
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.DataSourceChanged += DataGridViewEx_DataSourceChanged;
        }

        public void AddSelectedItemBinding<T>(T vm, Expression<Func<T, object>> selectedItemProperty)
        {
            var prop = GetPropertyName(selectedItemProperty);
            this.DataBindings.Add(nameof(SelectedItem), vm, prop);
        }

        static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
        {
            MemberExpression memberExpression = propertyExpression.Body as MemberExpression;

            if (memberExpression == null)
            {
                UnaryExpression unaryExpression = propertyExpression.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                }
            }

            if (memberExpression != null)
                return memberExpression.Member.Name;

            throw new ArgumentException("Expression is not a member access", nameof(propertyExpression));
        }

        private void DataGridViewEx_DataSourceChanged(object sender, EventArgs e)
        {
            if (this.bindingSource is not null)
                this.bindingSource.CurrencyManager.CurrentChanged -= CurrencyManager_CurrentChanged;

            if (DataSource is BindingSource bs)
            {
                if (bindingSource != bs)
                {
                    this.bindingSource = bs;
                    bs.CurrencyManager.CurrentChanged += CurrencyManager_CurrentChanged;
                }
            }
            else
                throw new InvalidOperationException($"{nameof(DataGridViewEx)} data source must be a BindingSource in order to handle currency");
        }

        private void CurrencyManager_CurrentChanged(object sender, EventArgs e)
        {
            // Set SelectedItem property
            object current = GetCurrent();                
                if (current != SelectedItem)
                    SelectedItem = current;
        }

        private object GetCurrent()
        {
            return bindingSource.Count == 0 || 
                   bindingSource.CurrencyManager.Position != -1
                ? null 
                : bindingSource.CurrencyManager.Current;
        }

        private void SetCurrentItem(object dataItem)
        {
            if (SelectedItem != dataItem)
            {
                foreach (DataGridViewRow row in Rows)
                {
                    if (row.DataBoundItem == dataItem)
                    {
                        SelectedItem = dataItem;
                        CurrentCell = row.Cells[0];
                        return;
                    }
                }
                SelectedItem = null;
                ClearSelection();                
            }
        }
    }
}
