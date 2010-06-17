using DataVault.Core.Api;

namespace DataVault.Playground
{
    public static class RevisionHistoryHelper
    {
        public static IVault BeforeInv(this IVault vault, RevisionHistory history)
        {
            history.BeforeInv(vault);
            return vault;
        }

        public static IVault BeforeMutate(this IVault vault, RevisionHistory history)
        {
            history.BeforeMutate(vault);
            return vault;
        }

        public static IBranch BeforeInv(this IBranch branch, RevisionHistory history)
        {
            history.BeforeInv(branch.Vault);
            return branch;
        }

        public static IBranch BeforeMutate(this IBranch branch, RevisionHistory history)
        {
            history.BeforeMutate(branch.Vault);
            return branch;
        }

        public static IValue BeforeInv(this IValue value, RevisionHistory history)
        {
            history.BeforeInv(value.Vault);
            return value;
        }

        public static IValue BeforeMutate(this IValue value, RevisionHistory history)
        {
            history.BeforeMutate(value.Vault);
            return value;
        }

        public static IVault Verify(this IVault vault, RevisionHistory history)
        {
            history.Verify(vault);
            return vault;
        }
    }
}