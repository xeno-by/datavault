using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Impl.Xml
{
    internal class XmlVault : VaultBase
    {
        internal static readonly string SchemaUri = "DataVault.Core.Impl.Xml.DataVault.Core.xsd";

        private String _uri;
        public override String Uri { get { return Path.GetFullPath(_uri).Unslash(); } }

        public XmlVault(String fileName)
        {
            using (InternalExpose())
            {
                _uri = fileName;
                DeserializeFrom(fileName);

                // for backward compatibility (DO NOT REMOVE again, please)
                var oldVersionHolder = Root.GetValue("version$");
                if (oldVersionHolder != null)
                {
                    var possibleOverlap = Root.GetValue("$version");
                    if (possibleOverlap == null)
                    {
                        var versionHolder = Root.CreateValue("$version");
                        versionHolder.SetContent(oldVersionHolder.ContentString);
                    }
                }

                Root.AfterLoad();
                Root.GetBranchesRecursive().Cast<Branch>().ForEach(b => b.AfterLoad());
                Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<Value>().ForEach(v => v.AfterLoad());

                (Id != Guid.Empty).AssertTrue();
                (Revision is UInt64).AssertTrue();
            }
        }

        public override IVault Save()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    SerializeTo(_uri);
                    return this;
                }
            }
        }

        public override IVault SaveAs(String uri)
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    Id = Guid.NewGuid();
                    Revision = 0;

                    SerializeTo(uri);
                    _uri = uri;
                    return this;
                }
            }
        }

        public override IVault Backup()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                    var bakUri = Uri + ".bak";
                    if (File.Exists(bakUri)) File.Delete(bakUri);
                    SerializeTo(bakUri);

                    return this;
                }
            }
        }

        private void SerializeTo(String uri)
        {
            using (var sw = new StreamWriter(uri))
            {
                // claim the responsibility to fixup all tree elements
                Root.GetValuesRecursive(ValueKind.RegularAndInternal).ForEach(Bind);
                Root.GetBranchesRecursive().ForEach(Bind);

                var dto = XmlVaultDto.FromRootBranch(this);
                new XmlSerializer(typeof(XmlVaultDto)).Serialize(sw, dto);

                // set the changes in stone
                Root.AfterSave();
                Root.GetBranchesRecursive().Cast<Branch>().ForEach(b => b.AfterSave());
                Root.GetValuesRecursive(ValueKind.RegularAndInternal).Cast<Value>().ForEach(v => v.AfterSave());
            }
        }

        private void DeserializeFrom(String uri)
        {
            if (!File.Exists(uri))
            {
                Root = new Branch(this, null, null);
            }
            else
            {
                using (var sr = new StreamReader(uri))
                {
                    using (var xsd = Assembly.GetExecutingAssembly().GetManifestResourceStream(SchemaUri))
                    {
                        var cfg = new XmlReaderSettings();
                        cfg.ValidationType = ValidationType.Schema;
                        cfg.Schemas.Add(XmlSchema.Read(xsd, null));
                        var xml = XmlReader.Create(sr, cfg);

                        var dto = (XmlVaultDto)new XmlSerializer(typeof(XmlVaultDto)).Deserialize(xml);
                        Root = (Branch)dto.ToRootBranch(this);
                    }
                }
            }
        }
    }
}