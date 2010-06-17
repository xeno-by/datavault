using System;
using System.Collections.Generic;

namespace DataVault.Core.Helpers.Collections
{
    public class ItemListChangeEventArgs<T> : EventArgs
    {
        // Fields
        private ItemListChangeAction action;
        private ICollection<T> addedItems;
        private int index;
        private ICollection<T> removedItems;

        // Methods
        public ItemListChangeEventArgs(int index, ICollection<T> removedItems, ICollection<T> addedItems, ItemListChangeAction action)
        {
            this.action = ItemListChangeAction.Add;
            this.index = index;
            this.removedItems = removedItems;
            this.addedItems = addedItems;
            this.action = action;
        }

        public ItemListChangeEventArgs(int index, T removedActivity, T addedActivity, ItemListChangeAction action)
        {
            this.action = ItemListChangeAction.Add;
            this.index = index;
            if (removedActivity != null)
            {
                this.removedItems = new List<T>();
                ((List<T>)this.removedItems).Add(removedActivity);
            }
            if (addedActivity != null)
            {
                this.addedItems = new List<T>();
                ((List<T>)this.addedItems).Add(addedActivity);
            }
            this.action = action;
        }

        // Properties
        public ItemListChangeAction Action
        {
            get
            {
                return this.action;
            }
        }

        public IList<T> AddedItems
        {
            get
            {
                if (this.addedItems == null)
                {
                    return new List<T>().AsReadOnly();
                }
                return new List<T>(this.addedItems).AsReadOnly();
            }
        }

        public int Index
        {
            get
            {
                return this.index;
            }
        }

        public IList<T> RemovedItems
        {
            get
            {
                if (this.removedItems == null)
                {
                    return new List<T>().AsReadOnly();
                }
                return new List<T>(this.removedItems).AsReadOnly();
            }
        }
    }
}