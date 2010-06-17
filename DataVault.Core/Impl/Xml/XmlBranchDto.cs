using System;
using System.Xml.Serialization;
using DataVault.Core.Api;
using System.Linq;
using DataVault.Core.Impl.Api;

namespace DataVault.Core.Impl.Xml
{
    [XmlRoot("branch")]
    public class XmlBranchDto
    {
        [XmlAttribute("name")]
        public String Name { get; set; }

        [XmlElement("metadata")]
        public String Metadata { get; set; }

        private XmlValueDto[] _values;

        [XmlArray("values")]
        [XmlArrayItem("value")]
        public XmlValueDto[] Values
        {
            get { return _values ?? new XmlValueDto[0]; }
            set { _values = value; }
        }

        private XmlBranchDto[] _branches;

        [XmlArray("branches")]
        [XmlArrayItem("branch")]
        public XmlBranchDto[] Branches
        {
            get { return _branches ?? new XmlBranchDto[0]; }
            set { _branches = value; }
        }

        public static XmlBranchDto FromBranch(IBranch branch)
        {
            return new XmlBranchDto 
            {
                Name = branch.Name,
                Metadata = ((Branch)branch).Metadata.Raw,
                Values = ((Branch)branch).GetValues(ValueKind.RegularAndInternal).Select(v => XmlValueDto.FromValue(v)).ToArray(),
                Branches = branch.GetBranches().Select(b => XmlBranchDto.FromBranch(b)).ToArray(),
            };
        }

        public IBranch ToBranch(IVault vault)
        {
            // fixup xmlwriter's error
            Name = Name == null ? null : Name.Replace("\n", Environment.NewLine);
            Metadata = Metadata == null ? null : Metadata.Replace("\n", Environment.NewLine);

            var values = Values.Select(v => v.ToValue(vault)).Cast<Element>();
            var branches = Branches.Select(b => b.ToBranch(vault)).Cast<Element>();

            var branch = new Branch((VaultBase)vault, Name, values.Concat(branches));
            return branch.RawSetMetadata(Metadata);
        }
    }
}