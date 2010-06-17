using System;
using DataVault.Core.Helpers.Reflection.Shortcuts;

namespace DataVault.Core.Api.Events
{
    public abstract class ElementEventArgs : EventArgs
    {
        public Guid Id { get; private set; }
        public Guid CorrelationId { get; private set; }
        public EventReason Reason { get; private set; }
        public IElement Subject { get; private set; }
        public UInt64 OldRevision { get; private set; }

        private Object _oldValue;
        public Object OldValue
        {
            get
            {
                var lazy = _oldValue as Func<Object>;
                if (lazy != null)
                {
                    _oldValue = lazy();
                    return OldValue;
                }
                else
                {
                    return _oldValue;
                }
            }

            private set
            {
                _oldValue = value;
            }
        }

        private Object _newValue;
        public Object NewValue
        {
            get
            {
                var lazy = _newValue as Func<Object>;
                if (lazy != null)
                {
                    _newValue = lazy();
                    return NewValue;
                }
                else
                {
                    return _newValue;
                }
            }

            private set
            {
                _newValue = value;
            }
        }

        protected ElementEventArgs(Guid correlationId, EventReason reason, IElement subject, UInt64 revision, Object oldValue, Object newValue)
        {
            Id = Guid.NewGuid();
            Reason = reason;
            Subject = subject;
            OldRevision = revision;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public override String ToString()
        {
            Func<Object, String> tos = o => o == null ? "null" : o.ToString();
            return String.Format("[{0}: {1}] {2} -> {3}", Reason, Subject, tos(OldValue), tos(NewValue));
        }

        internal String ToStringThatsFriendlyToUnitTests()
        {
            Func<Object, String> friendly = o =>
            {
                if (o == null)
                {
                    return "null";
                }
                else
                {
                    var f = o.GetType().GetMethod("ToStringThatsFriendlyToUnitTests", BF.All);
                    if (f != null)
                    {
                        return (String)f.Invoke(o, null);
                    }
                    else
                    {
                        return o.ToString();
                    }
                }
            };

            return String.Format("[{0}: {1}] {2} -> {3}", Reason, Subject, friendly(OldValue), friendly(NewValue));
        }
    }
}