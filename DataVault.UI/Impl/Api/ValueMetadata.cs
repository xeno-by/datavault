using System;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;

namespace DataVault.UI.Impl.Api
{
    internal class ValueMetadata
    {
        private IValue _v;
        private String _typeToken;
        private Guid? _id;

        public String TypeToken
        {
            get { return _typeToken; }
            set
            {
                if (_typeToken != value)
                {
                    _typeToken = value;
                    _v.SetMetadata(ToString());
                }
            }
        }

        public Guid? Id
        {
            get { return _id; }
            set
            {
                if (_id != value)
                {
                    _id = value;
                    _v.SetMetadata(ToString());
                }
            }
        }

        private ValueMetadata(IValue v)
        {
            var s = (String)v.Metadata;
            var lines = (s ?? String.Empty).Split(new String[] {"\n"}, 
                StringSplitOptions.None).Select(line => line.Trim());
            var line0 = lines.ElementAtOrDefault(0);
            var line1 = lines.ElementAtOrDefault(1);

            _v = v;
            _typeToken = line0.IsNullOrEmpty() ? "text" : line0;
            _id = line1.IsNullOrEmpty() ? null : (Guid?)new Guid(line1);
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("Since rev492 we don't need manual structurization of the metadata field")]
#endif
        public static ValueMetadata ForValue(IValue v)
        {
            return new ValueMetadata(v);
        }

        public override String ToString()
        {
            return String.Format("{0}{1}{2}", TypeToken, Environment.NewLine, Id);
        }
    }
}