using System;
using DataVault.Core.Api.Events;

namespace DataVault.Core.Api
{
    public static class ApiExtensionsChangingEvents
    {
        public static IVault Changing(this IVault vault, Action<ElementChangingEventArgs> listener)
        {
            return vault.Changing(e => true, listener);
        }

        // VALUE-SPECIFIC

        public static IVault ValueChanging(this IVault vault, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IVault ValueChanging(this IVault vault, Func<ValueChangingEventArgs, bool> filter, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue && filter(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)),
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Changing(this IValue value, Action<ValueChangingEventArgs> listener)
        {
            value.Vault.Changing(
                e => e.Subject == value,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueAdding(this IVault vault, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue && e.Reason == EventReason.Add,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Adding(this IValue value, Action<ValueChangingEventArgs> listener)
        {
            value.Vault.Changing(
                e => e.Subject == value && e.Reason == EventReason.Add,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueRemoving(this IVault vault, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue && e.Reason == EventReason.Remove,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Removing(this IValue value, Action<ValueChangingEventArgs> listener)
        {
            value.Vault.Changing(
                e => e.Subject == value && e.Reason == EventReason.Remove,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueRenaming(this IVault vault, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue && e.Reason == EventReason.Rename,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue Renaming(this IValue value, Action<ValueChangingEventArgs> listener)
        {
            value.Vault.Changing(
                e => e.Subject == value && e.Reason == EventReason.Rename,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueContentChanging(this IVault vault, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue && e.Reason == EventReason.Content,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue ContentChanging(this IValue value, Action<ValueChangingEventArgs> listener)
        {
            value.Vault.Changing(
                e => e.Subject == value && e.Reason == EventReason.Content,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        public static IVault ValueMetadataChanging(this IVault vault, Action<ValueChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IValue && e.Reason == EventReason.Metadata,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IValue MetadataChanging(this IValue value, Action<ValueChangingEventArgs> listener)
        {
            value.Vault.Changing(
                e => e.Subject == value && e.Reason == EventReason.Metadata,
                e => listener(new ValueChangingEventArgs(e.CorrelationId, e.Reason, (IValue)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return value;
        }

        // BRANCH-SPECIFIC

        public static IVault BranchChanging(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IVault BranchChanging(this IVault vault, Func<BranchChangingEventArgs, bool> filter, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && filter(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)),
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Changing(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchAdding(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && e.Reason == EventReason.Add,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Adding(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch && e.Reason == EventReason.Add,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchRemoving(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && e.Reason == EventReason.Remove,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Removing(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch && e.Reason == EventReason.Remove,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchRenaming(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && e.Reason == EventReason.Rename,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch Renaming(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch && e.Reason == EventReason.Rename,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchElementAdding(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && e.Reason == EventReason.ElementAdd,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch ElementAdding(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch && e.Reason == EventReason.ElementAdd,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchElementRemoving(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && e.Reason == EventReason.ElementRemove,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch ElementRemoving(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch && e.Reason == EventReason.ElementRemove,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }

        public static IVault BranchMetadataChanging(this IVault vault, Action<BranchChangingEventArgs> listener)
        {
            return vault.Changing(
                e => e.Subject is IBranch && e.Reason == EventReason.Metadata,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
        }

        public static IBranch MetadataChanging(this IBranch branch, Action<BranchChangingEventArgs> listener)
        {
            branch.Vault.Changing(
                e => e.Subject == branch && e.Reason == EventReason.Metadata,
                e => listener(new BranchChangingEventArgs(e.CorrelationId, e.Reason, (IBranch)e.Subject, e.OldRevision, e.OldValue, e.NewValue)));
            return branch;
        }
    }
}