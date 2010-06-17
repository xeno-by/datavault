using System;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Impl.Memory
{
    internal class InMemoryVault : VaultBase
    {
        public override String Uri { get { throw new NotSupportedException(); } }

        public InMemoryVault()
        {
            using (InternalExpose())
            {
                Root = new Branch(this, null, null);

                (Id != Guid.Empty).AssertTrue();
                (Revision is UInt64).AssertTrue();
            }
        }

        public override IVault Save() { throw new NotSupportedException(); }
        public override IVault SaveAs(String uri) { throw new NotSupportedException(); }
        public override IVault Backup() { throw new NotSupportedException(); }
    }
}
