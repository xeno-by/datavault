using System;
using System.Collections;
using System.Collections.Generic;

namespace DataVault.Core.Api
{
    public abstract class IMetadata : IDictionary<String, String>, ICloneable
    {
        public abstract IElement Element { get; }
        public abstract IMetadata InitializeFrom(IMetadata external);

        public String Default
        {
            get { return this[CoreConstants.DefaultSection]; }
            set { this[CoreConstants.DefaultSection] = value; }
        }

        public static implicit operator String(IMetadata metadata)
        {
            return metadata[CoreConstants.DefaultSection];
        }

        #region Implementation of interfaces

        IEnumerator IEnumerable.GetEnumerator() {return GetEnumerator(); }
        public abstract IEnumerator<KeyValuePair<String, String>> GetEnumerator();

        public abstract void Add(KeyValuePair<String, String> item);
        public abstract void Clear();
        public abstract bool Contains(KeyValuePair<String, String> item);
        public abstract void CopyTo(KeyValuePair<String, String>[] array, int arrayIndex);
        public abstract bool Remove(KeyValuePair<String, String> item);
        public abstract int Count { get; }
        public bool IsReadOnly { get { return false; } }

        public abstract void Add(String key, String value);
        public abstract bool ContainsKey(String key);
        public abstract bool Remove(String key);
        public abstract bool TryGetValue(String key, out String value);
        public abstract String this[String key] { get; set; }
        public abstract ICollection<String> Keys { get; }
        public abstract ICollection<String> Values { get; }

        object ICloneable.Clone() { return Clone(); }
        public abstract IMetadata Clone();

        #endregion
    }
}