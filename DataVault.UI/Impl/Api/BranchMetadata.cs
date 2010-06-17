using System;
using DataVault.Core.Api;
using DataVault.Core.Helpers;

namespace DataVault.UI.Impl.Api
{
    internal class BranchMetadata
    {
        private IBranch _b;
        private Guid? _id;

        public Guid? Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    _b.SetMetadata(ToString());
                }
            }
        }

        private BranchMetadata(IBranch b)
        {
            _b = b;
            _id = ((String)b.Metadata).IsNullOrEmpty() ? null : (Guid?)new Guid(b.Metadata);
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("Since rev492 we don't need manual structurization of the metadata field")]
#endif
        public static BranchMetadata ForBranch(IBranch b)
        {
            return new BranchMetadata(b);
        }

        public override String ToString()
        {
            return String.Format("{0}", Id);
        }
    }
}