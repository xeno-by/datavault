using System;

namespace DataVault.Core.Helpers.Collections
{
    public class StackItemEventArgs<T> : EventArgs
    {
        public T Item { get; private set; }

        public StackItemEventArgs(T item)
        {
            Item = item;
        }
    }
}