using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Impl
{
    internal class Metadata : IMetadata
    {
        private Element _element;
        public override IElement Element { get { return _element; } }
        private VaultBase Vault { get { return ((Element)Element).Vault; } }
        private VaultBase BoundVault { get { return ((Element)Element).BoundVault; } }
        private void VerifyMutation() { ((Element)Element).VerifyMutation(Element.VPath); }

        public bool SaveMePlease { get; private set; }

        public Metadata(Element element)
            : this(element, (Dictionary<String, String>)null)
        {
        }

        private Metadata(Element element, Func<Stream> lazyStream)
        {
            _element = element;
            _lazySource = lazyStream;
        }

        private Metadata(Element element, Dictionary<String, String> impl)
        {
            _element = element;
            _dontUseThisForGetUsePropertyInstead = new Dictionary<String, String>(
                impl ?? new Dictionary<String, String>());
        }

        public void CacheInMemory()
        {
            if (_lazySource != null)
            {
                _lazySource = () => _lazySource().CacheInMemory();
            }
        }

        public override IMetadata Clone()
        {
            if (_lazySource == null)
            {
                return new Metadata(_element, Impl);
            }
            else
            {
                return new Metadata(_element, _lazySource);
            }
        }

        public virtual void AfterLoad()
        {
            SaveMePlease = false;
        }

        public virtual void AfterSave()
        {
            SaveMePlease = false;
        }

        public Metadata InitializeFrom(Func<Stream> lazyStream)
        {
            using (Vault.ExposeReadWrite())
            {
                var oldMetadata = Clone();
                var newMetadata = new Metadata(_element, lazyStream);
                var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                VerifyMutation();
                _lazySource = lazyStream;
                SaveMePlease = true;

                BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
                return this;
            }
        }

        public override IMetadata InitializeFrom(IMetadata metadata)
        {
            using (Vault.ExposeReadWrite())
            {
                var oldMetadata = Clone();
                var newMetadata = metadata;
                var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                VerifyMutation();
                _lazySource = null;
                var externalImpl = metadata == null ? new Dictionary<String, String>() : ((Metadata)metadata).Impl;
                externalImpl = externalImpl ?? new Dictionary<String, String>();
                _dontUseThisForGetUsePropertyInstead = new Dictionary<String, String>(externalImpl);
                SaveMePlease = true;

                BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
                return this;
            }
        }

        private Func<Stream> _lazySource;

        // used for non-r/w actions with Impl that need to be synced
        // e.g. adding some key with a null value (to the vault this doesn't change anything,
        // but in fact this leads to changes in the dictionary and might crash under heavy concurrency)

        private Object _isomorphicWritesLock = new Object(); 
        private Dictionary<String, String> _dontUseThisForGetUsePropertyInstead;

        private Dictionary<String, String> Impl
        {
            get
            {
                if (_lazySource != null)
                {
                    var content = _lazySource().AsString();
                    _dontUseThisForGetUsePropertyInstead = ReadSections(content);
                    _lazySource = null;
                    EnsureDefault();
                }

                return _dontUseThisForGetUsePropertyInstead;
            }
        }

        public String Raw
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    return WriteSections(Impl);
                }
            }
        }

        public override string ToString()
        {
            return Raw ?? "null";

            // don't display the "$content-is-null" for the sake of debug
            // depending on the vault
        }

        internal string ToStringThatsFriendlyToUnitTests()
        {
            // don't display the "$content-is-null" for the sake of debug
            // depending on the vault it can be either present either not present in the metadata
            // at a certain moment of a test scenario (depends on what gets saved first: Content or Metadata)

            using (Vault.InternalExpose())
            {
                var friendlyClone = Clone();
                friendlyClone.Remove("$content-is-null");
                return friendlyClone.ToString();
            }
        }

        private static Dictionary<String, String> ReadSections(String source)
        {
            if (source == null)
            {
                return new Dictionary<String, String>();
            }
            else
            {
                var map = new Dictionary<String, String>();

                // 0 -> reading value
                // 1 -> just read [ (suspecting start of the section, or [ if the next symbol is [)
                // 2 -> just read [! (expecting either ! that'd escape the previous !, or [ that'd be escaped by the previous !)
                // 3 -> reading section name
                // 4 -> just read ] (suspecting end of the section, or ] if the next symbol is ])
                // 5 -> just read ]! (expecting either ! that'd escape the previous !, or ] that'd be escaped by the previous !)
                var state = 0;

                // if the very first chunk of text is section-less (backward compatibility)
                // then it gets assigned to the default section
                var section = CoreConstants.DefaultSection;
                var value = String.Empty;

                for (var i = 0; i < source.Length; ++i)
                {
                    var c = source[i];

                    // 0 -> reading value
                    if (state == 0)
                    {
                        if (c == '[')
                        {
                            state = 1;
                        }
                        else
                        {
                            value += c;
                        }
                    }
                    // 1 -> just read [ while reading content, possible outcomes:
                    // * [ if the next symbol of content is [
                    // * ! end of content, first symbol of section name needs to be escaped
                    // * end of content, start of the section name
                    else if (state == 1)
                    {
                        if (c == '[')
                        {
                            value += c;
                            state = 0;
                        }
                        else if (c == '!')
                        {
                            if (i > 1)
                            {
                                value.EndsWith(Environment.NewLine).AssertTrue();
                                map.Add(section, value.Slice(0, -2));
                            }

                            section = String.Empty;
                            state = 2;
                        }
                        else
                        {
                            if (i > 1)
                            {
                                value.EndsWith(Environment.NewLine).AssertTrue();
                                map.Add(section, value.Slice(0, -2));
                            }

                            section = String.Empty;
                            if (c == ']')
                            {
                                state = 4;
                            }
                            else
                            {
                                section += c;
                                state = 3;
                            }
                        }
                    }
                    // 2 -> just read [!, possible outcomes:
                    // * ! that'd escape the previous ! (and belong to section name)
                    // * [ that'd be escaped by the previous ! (and belong to section name)
                    else if (state == 2)
                    {
                        if (c == '!')
                        {
                            section += c;
                            state = 3;
                        }
                        else if (c == '[')
                        {
                            section += c;
                            state = 3;
                        }
                        else
                        {
                            AssertionHelper.Fail();
                        }
                    }
                    // 3 -> reading section name
                    else if (state == 3)
                    {
                        if (c == ']')
                        {
                            state = 4;
                        }
                        else
                        {
                            section += c;
                            state = 3;
                        }
                    }
                    // 4 -> just read ] while reading section name, possible outcomes:
                    // * ] if the next symbol of section name is ]
                    // * ! end of section name, first symbol of content needs to be escaped
                    // * end of section name, start of content
                    else if (state == 4)
                    {
                        if (c == ']')
                        {
                            section += c;
                            state = 3;
                        }
                        else if (c == '!')
                        {
                            section.IsNeitherNullNorEmpty().AssertTrue();
                            value = String.Empty;
                            state = 5;
                        }
                        else
                        {
                            section.IsNeitherNullNorEmpty().AssertTrue();
                            value = String.Empty;

                            if (c == '[')
                            {
                                state = 1;
                            }
                            else
                            {
                                value += c;
                                state = 0;
                            }
                        }
                    }
                    // 5 -> just read ]!, possible outcomes:
                    // * ! that'd escape the previous ! (and belong to content)
                    // * ] that'd be escaped by the previous ! (and belong to content)
                    else if (state == 5)
                    {
                        if (c == '!')
                        {
                            value += c;
                            state = 0;
                        }
                        else if (c == ']')
                        {
                            value += c;
                            state = 0;
                        }
                        else
                        {
                            AssertionHelper.Fail();
                        }
                    }
                    else
                    {
                        AssertionHelper.Fail();
                    }
                }

                if (state == 0)
                {
                    map.Add(section, value);
                }
                else if (state == 4)
                {
                    map.Add(section, String.Empty);
                }
                else
                {
                    AssertionHelper.Fail();
                }

                return map;
            }
        }

        private static String WriteSections(Dictionary<String, String> source)
        {
            var valid = source.Where(kvp => kvp.Value != null).OrderBy(kvp => kvp.Key).ToArray();
            if (valid.IsEmpty())
            {
                return null;
            }
            else if (valid.Count() == 1 && valid.Single().Key == CoreConstants.DefaultSection)
            {
                // only escape opening brackets since there's no section marks
                var content = valid.Single().Value;
                return content.Replace("[", "[[");
            }
            else
            {
                var buffer = new StringBuilder();
                for (var i = 0; i < valid.Length; ++i)
                {
                    var section = valid[i].Key;
                    if (section.StartsWith("!") || section.StartsWith("[")) section = "!" + section;
                    section = section.Replace("]", "]]");
                    buffer.Append("[" + section + "]");

                    var content = valid[i].Value;
                    if (content.StartsWith("!") || content.StartsWith("]")) content = "!" + content;
                    content = content.Replace("[", "[[");
                    buffer.Append(content);

                    if (i != valid.Length - 1) buffer.AppendLine();
                }

                return buffer.ToString();
            }
        }

        private void EnsureDefault()
        {
            using (Vault.ExposeReadOnly())
            {
                lock (_isomorphicWritesLock)
                {
                    if (!Impl.ContainsKey(CoreConstants.DefaultSection))
                    {
                        Impl.Add(CoreConstants.DefaultSection, null);
                    }
                }
            }
        }

        public override IEnumerator<KeyValuePair<String, String>> GetEnumerator()
        {
            using (Vault.ExposeReadOnly())
            {
                EnsureDefault();
                var clone = new Dictionary<String, String>(Impl);
                return clone.GetEnumerator();
            }
        }

        public override void Add(KeyValuePair<String, String> item)
        {
            using (Vault.ExposeReadWrite())
            {
                EnsureDefault();

                var oldMetadata = Clone();
                var newImpl = new Dictionary<String, String>(Impl);
                ((ICollection<KeyValuePair<String, String>>)newImpl).Add(item);
                var newMetadata = new Metadata(_element, newImpl);
                var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                VerifyMutation();
                ((ICollection<KeyValuePair<String, String>>)Impl).Add(item);
                SaveMePlease = true;

                BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
            }
        }

        public override void Clear()
        {
            throw new NotSupportedException("Metadata doesn't allow full clear "+
                "since you could accidentally damage sections you don't even know about. " +
                "Consider the following variants: " + Environment.NewLine  +
                "1) To clear the default section use IElement.SetMetadata(null). " + Environment.NewLine +
                "2) If you still do need to erase ALL SECTIONS of the metadata, delete them one by one.");
        }

        public override bool Contains(KeyValuePair<String, String> item)
        {
            using (Vault.ExposeReadOnly())
            {
                EnsureDefault();
                return ((ICollection<KeyValuePair<String, String>>)Impl).Contains(item);
            }
        }

        public override void CopyTo(KeyValuePair<String, String>[] array, int arrayIndex)
        {
            using (Vault.ExposeReadOnly())
            {
                EnsureDefault();
                ((ICollection<KeyValuePair<String, String>>)Impl).CopyTo(array, arrayIndex);
            }
        }

        public override bool Remove(KeyValuePair<String, String> item)
        {
            var key = item.Key;
            var value = item.Value;
            using (Vault.ExposeReadOnly())
            {
                if (!Impl.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    if (Impl[key] == value)
                    {
                        return false;
                    }
                    else
                    {
                        using (Vault.ExposeReadWrite())
                        {
                            var oldMetadata = Clone();
                            var newImpl = new Dictionary<String, String>(Impl);
                            ((ICollection<KeyValuePair<String, String>>)newImpl).Remove(item);
                            var newMetadata = new Metadata(_element, newImpl);
                            var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                            VerifyMutation();
                            var result = ((ICollection<KeyValuePair<String, String>>)Impl).Remove(item);
                            EnsureDefault();
                            SaveMePlease = true;

                            BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
                            return result;
                        }
                    }
                }
            }
        }

        public override int Count
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    EnsureDefault();
                    return Impl.Count;
                }
            }
        }

        public override void Add(String key, String value)
        {
            using (Vault.ExposeReadWrite())
            {
                EnsureDefault();

                var oldMetadata = Clone();
                var newImpl = new Dictionary<String, String>(Impl);
                newImpl.Add(key, value);
                var newMetadata = new Metadata(_element, newImpl);
                var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                VerifyMutation();
                Impl.Add(key, value);
                SaveMePlease = true;

                BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
            }
        }

        public override bool ContainsKey(String key)
        {
            using (Vault.ExposeReadOnly())
            {
                EnsureDefault();
                return Impl.ContainsKey(key);
            }
        }

        public override bool Remove(String key)
        {
            using (Vault.ExposeReadOnly())
            {
                if (!Impl.ContainsKey(key))
                {
                    return false;
                }
                else
                {
                    if (Impl[key] == null)
                    {
                        return false;
                    }
                    else
                    {
                        using (Vault.ExposeReadWrite())
                        {
                            var oldMetadata = Clone();
                            var newImpl = new Dictionary<String, String>(Impl);
                            newImpl.Remove(key);
                            var newMetadata = new Metadata(_element, newImpl);
                            var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                            VerifyMutation();
                            var result = Impl.Remove(key);
                            EnsureDefault();
                            SaveMePlease = true;

                            BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
                            return result;
                        }
                    }
                }
            }
        }

        public override bool TryGetValue(String key, out String value)
        {
            using (Vault.ExposeReadOnly())
            {
                EnsureDefault();
                lock (_isomorphicWritesLock)
                {
                    if (!ContainsKey(key))
                    {
                        Impl.Add(key, null);
                    }
                }

                return Impl.TryGetValue(key, out value);
            }
        }

        public override String this[String key]
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    EnsureDefault();
                    lock (_isomorphicWritesLock)
                    {
                        if (!ContainsKey(key))
                        {
                            Impl.Add(key, null);
                        }
                    }

                    return Impl[key];
                }
            }
            set
            {
                using (Vault.ExposeReadOnly())
                {
                    EnsureDefault();

                    if (ContainsKey(key) && Impl[key] == value)
                    {
                        return;
                    }
                    else
                    {
                        using (Vault.ExposeReadWrite())
                        {
                            var oldMetadata = Clone();
                            var newImpl = new Dictionary<String, String>(Impl);
                            newImpl[key] = value;
                            var newMetadata = new Metadata(_element, newImpl);
                            var corrId = BoundVault.ReportChanging(EventReason.Metadata, Element, oldMetadata, newMetadata);

                            VerifyMutation();
                            Impl[key] = value;
                            SaveMePlease = true;

                            BoundVault.ReportChanged(corrId, EventReason.Metadata, Element, oldMetadata, newMetadata);
                        }
                    }
                }
            }
        }

        public override ICollection<String> Keys
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    var clone = Impl.Keys.ToArray();
                    return clone;
                }
            }
        }

        public override ICollection<String> Values
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    var clone = Impl.Values.ToArray();
                    return clone;
                }
            }
        }
    }
}