using System;

namespace DataVault.Core.Api.Events
{
    public class ElementChangedEventArgs : ElementEventArgs
    {
        public ElementChangedEventArgs(Guid correlationId, EventReason reason, IElement subject, UInt64 revision, Object oldValue, Object newValue)
            : base(correlationId, reason, subject, revision, oldValue, newValue)
        {
        }
    }
}