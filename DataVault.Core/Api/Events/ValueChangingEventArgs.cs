using System;

namespace DataVault.Core.Api.Events
{
    public class ValueChangingEventArgs : ElementChangingEventArgs
    {
        public new IValue Subject { get { return (IValue)base.Subject; } }

        public ValueChangingEventArgs(Guid correlationId, EventReason reason, IValue subject, UInt64 revision, Object oldValue, Object newValue)
            : base(correlationId, reason, subject, revision, oldValue, newValue)
        {
        }
    }
}
