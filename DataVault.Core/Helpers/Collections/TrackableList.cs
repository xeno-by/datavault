using System;
using System.Collections;
using System.Collections.Generic;

namespace DataVault.Core.Helpers.Collections
{
    // made internal in order not to interfere with DataVault.Tesla

    public class TrackableList<T> : List<T>, IList<T>, IList
    {
        public event ItemListChangeEventHandler<T> ListChanged;
        public event ItemListChangeEventHandler<T> ListChanging;

        public TrackableList()
        {
        }

        public TrackableList(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public new void Add(T item)
        {
            ((ICollection<T>)this).Add(item);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(-1, null, new List<T>(collection), ItemListChangeAction.Add));
            base.AddRange(collection);
            this.FireListChanged(new ItemListChangeEventArgs<T>(base.Count, null, new List<T>(collection), ItemListChangeAction.Add));
        }

        public new void Clear()
        {
            ((ICollection<T>)this).Clear();
        }

        protected virtual void FireListChanged(ItemListChangeEventArgs<T> eventArgs)
        {
            if (this.ListChanged != null)
            {
                this.ListChanged(this, eventArgs);
            }
        }

        protected virtual void FireListChanging(ItemListChangeEventArgs<T> eventArgs)
        {
            if (this.ListChanging != null)
            {
                this.ListChanging(this, eventArgs);
            }
        }

        public new void Insert(int index, T item)
        {
            ((IList<T>)this).Insert(index, item);
        }

        public new void InsertRange(int index, IEnumerable<T> collection)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(index, null, new List<T>(collection), ItemListChangeAction.Add));
            base.InsertRange(index, collection);
            this.FireListChanged(new ItemListChangeEventArgs<T>(index, null, new List<T>(collection), ItemListChangeAction.Add));
        }

        public new bool Remove(T item)
        {
            return ((ICollection<T>)this).Remove(item);
        }

        public new void RemoveAt(int index)
        {
            ((IList<T>)this).RemoveAt(index);
        }

        void ICollection<T>.Add(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(base.Count, default(T), item, ItemListChangeAction.Add));
            base.Add(item);
            this.FireListChanged(new ItemListChangeEventArgs<T>(base.Count, default(T), item, ItemListChangeAction.Add));
        }

        void ICollection<T>.Clear()
        {
            ICollection<T> range = base.GetRange(0, base.Count);
            this.FireListChanging(new ItemListChangeEventArgs<T>(-1, range, null, ItemListChangeAction.Remove));
            base.Clear();
            this.FireListChanged(new ItemListChangeEventArgs<T>(-1, range, null, ItemListChangeAction.Remove));
        }

        bool ICollection<T>.Contains(T item)
        {
            return base.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        bool ICollection<T>.Remove(T item)
        {
            if (base.Contains(item))
            {
                var index = base.IndexOf(item);
                if (index >= 0)
                {
                    this.FireListChanging(new ItemListChangeEventArgs<T>(index, item, default(T), ItemListChangeAction.Remove));
                    base.Remove(item);
                    this.FireListChanged(new ItemListChangeEventArgs<T>(index, item, default(T), ItemListChangeAction.Remove));
                    return true;
                }
            }
            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        int IList<T>.IndexOf(T item)
        {
            return base.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException();
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            this.FireListChanging(new ItemListChangeEventArgs<T>(index, default(T), item, ItemListChangeAction.Add));
            base.Insert(index, item);
            this.FireListChanged(new ItemListChangeEventArgs<T>(index, default(T), item, ItemListChangeAction.Add));
        }

        void IList<T>.RemoveAt(int index)
        {
            if ((index < 0) || (index > base.Count))
            {
                throw new ArgumentOutOfRangeException();
            }
            var removedActivity = base[index];
            this.FireListChanging(new ItemListChangeEventArgs<T>(index, removedActivity, default(T), ItemListChangeAction.Remove));
            base.RemoveAt(index);
            this.FireListChanged(new ItemListChangeEventArgs<T>(index, removedActivity, default(T), ItemListChangeAction.Remove));
        }

        void ICollection.CopyTo(Array array, int index)
        {
            for (var i = 0; i < base.Count; i++)
            {
                array.SetValue(this[i], (int)(i + index));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return base.GetEnumerator();
        }

        int System.Collections.IList.Add(Object value)
        {
            ((ICollection<T>)this).Add((T)value);
            return (base.Count - 1);
        }

        void System.Collections.IList.Clear()
        {
            ((ICollection<T>)this).Clear();
        }

        bool IList.Contains(Object value)
        {
            return ((ICollection<T>)this).Contains((T)value);
        }

        int IList.IndexOf(Object value)
        {
            return ((IList<T>)this).IndexOf((T)value);
        }

        void IList.Insert(int index, Object value)
        {
            ((IList<T>)this).Insert(index, (T)value);
        }

        void IList.Remove(Object value)
        {
            ((ICollection<T>)this).Remove((T)value);
        }

        // Properties
        private bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public new T this[int index]
        {
            get
            {
                return ((IList<T>)this)[index];
            }
            set
            {
                ((IList<T>)this)[index] = value;
            }
        }

        int ICollection<T>.Count
        {
            get
            {
                return base.Count;
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return false;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("item");
                }
                var removedActivity = base[index];
                this.FireListChanging(new ItemListChangeEventArgs<T>(index, removedActivity, value, ItemListChangeAction.Replace));
                base[index] = value;
                this.FireListChanged(new ItemListChangeEventArgs<T>(index, removedActivity, value, ItemListChangeAction.Replace));
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        Object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return false;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return (this as ICollection<T>).IsReadOnly;
            }
        }

        Object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }
    }
}