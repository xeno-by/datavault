using System;

namespace DataVault.Core.Api.Events
{
    public class BranchChangingEventArgs : ElementChangingEventArgs
    {
        public new IBranch Subject { get { return (IBranch)base.Subject; } }

        public BranchChangingEventArgs(Guid correlationId, EventReason reason, IBranch subject, UInt64 revision, Object oldValue, Object newValue)
            : base(correlationId, reason, subject, revision, oldValue, newValue)
        {
        }
    }
}