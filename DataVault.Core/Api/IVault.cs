using System;
using DataVault.Core.Api.Events;

namespace DataVault.Core.Api
{
    public interface IVault : IBranch, IDisposable
    {
        String Uri { get; }
        UInt64 Revision { get; }

        IBranch Root { get; }

        IVault Save();
        IVault SaveAs(String uri);
        IVault Backup();

        // todo. add support for nested transactions via STM, also expose txstart/txcommit/txrollback events

        IDisposable ExposeReadOnly(out int prevRecursiveExpositionsRO, out int prevRecursiveExpositionsRW);

        // todo. fix usages of this signature and its overload, so that r/w of system values don't cause ExposeRW
        // the latter is dangerous since it causes unexpected r -> w transition and, consequently,
        // might cause unexpected alien thread hijacks into seemingly safe code blocks
        IDisposable ExposeReadWrite(out int prevRecursiveExpositionsRO, out int prevRecursiveExpositionsRW);

        // todo. add weights to events
        // todo. somehow add possibility to make events fire as follows: always, on top commit, on some predicate of current txn and its state
        IVault Changing(Func<ElementChangingEventArgs, bool> filter, Action<ElementChangingEventArgs> listener);
        IVault Changed(Func<ElementChangedEventArgs, bool> filter, Action<ElementChangedEventArgs> listener);
    }
}