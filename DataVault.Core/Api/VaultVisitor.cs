using DataVault.Core.Helpers;
using DataVault.Core.Impl;

namespace DataVault.Core.Api
{
    // todo. it's currently unclear what should the visitor do in order to preserve internal properties
    // so far, I just put aside that issue since there are much more important things to do

    public abstract class VaultVisitor
    {
        public virtual IVault Visit(IVault copyCat, IVault vault)
        {
            using (((VaultBase)copyCat).InternalExpose())
            {
                using (vault.ExposeReadOnly())
                {
                    Visit(copyCat, vault.Root);
                    return copyCat;
                }
            }
        }

        protected virtual IBranch Visit(IVault copyCat, IBranch branch)
        {
            var copy = copyCat.GetOrCreateBranch(branch.VPath);
            copy.SetEntireMetadata(branch.Metadata);
            branch.GetBranches().ForEach(b => Visit(copyCat, b));
            ((Branch)branch).GetValues(ValueKind.Regular).ForEach(v => Visit(copyCat, (IValue)v));
            ((Branch)branch).GetValues(ValueKind.Internal).ForEach(v => VisitInternal(copyCat, v));
            return copy;
        }

        protected virtual IValue Visit(IVault copyCat, IValue value)
        {
            var copy = copyCat.CreateValue(value.VPath);
            copy.SetContent(value.ContentStream).SetEntireMetadata(value.Metadata);
            return copy;
        }

        private IValue VisitInternal(IVault copyCat, IValue internalValue)
        {
            var copy = copyCat.CreateValue(internalValue.VPath);
            copy.SetContent(internalValue.ContentStream).SetEntireMetadata(internalValue.Metadata);
            return copy;
        }
    }
}