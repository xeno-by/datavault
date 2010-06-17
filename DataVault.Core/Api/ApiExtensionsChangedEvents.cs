using System;
using DataVault.Core.Api.Events;

namespace DataVault.Core.Api
{
    public static class ApiExtensionsChangedEvents
    {
        public static IVault Changed(this IVault vault, Action<ElementChangedEventArgs> listener)
        {
            return vault.Changed(e => true, listener);
        }

        // VALUE-SPECIFIC

        public static IVault ValueChanged(this IVault vault, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IVault ValueChanged(this IVault vault, Func<ValueChangedEventArgs, bool> filter, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue && filter(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)),
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Changed(this IValue value, Action<ValueChangedEventArgs> listener)
        {
            value.Vault.Changed(
                e => e.Subject == value,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueAdded(this IVault vault, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue && e.Reason == EventReason.Add,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Added(this IValue value, Action<ValueChangedEventArgs> listener)
        {
            value.Vault.Changed(
                e => e.Subject == value && e.Reason == EventReason.Add,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueRemoved(this IVault vault, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue && e.Reason == EventReason.Remove,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Removed(this IValue value, Action<ValueChangedEventArgs> listener)
        {
            value.Vault.Changed(
                e => e.Subject == value && e.Reason == EventReason.Remove,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueRenamed(this IVault vault, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue && e.Reason == EventReason.Rename,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Renamed(this IValue value, Action<ValueChangedEventArgs> listener)
        {
            value.Vault.Changed(
                e => e.Subject == value && e.Reason == EventReason.Rename,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueContentChanged(this IVault vault, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue && e.Reason == EventReason.Content,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue ContentChanged(this IValue value, Action<ValueChangedEventArgs> listener)
        {
            value.Vault.Changed(
                e => e.Subject == value && e.Reason == EventReason.Content,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueMetadataChanged(this IVault vault, Action<ValueChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IValue && e.Reason == EventReason.Metadata,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue MetadataChanged(this IValue value, Action<ValueChangedEventArgs> listener)
        {
            value.Vault.Changed(
                e => e.Subject == value && e.Reason == EventReason.Metadata,
                e => listener(new ValueChangedEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        // BRANCH-SPECIFIC

        public static IVault BranchChanged(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IVault BranchChanged(this IVault vault, Func<BranchChangedEventArgs, bool> filter, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && filter(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)),
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Changed(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchAdded(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && e.Reason == EventReason.Add,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Added(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch && e.Reason == EventReason.Add,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchRemoved(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && e.Reason == EventReason.Remove,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Removed(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch && e.Reason == EventReason.Remove,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchRenamed(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && e.Reason == EventReason.Rename,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Renamed(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch && e.Reason == EventReason.Rename,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchElementAdded(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && e.Reason == EventReason.ElementAdd,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch ElementAdded(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch && e.Reason == EventReason.ElementAdd,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchElementRemoved(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && e.Reason == EventReason.ElementRemove,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch ElementRemoved(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch && e.Reason == EventReason.ElementRemove,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchMetadataChanged(this IVault vault, Action<BranchChangedEventArgs> listener)
        {
            return vault.Changed(
                e => e.Subject is IBranch && e.Reason == EventReason.Metadata,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch MetadataChanged(this IBranch branch, Action<BranchChangedEventArgs> listener)
        {
            branch.Vault.Changed(
                e => e.Subject == branch && e.Reason == EventReason.Metadata,
                e => listener(new BranchChangedEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }
    }
}