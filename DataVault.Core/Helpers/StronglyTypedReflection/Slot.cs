using System;

namespace DataVault.Core.Helpers.StronglyTypedReflection
{
    public class Slot<T>
    {
        private readonly Func<T> _getter;
        public T Get() { return _getter(); }

        private readonly Action<T> _setter;
        public void Set(T value) { _setter(value); }

        public Slot(Func<T> getter, Action<T> setter)
        {
            _getter = getter;
            _setter = setter;
        }

        public T Value
        {
            get { return _getter(); }
            set { _setter(value); }
        }
    }
}