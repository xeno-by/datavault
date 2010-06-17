using System;
using System.Collections;
using System.Collections.Generic;

namespace DataVault.Core.Impl.Optimize
{
    internal class IndexedNamedObjectCollection<T> : IEnumerable<T>
        where T : INamedObject
    {
        private Dictionary<String, T> _index = new Dictionary<String, T>();
        private List<T> _storage = new List<T>();

        public T this[String name]
        {
            get
            {
                return _index.ContainsKey(name) ? _index[name] : default(T);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _storage.GetEnumerator();
        }

        public void Add(T namedObj)
        {
            _index.Add(namedObj.Name, namedObj);
            _storage.Add(namedObj);
        }

        public bool Remove(T namedObj)
        {
            var hit = _index.Remove(namedObj.Name);
            if (hit)
            {
                _storage.Remove(namedObj);
            }
            else
            {
                // hack that makes renaming work
                _storage.Remove(namedObj);
            }

            return hit;
        }

        public void Clear()
        {
            _index.Clear();
            _storage.Clear();
        }

        public int Count
        {
            get
            {
                return _index.Count;
            }
        }
    }
}