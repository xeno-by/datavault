using System;
using System.Xml.Serialization;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Impl.Api;

namespace DataVault.Core.Impl.Xml
{
    [XmlRoot("value")]
    public class XmlValueDto
    {
        [XmlAttribute("name")]
        public String Name { get; set; }

        [XmlElement("metadata")]
        public String Metadata { get; set; }

        [XmlElement("content")]
        public String Content { get; set; }

        public static XmlValueDto FromValue(IValue value)
        {
            var xvd = new XmlValueDto
            {
                Name = value.Name, 
                Metadata = ((Value)value).Metadata.Raw, 
                Content = value.ContentString
            };

            // very cheap attempt to perform link rewriting
            var vi = (Value)value;
            vi.RawSetMetadata(xvd.Metadata);
            vi.SetContent(xvd.Content.AsLazyStream());

            return xvd;
        }

        public IValue ToValue(IVault vault)
        {
            // fixup xmlwriter's error
            Name = Name == null ? null : Name.Replace("\n", Environment.NewLine);
            Metadata = Metadata == null ? null : Metadata.Replace("\n", Environment.NewLine);
            Content = Content == null ? null : Content.Replace("\n", Environment.NewLine);

            var value = new Value((VaultBase)vault, Name, Content.AsLazyStream());
            value.RawSetMetadata(Metadata);
            return value;
        }
    }
}