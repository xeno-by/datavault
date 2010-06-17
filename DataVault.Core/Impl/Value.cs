using System;
using System.IO;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl
{
    internal class Value : Element, IValue
    {
        public Value(VaultBase vault, String name, Func<Stream> content)
            : base(vault, name, null)
        {
            _content = content ?? (() => null);
        }

        public override String ToString()
        {
            return String.Format("{0} (V)", VPath);
        }

        IValue IValue.CacheInMemory()
        {
            return (IValue)CacheInMemory();
        }

        public override IElement CacheInMemory()
        {
            base.CacheInMemory();

            var cachedContent = _content().CacheInMemory();
            _content = () => cachedContent;

            return this;
        }

        protected override IElement IElementClone()
        {
            return Clone();
        }

        public IValue Clone()
        {
            using (Vault.ExposeReadWrite())
            {
                return (IValue)new Value(Vault, Name, () => ContentStream).SetMetadata(Metadata);
            }
        }

        public override void AfterLoad()
        {
            base.AfterLoad();
            _saveMyContentPlease = false;
        }

        public override void AfterSave()
        {
            base.AfterSave();
            _saveMyContentPlease = false;
        }

        private Func<Stream> _content;

        private bool _saveMyContentPlease { get; set; }
        public bool SaveMyContentPlease { get { return _saveMyContentPlease || _saveMyNamePlease; } }

        private bool _isContentCached;
        private Stream _contentCached;
        private bool _isContentStringCached;
        private String _contentStringCached;

        public Stream ContentStream
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    if (!_isContentCached)
                    {
                        var content = _content();
                        if (Metadata[CoreConstants.ContentIsNullSection].IsNeitherNullNorEmpty()) 
                            content = null;

                        _contentCached = content;
                        _isContentCached = true;

                        using (Vault.InternalExpose())
                        {
                            Metadata[CoreConstants.ContentIsNullSection] = 
                                _contentCached != null ? null : "I GUARANTEE IT";
                        }
                    }

                    return _contentCached;
                }
            }
        }

        public String ContentString
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    if (!_isContentStringCached)
                    {
                        _contentStringCached = ContentStream.AsString();
                        _isContentStringCached = true;
                    }

                    return _contentStringCached;
                }
            }
        }

        public IValue SetContent(Func<Stream> content)
        {
            content = content ?? (() => null);
            using (Vault.ExposeReadWrite())
            {
                var oldContent = _content;
                if (Metadata[CoreConstants.ContentIsNullSection].IsNeitherNullNorEmpty())
                    oldContent = () => null;

                var corrId = BoundVault.ReportChanging(EventReason.Content, this, oldContent, content);

                VerifyMutation(VPath);
                _content = content;
                _saveMyContentPlease = true;

                using (Vault.InternalExpose())
                {
                    _isContentCached = false;
                    _isContentStringCached = false;
                    Metadata[CoreConstants.ContentIsNullSection] = null;
                }

                BoundVault.ReportChanged(corrId, EventReason.Content, this, oldContent, content);
                return this;
            }
        }
    }
}