using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Forms;

namespace LibationWinForms.GridView;

/// <summary>
/// A DataGridView with a bindable SelectedItem property.
/// </summary>
public class DataGridViewEx : DataGridView, INotifyPropertyChanged
{
    private BindingSource bindingSource;

    private object selectedItem;
    /// <summary>
    /// Can bind to this.
    /// </summary>
    public object SelectedItem
    {
        get => DbNullToNull(selectedItem);
        set => SetCurrentItem(DbNullToNull(value));
    }

    public DataGridViewEx()
    {
        EditMode = DataGridViewEditMode.EditProgrammatically;
        MultiSelect = false;
        ColumnHeadersDefaultCellStyle.SelectionBackColor =
            ColumnHeadersDefaultCellStyle.BackColor;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        this.DataSourceChanged += DataGridViewEx_DataSourceChanged;
    }

    public void AddSelectedItemBinding<T>(T vm, Expression<Func<T, object>> selectedItemProperty)
    {
        try
        {
            var prop = GetPropertyName(selectedItemProperty);
            this.DataBindings.Add(nameof(SelectedItem), vm, prop);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
    {
        var memberExpression = propertyExpression.Body as MemberExpression;

        if (memberExpression == null)
        {
            if (propertyExpression.Body is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
        }

        if (memberExpression != null)
            return memberExpression.Member.Name;

        throw new ArgumentException($"Property expression is not a member access: {propertyExpression}", nameof(propertyExpression));
    }

    private void DataGridViewEx_DataSourceChanged(object sender, EventArgs e)
    {
        try
        {
            // Remove events from old binding source.
            if (this.bindingSource is not null)
            {
                this.bindingSource.CurrencyManager.PositionChanged -= CurrencyManager_PositionChanged;
                this.bindingSource.CurrencyManager.ListChanged -= CurrencyManager_ListChanged;
            }

            if (DataSource is BindingSource bs)
            {
                // Set up events on new binding source.
                this.bindingSource = bs;
                bs.CurrencyManager.PositionChanged += CurrencyManager_PositionChanged;
                bs.CurrencyManager.ListChanged += CurrencyManager_ListChanged;
                this.DataBindings.DefaultDataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                RaisePropertyChanged(nameof(SelectedItem));
            }
            else
                throw new InvalidOperationException($"{nameof(DataGridViewEx)} data source must be a BindingSource in order to handle selection.");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }

    private void CurrencyManager_PositionChanged(object sender, EventArgs args)
    {
        var current = GetCurrent();
        if (current != SelectedItem)
            SelectedItem = current;
    }

    private void CurrencyManager_ListChanged(object sender, ListChangedEventArgs args)
    {
        if (args.ListChangedType == ListChangedType.Reset ||
            args.ListChangedType == ListChangedType.ItemDeleted)
        {
            if (Rows.Count > 0 && SelectedRows.Count == 0)
            {
                this.bindingSource.Position = 0;
            }
            if (SelectedRows.Count > 0)
                BeginInvoke(new MethodInvoker(() =>
                {
                    if (SelectedRows.Count > 0)
                        SelectedItem = SelectedRows[0].DataBoundItem;
                }));
        }
    }
    private object GetCurrent()
    {
        try
        {
            return bindingSource.Count == 0 ||
                   bindingSource.CurrencyManager.Position == -1 ||
                   bindingSource.CurrencyManager.Position >= Rows.Count
                ? null
                : DbNullToNull(Rows[bindingSource.CurrencyManager.Position].DataBoundItem);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void SetCurrentItem(object dataItem)
    {
        try
        {
            if (dataItem == null)
            {
                if (selectedItem != null)
                {
                    selectedItem = null;
                    if (SelectedRows.Count > 0)
                        BeginInvoke(new MethodInvoker(ClearSelection));
                }
                return;
            }

            selectedItem = dataItem;

            for (var index = 0; index < Rows.Count; index++)
            {
                var row = Rows[index];

                // Change the physically selected row in the grid.
                if (CurrentRow == null ||
                    row.DataBoundItem == dataItem)
                {
                    BeginInvoke(new MethodInvoker(() =>
                        CurrentCell = row.Cells[0]));
                    return;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            RaisePropertyChanged(nameof(SelectedItem));
        }
    }
    
    private object DbNullToNull(object dataItem)
    {
        return dataItem is DBNull ? null: dataItem;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    private void RaisePropertyChanged(string propertyName)
    {
        try
        {
            var ev = new PropertyChangedEventArgs(propertyName);
            PropertyChanged?.Invoke(this, ev);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // Eat DBNull conversion bug.
        }
        
    }
}