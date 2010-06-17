using System;

namespace DataVault.Core.Api.Events
{
    public class ValueChangedEventArgs : ElementChangedEventArgs
    {
        public new IValue Subject { get { return (IValue)base.Subject; } }

        public ValueChangedEventArgs(Guid correlationId, EventReason reason, IValue subject, UInt64 revision, Object oldValue, Object newValue)
            : base(correlationId, reason, subject, revision, oldValue, newValue)
        {
        }
    }
}