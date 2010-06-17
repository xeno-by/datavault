using System;

namespace DataVault.Core.Api.Events
{
    public class BranchChangedEventArgs : ElementChangedEventArgs
    {
        public new IBranch Subject { get { return (IBranch)base.Subject; } }

        public BranchChangedEventArgs(Guid correlationId, EventReason reason, IBranch subject, UInt64 revision, Object oldValue, Object newValue)
            : base(correlationId, reason, subject, revision, oldValue, newValue)
        {
        }
    }
}