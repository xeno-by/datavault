using System;
using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Api;
using DataVault.Core.Impl.Optimize;

namespace DataVault.Core.Impl
{
    internal abstract class Element : IVaultBoundElement, INamedObject
    {
        internal VaultBase Vault { get; private set; }
        internal VaultBase BoundVault { get; set; }
        IVault IElement.Vault { get { return Vault; } }

        internal Branch _parent;
        IBranch IElement.Parent { get { return Parent; } }
        public IEnumerable<Branch> Parents { get { return this.Parents().Cast<Branch>(); } }
        public bool IsRoot { get { return Parent == null; } }
        private IndexedNodeCollection _children;
        // VPath is declared below as a computed property (shouldn't be cached!)

        private String _name;
        // Id is declared below as a computed property

        private Metadata _metadata;
        IMetadata IElement.Metadata { get { return Metadata; } }
        public Metadata Metadata { get { using (Vault.ExposeReadOnly()) { return _metadata; } } }

        protected bool _saveMyNamePlease { get; set; }
        public bool SaveMyMetadataPlease { get { return _saveMyNamePlease || _metadata.SaveMePlease; } }

        protected Element(VaultBase vault, String name, IEnumerable<Element> children)
        {
            Vault = vault.AssertNotNull();
            Bind(Vault);

            // setting parent auto-adds the child to this collection
            // so it should be initialized as an empty one
            _children = new IndexedNodeCollection(this);
            (children ?? Enumerable.Empty<Element>()).ForEach(c => c.Parent = (Branch)this);

            Name = name;
            _metadata = new Metadata(this);
        }

        public Branch Parent
        {
            get { using (Vault.ExposeReadOnly()) { return _parent; } }
            set
            {
                value.AssertNotNull();
                using (Vault.ExposeReadWrite())
                {
                    // parent changes get reported from the IndexedNodeCollection
                    // _parent value is set from the IndexedNodeCollection

                    VerifyMutation(value.VPath + Name);
                    value.Children.Add(this);
                }
            }
        }

        public IndexedNodeCollection Children
        {
            get { using (Vault.ExposeReadOnly()) { return _children; } }
        }

        public String Name
        {
            get { using (Vault.ExposeReadOnly()) { return _name; } }
            private set
            {
                using (Vault.ExposeReadOnly())
                {
                    if (_name != value)
                    {
                        using (Vault.ExposeReadWrite())
                        {
                            VerifyMutation(VPath.Parent + value);
                            _name = value;
                        }
                    }
                }
            }
        }

        public Guid Id
        {
            get
            {
                using (Vault.InternalExpose())
                {
                    var content = Metadata[CoreConstants.IdSection];
                    if (content == null)
                    {
                        content = Guid.NewGuid().ToString();
                        Metadata[CoreConstants.IdSection] = content;
                    }

                    return new Guid(content);
                }
            }
        }

        public VPath VPath
        {
            get
            {
                using (Vault.ExposeReadOnly())
                {
                    return Parents.Reverse().Where(p => !p.Name.IsNullOrEmpty())
                        .Select(p => p.Name).Concat(Name.MkArray()).ToArray();
                }
            }
        }

        public virtual IElement CacheInMemory()
        {
            _metadata.CacheInMemory();
            return this;
        }

        IElement IElement.Clone() { return IElementClone(); }
        protected abstract IElement IElementClone();

        public virtual void Delete()
        {
            using (Vault.ExposeReadWrite())
            {
                if (IsRoot)
                {
                    Children.Clear();
                }
                else
                {
                    VerifyMutation(VPath);
                    Parent.Children.Remove(this);
                }
            }
        }

        public virtual IElement Rename(String name)
        {
            IsRoot.AssertFalse();
            (VPath + name).AssertNotNull();

            using (Vault.ExposeReadOnly())
            {
                var oldName = Name;
                var corrId = Guid.Empty;
                if (name != oldName)
                {
                    corrId = BoundVault.ReportChanging(EventReason.Rename, this, oldName, name);
                }

                if (name != oldName)
                {
                    using (Vault.ExposeReadWrite())
                    {
                        Name = name;

                        var index = Parent == null ? null : Parent.Children as IndexedNodeCollection;
                        if (index != null) index.Reindex(this);
                    }
                }

                // do not insert the check "Name == name" here
                // it will break branch name changes propagation
                // i.e. if a parent has been renamed, all his children should get 
                // the _saveMyNamePlease flag since their position @ the physical storage has changed
                _saveMyNamePlease = true;

                if (name != oldName)
                {
                    (corrId != Guid.Empty).AssertTrue();
                    BoundVault.ReportChanged(corrId, EventReason.Rename, this, oldName, name);
                }
                return this;
            }
        }

        public void Bind(VaultBase vault)
        {
            using (Vault.ExposeReadWrite())
            {
                VerifyMutation(VPath);

                if (BoundVault == vault) return;
                if (BoundVault != null) Unbind();
                BoundVault = vault;

                vault.Bind(this);
            }
        }

        public void Unbind()
        {
            using (Vault.ExposeReadWrite())
            {
                VerifyMutation(VPath);
                BoundVault = null;
            }
        }

        public virtual void AfterLoad()
        {
            _metadata.AfterLoad();
            _saveMyNamePlease = false;
        }

        public virtual void AfterSave()
        {
            _metadata.AfterSave();
            _saveMyNamePlease = false;
        }

        internal void VerifyMutation(VPath effectiveVPath)
        {
            int exposedRO, exposedRW;
            using (Vault.ExposeReadOnly(out exposedRO, out exposedRW))
            {
                int exposedInternal;
                using (Vault.InternalExpose(out exposedInternal))
                {
                    // mutation is never allowed in unexposed state
                    // exposition might be either regular (most public ops) or internal (ctor/save)
                    if (exposedRW != 0 || exposedInternal != 0)
                    {
                        // mutation is then allowed for non-internal elements
                        var mutationAprioriAllowed = !this.IsInternal(effectiveVPath);
                        if (mutationAprioriAllowed) return;

                        // for internal values mutation is only allowed when it 
                        // comes during construction and/or saving of different kinds
                        if (exposedInternal != 0) return;

                        // otherwise the mutation is illegal
                        throw new NotSupportedException(String.Format(
                            "Completing this operation would mutate a value at effective vpath '{0}'. " +
                            "This is prohibited because this vpath is reserved by the vault for internal purposes.",
                            effectiveVPath));
                    }
                    else
                    {
                        // mutation is never allowed in unexposed state
                        throw new NotSupportedException(String.Format(
                            "Completing this operation would mutate the node '{0}'. " +
                            "This is prohibited because mutations are never allowed in unexposed state.",
                            effectiveVPath));
                    }
                }
            }
        }
    }
}