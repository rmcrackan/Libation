using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace AppScaffolding;

/// <summary>Represents a thread-safe dynamic data collection 
/// that provides notifications when items get added, removed, 
/// or when the whole list is refreshed.
/// Yoinked from: https://github.com/CoryCharlton/CCSWE.Core/blob/master/src/Core/Collections/ObjectModel/SynchronizedObservableCollection%601.cs</summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
#if NETSTANDARD2_0 || NETFULL
    [Serializable]
#endif
[ComVisible(false)]
[DebuggerDisplay("Count = {Count}")]
public class SynchronizedObservableCollection<T> : IDisposable, IList<T>, IList, IReadOnlyList<T>, INotifyCollectionChanged, INotifyPropertyChanged
{
    /// <summary>Initializes a new instance of the <see cref="SynchronizedObservableCollection{T}" /> class.</summary>
    public SynchronizedObservableCollection() : this(new List<T>(), GetCurrentSynchronizationContext())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SynchronizedObservableCollection{T}" /> class that contains elements copied from the specified collection.</summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> parameter cannot be null.</exception>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public SynchronizedObservableCollection(IEnumerable<T> collection) : this(collection, GetCurrentSynchronizationContext())
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SynchronizedObservableCollection{T}" /> class with the specified context.</summary>
    /// <param name="context">The context used for event invokation.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="context" /> parameter cannot be null.</exception>
    public SynchronizedObservableCollection(SynchronizationContext context) : this(new List<T>(), context)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SynchronizedObservableCollection{T}" /> class that contains elements copied from the specified collection with the specified context.</summary>
    /// <param name="collection">The collection from which the elements are copied.</param>
    /// <param name="context">The context used for event invokation.</param>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="collection" /> parameter cannot be null.</exception>
    /// <exception cref="T:System.ArgumentNullException">The <paramref name="context" /> parameter cannot be null.</exception>
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public SynchronizedObservableCollection(IEnumerable<T> collection, SynchronizationContext context)
    {
        Ensure.IsNotNull(nameof(collection), collection);
        Ensure.IsNotNull(nameof(context), context);

        _context = context;

        foreach (var item in collection)
        {
            _items.Add(item);
        }
    }

#if NETSTANDARD2_0 || NETFULL
        [NonSerialized]
#endif
    private readonly SynchronizationContext _context;
    private bool _isDisposed;
    private readonly IList<T> _items = new List<T>();
#if NETSTANDARD2_0 || NETFULL
        [NonSerialized]
#endif
    private readonly ReaderWriterLockSlim _itemsLocker = new ReaderWriterLockSlim();
#if NETSTANDARD2_0 || NETFULL
        [NonSerialized]
#endif
    private readonly SimpleMonitor _monitor = new SimpleMonitor();
#if NETSTANDARD2_0 || NETFULL
        [NonSerialized]
#endif
    private object _syncRoot;

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add { PropertyChanged += value; }
        remove { PropertyChanged -= value; }
    }

    /// <summary>Occurs when an item is added, removed, changed, moved, or the entire list is refreshed.</summary>
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    /// <summary>Occurs when a property value changes.</summary>
    protected event PropertyChangedEventHandler PropertyChanged;

    /// <summary>Gets a value indicating whether the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>true if the <see cref="SynchronizedObservableCollection{T}" /> has a fixed size; otherwise, false.</returns>
    protected bool IsFixedSize => false;

    bool IList.IsFixedSize => IsFixedSize;

    /// <summary>Gets a value indicating whether the <see cref="SynchronizedObservableCollection{T}" /> is read-only.</summary>
    /// <returns>true if the <see cref="SynchronizedObservableCollection{T}" /> is read-only; otherwise, false.</returns>
    protected bool IsReadOnly => false;

    bool ICollection<T>.IsReadOnly => IsReadOnly;

    bool IList.IsReadOnly => IsReadOnly;

    /// <summary>Gets a value indicating whether access to the <see cref="SynchronizedObservableCollection{T}" /> is synchronized (thread safe).</summary>
    /// <returns>true if access to the <see cref="SynchronizedObservableCollection{T}" /> is synchronized (thread safe); otherwise, false.</returns>
    protected bool IsSynchronized => true;

    bool ICollection.IsSynchronized => IsSynchronized;

    /// <summary>Gets an object that can be used to synchronize access to the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>An object that can be used to synchronize access to the <see cref="SynchronizedObservableCollection{T}" />.</returns>
    protected object SyncRoot
    {
        get
        {
            // ReSharper disable once InvertIf
            if (_syncRoot == null)
            {
                _itemsLocker.EnterReadLock();

                try
                {
                    var collection = _items as ICollection;
                    if (collection != null)
                    {
                        _syncRoot = collection.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
                    }
                }
                finally
                {
                    _itemsLocker.ExitReadLock();
                }
            }

            return _syncRoot;
        }
    }

    object ICollection.SyncRoot => SyncRoot;

    object IList.this[int index]
    {
        get { return this[index]; }
        set
        {
            try
            {
                this[index] = (T)value;
            }
            catch (InvalidCastException)
            {
                throw new ArgumentException("'value' is the wrong type");
            }
        }
    }

    /// <summary>
    /// Gets the <see cref="SynchronizationContext"/> that events will be invoked on.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public SynchronizationContext Context => _context;

    /// <summary>Gets the number of elements actually contained in the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>The number of elements actually contained in the <see cref="SynchronizedObservableCollection{T}" />.</returns>
    public int Count
    {
        get
        {
            _itemsLocker.EnterReadLock();

            try
            {
                return _items.Count;
            }
            finally
            {
                _itemsLocker.ExitReadLock();
            }
        }
    }

    /// <summary>Gets or sets the element at the specified index.</summary>
    /// <returns>The element at the specified index.</returns>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="SynchronizedObservableCollection{T}.Count" />. </exception>
    public T this[int index]
    {
        get
        {
            _itemsLocker.EnterReadLock();

            try
            {
                CheckIndex(index);

                return _items[index];
            }
            finally
            {
                _itemsLocker.ExitReadLock();
            }
        }
        set
        {
            T oldValue;

            _itemsLocker.EnterWriteLock();

            try
            {
                CheckIndex(index);
                CheckReentrancy();

                oldValue = _items[index];

                _items[index] = value;

            }
            finally
            {
                _itemsLocker.ExitWriteLock();
            }

            OnNotifyItemReplaced(value, oldValue, index);
        }
    }

    private IDisposable BlockReentrancy()
    {
        _monitor.Enter();

        return _monitor;
    }

    // ReSharper disable once UnusedParameter.Local
    private void CheckIndex(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            throw new ArgumentOutOfRangeException();
        }
    }

    private void CheckReentrancy()
    {
        if (_monitor.Busy && CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
        {
            throw new InvalidOperationException("SynchronizedObservableCollection reentrancy not allowed");
        }
    }

    private static SynchronizationContext GetCurrentSynchronizationContext()
    {
        return SynchronizationContext.Current ?? new SynchronizationContext();
    }

    private static bool IsCompatibleObject(object value)
    {
        // Non-null values are fine.  Only accept nulls if T is a class or Nullable<U>.
        // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
        return ((value is T) || (value == null && default(T) == null));
    }

    private void OnNotifyCollectionReset()
    {
        using (BlockReentrancy())
        {
            _context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }, null);
        }
    }

    private void OnNotifyItemAdded(T item, int index)
    {
        using (BlockReentrancy())
        {
            _context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }, null);
        }
    }

    private void OnNotifyItemMoved(T item, int newIndex, int oldIndex)
    {
        using (BlockReentrancy())
        {
            _context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
            }, null);
        }
    }

    private void OnNotifyItemRemoved(T item, int index)
    {
        using (BlockReentrancy())
        {
            _context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }, null);
        }
    }

    private void OnNotifyItemReplaced(T newItem, T oldItem, int index)
    {
        using (BlockReentrancy())
        {
            _context.Send(state =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
            }, null);
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SynchronizedObservableCollection{T}"/>.
    /// </summary>
    /// <param name="disposing">Not used.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _itemsLocker.Dispose();
        _isDisposed = true;
    }

    /// <summary>Adds an object to the end of the <see cref="SynchronizedObservableCollection{T}" />. </summary>
    /// <param name="item">The object to be added to the end of the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
    public void Add(T item)
    {
        _itemsLocker.EnterWriteLock();

        int index;

        try
        {
            CheckReentrancy();

            index = _items.Count;

            _items.Insert(index, item);
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyItemAdded(item, index);
    }

    int IList.Add(object value)
    {
        _itemsLocker.EnterWriteLock();

        int index;
        T item;

        try
        {
            CheckReentrancy();

            index = _items.Count;
            item = (T)value;

            _items.Insert(index, item);
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException("'value' is the wrong type");
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyItemAdded(item, index);

        return index;
    }

    /// <summary>Removes all elements from the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    public void Clear()
    {
        _itemsLocker.EnterWriteLock();

        try
        {
            CheckReentrancy();

            _items.Clear();
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyCollectionReset();
    }

    /// <summary>Copies the <see cref="SynchronizedObservableCollection{T}" /> elements to an existing one-dimensional <see cref="System.Array" />, starting at the specified array index.</summary>
    /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="SynchronizedObservableCollection{T}" />. The <see cref="System.Array" /> must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="array" /> is null.</exception>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="arrayIndex" /> is less than zero.</exception>
    /// <exception cref="T:System.ArgumentException">The number of elements in the source <see cref="SynchronizedObservableCollection{T}" /> is greater than the available space from <paramref name="arrayIndex" /> to the end of the destination <paramref name="array" />.</exception>
    public void CopyTo(T[] array, int arrayIndex)
    {
        Ensure.IsNotNull(nameof(array), array);
        Ensure.IsInRange(nameof(arrayIndex), arrayIndex >= 0 && arrayIndex < array.Length);
        Ensure.IsValid(nameof(arrayIndex), array.Length - arrayIndex >= Count, "Invalid offset length.");

        _itemsLocker.EnterReadLock();

        try
        {
            _items.CopyTo(array, arrayIndex);
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
        Ensure.IsNotNull(nameof(array), array);
        Ensure.IsValid(nameof(array), array.Rank == 1, "Multidimensional array are not supported");
        Ensure.IsValid(nameof(array), array.GetLowerBound(0) == 0, "Non-zero lower bound is not supported");
        Ensure.IsInRange(nameof(arrayIndex), arrayIndex >= 0 && arrayIndex < array.Length);
        Ensure.IsValid(nameof(arrayIndex), array.Length - arrayIndex >= Count, "Invalid offset length.");

        _itemsLocker.EnterReadLock();

        try
        {
            var tArray = array as T[];
            if (tArray != null)
            {
                _items.CopyTo(tArray, arrayIndex);
            }
            else
            {

#if NETSTANDARD2_0 || NETFULL
                    //
                    // Catch the obvious case assignment will fail.
                    // We can found all possible problems by doing the check though.
                    // For example, if the element type of the Array is derived from T,
                    // we can't figure out if we can successfully copy the element beforehand.
                    //
                    var targetType = array.GetType().GetElementType();
                    var sourceType = typeof (T);
                    if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType)))
                    {
                        throw new ArrayTypeMismatchException("Invalid array type");
                    }
#endif

                //
                // We can't cast array of value type to object[], so we don't support 
                // widening of primitive types here.
                //
                var objects = array as object[];
                if (objects == null)
                {
                    throw new ArrayTypeMismatchException("Invalid array type");
                }

                var count = _items.Count;
                try
                {
                    for (var i = 0; i < count; i++)
                    {
                        objects[arrayIndex++] = _items[i];
                    }
                }
                catch (ArrayTypeMismatchException)
                {
                    throw new ArrayTypeMismatchException("Invalid array type");
                }
            }
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    /// <summary>Determines whether an element is in the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>true if <paramref name="item" /> is found in the <see cref="SynchronizedObservableCollection{T}" />; otherwise, false.</returns>
    /// <param name="item">The object to locate in the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
    public bool Contains(T item)
    {
        _itemsLocker.EnterReadLock();

        try
        {
            return _items.Contains(item);
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    bool IList.Contains(object value)
    {
        if (!IsCompatibleObject(value))
        {
            return false;
        }

        _itemsLocker.EnterReadLock();

        try
        {
            return _items.Contains((T)value);
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    /// <summary>
    /// Releases all resources used by the <see cref="SynchronizedObservableCollection{T}"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Returns an enumerator that iterates through the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>An <see cref="T:System.Collections.Generic.IEnumerator`1" /> for the <see cref="SynchronizedObservableCollection{T}" />.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        _itemsLocker.EnterReadLock();

        try
        {
            return _items.ToList().GetEnumerator();
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        // ReSharper disable once RedundantCast
        return (IEnumerator)GetEnumerator();
    }

    /// <summary>Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>The zero-based index of the first occurrence of <paramref name="item" /> within the entire <see cref="SynchronizedObservableCollection{T}" />, if found; otherwise, -1.</returns>
    /// <param name="item">The object to locate in the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
    public int IndexOf(T item)
    {
        _itemsLocker.EnterReadLock();

        try
        {
            return _items.IndexOf(item);
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    int IList.IndexOf(object value)
    {
        if (!IsCompatibleObject(value))
        {
            return -1;
        }

        _itemsLocker.EnterReadLock();

        try
        {
            return _items.IndexOf((T)value);
        }
        finally
        {
            _itemsLocker.ExitReadLock();
        }
    }

    /// <summary>Inserts an element into the <see cref="SynchronizedObservableCollection{T}" /> at the specified index.</summary>
    /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
    /// <param name="item">The object to insert. The value can be null for reference types.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is greater than <see cref="SynchronizedObservableCollection{T}.Count" />.</exception>
    public void Insert(int index, T item)
    {
        _itemsLocker.EnterWriteLock();

        try
        {
            CheckReentrancy();

            if (index < 0 || index > _items.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            _items.Insert(index, item);
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyItemAdded(item, index);
    }

    void IList.Insert(int index, object value)
    {
        try
        {
            Insert(index, (T)value);
        }
        catch (InvalidCastException)
        {
            throw new ArgumentException("'value' is the wrong type");
        }
    }

    /// <summary>Moves the item at the specified index to a new location in the collection.</summary>
    /// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
    /// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
    public void Move(int oldIndex, int newIndex)
    {
        T item;

        _itemsLocker.EnterWriteLock();

        try
        {
            CheckReentrancy();
            CheckIndex(oldIndex);
            CheckIndex(newIndex);

            item = _items[oldIndex];

            _items.RemoveAt(oldIndex);
            _items.Insert(newIndex, item);
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyItemMoved(item, newIndex, oldIndex);
    }

    /// <summary>Removes the first occurrence of a specific object from the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <returns>true if <paramref name="item" /> is successfully removed; otherwise, false.  This method also returns false if <paramref name="item" /> was not found in the original <see cref="SynchronizedObservableCollection{T}" />.</returns>
    /// <param name="item">The object to remove from the <see cref="SynchronizedObservableCollection{T}" />. The value can be null for reference types.</param>
    public bool Remove(T item)
    {
        int index;
        T value;

        _itemsLocker.EnterWriteLock();

        try
        {
            CheckReentrancy();

            index = _items.IndexOf(item);

            if (index < 0)
            {
                return false;
            }

            value = _items[index];

            _items.RemoveAt(index);
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyItemRemoved(value, index);

        return true;
    }

    void IList.Remove(object value)
    {
        if (IsCompatibleObject(value))
        {
            Remove((T)value);
        }
    }

    /// <summary>Removes the element at the specified index of the <see cref="SynchronizedObservableCollection{T}" />.</summary>
    /// <param name="index">The zero-based index of the element to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// <paramref name="index" /> is less than zero.-or-<paramref name="index" /> is equal to or greater than <see cref="SynchronizedObservableCollection{T}.Count" />.</exception>
    public void RemoveAt(int index)
    {
        T value;

        _itemsLocker.EnterWriteLock();

        try
        {
            CheckIndex(index);
            CheckReentrancy();

            value = _items[index];

            _items.RemoveAt(index);
        }
        finally
        {
            _itemsLocker.ExitWriteLock();
        }

        OnNotifyItemRemoved(value, index);
    }

    private class SimpleMonitor : IDisposable
    {
        private int _busyCount;

        public bool Busy => _busyCount > 0;

        public void Enter()
        {
            ++_busyCount;
        }

        public void Dispose()
        {
            --_busyCount;
        }
    }
}