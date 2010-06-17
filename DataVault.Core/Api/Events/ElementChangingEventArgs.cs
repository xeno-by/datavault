using System;

namespace DataVault.Core.Api.Events
{
    public class ElementChangingEventArgs : ElementEventArgs
    {
        // no possibility to cancel, because often cancelling a modification would leave system in inconsistent state
        // however, if the change is inappropriate, you can always throw an exception

        public ElementChangingEventArgs(Guid correlationId, EventReason reason, IElement subject, UInt64 revision, Object oldValue, Object newValue)
            : base(correlationId, reason, subject, revision, oldValue, newValue)
        {
        }
    }
}