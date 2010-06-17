using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Helpers.Collections
{
    public class StackSlim<T> : IEnumerable<T>
    {
        private List<T> _impl = new List<T>();

        public void Push(T item)
        {
            _impl.Add(item);

            if (ItemPushed != null)
                ItemPushed.Invoke(this, new StackItemEventArgs<T>(item));
        }

        public T Pop()
        {
            _impl.IsNotEmpty().AssertTrue();
            var popt = _impl.Last();
            _impl.RemoveAt(_impl.Count - 1);

            if (ItemPopped != null)
                ItemPopped.Invoke(this, new StackItemEventArgs<T>(popt));
            return popt;
        }

        public T Peek()
        {
            _impl.IsNotEmpty().AssertTrue();
            return _impl.Last();
        }

        public event EventHandler<StackItemEventArgs<T>> ItemPushed;
        public event EventHandler<StackItemEventArgs<T>> ItemPopped;

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_impl).Reverse().GetEnumerator();
        }
    }
}
