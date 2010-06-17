using System;

namespace DataVault.Core.Api
{
    internal class VaultVisitorComposite : VaultVisitor
    {
        private readonly Func<IVault, IBranch, IBranch> _branchVisitor;
        private readonly Func<IVault, IValue, IValue> _valueVisitor;

        public VaultVisitorComposite(Func<IVault, IBranch, IBranch> branchVisitor)
            : this(branchVisitor, null)
        {
        }

        public VaultVisitorComposite(Func<IVault, IValue, IValue> valueVisitor)
            : this(null, valueVisitor)
        {
        }

        public VaultVisitorComposite(Func<IVault, IBranch, IBranch> branchVisitor, Func<IVault, IValue, IValue> valueVisitor)
        {
            _branchVisitor = (v, b) =>
            {
                if (branchVisitor == null)
                {
                    return Visit(v, b);
                }
                else
                {
                    var res = branchVisitor(v, b);
                    return res ?? Visit(v, b);
                }
            };

            _valueVisitor = (v, b) =>
            {
                if (valueVisitor == null)
                {
                    return Visit(v, b);
                }
                else
                {
                    var res = valueVisitor(v, b);
                    return res ?? Visit(v, b);
                }
            };
        }
    }
}