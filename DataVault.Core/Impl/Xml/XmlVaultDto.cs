using System.Xml.Serialization;
using DataVault.Core.Api;

namespace DataVault.Core.Impl.Xml
{
    [XmlRoot("vault")]
    public class XmlVaultDto
    {
        private XmlBranchDto _root;

        [XmlElement("root")]
        public XmlBranchDto Root
        {
            get { return _root ?? new XmlBranchDto(); }
            set { _root = value; }
        }

        public static XmlVaultDto FromRootBranch(IVault vault)
        {
            var root = XmlBranchDto.FromBranch(vault.Root ?? new Branch((VaultBase)vault, null, null));
            return new XmlVaultDto{Root = root};
        }

        public IBranch ToRootBranch(IVault vault)
        {
            return Root.ToBranch(vault);
        }
    }
}