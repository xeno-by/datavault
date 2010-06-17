using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Api;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl
{
    internal abstract class VaultBase : IVault, IContentAwareVault
    {
        public abstract String Uri { get; }

        public static readonly VPath IdVPath = "$id";
        public static readonly VPath RevisionVPath = "$revision";

        public Guid Id
        {
            get
            {
                using (InternalExpose())
                {
                    if (Root == null)
                    {
                        // when the root hasn't yet been created, we have no possibility to access
                        // the value used to store vault's id, so just ignore this stuff
                        throw new NotSupportedException(String.Format(
                            "Cannot acquire Id unless the vault is initialized."));
                    }
                    else
                    {
                        var host = Root.GetOrCreateValue(IdVPath, Guid.NewGuid().ToString(), ValueKind.Internal);
                        return new Guid(host.ContentString);
                    }
                }
            }

            internal set
            {
                using (InternalExpose())
                {
                    if (Root == null)
                    {
                        // when the root hasn't yet been created, we have no possibility to access
                        // the value used to store vault's id, so just ignore this stuff
                        throw new NotSupportedException(String.Format(
                            "Cannot set Id unless the vault is initialized."));
                    }
                    else
                    {
                        var host = Root.GetOrCreateValue(IdVPath, Guid.NewGuid().ToString(), ValueKind.Internal);
                        host.SetContent(value.ToString());
                    }
                }
            }
        }

        public UInt64 Revision
        {
            get
            {
                using (InternalExpose())
                {
                    if (Root == null)
                    {
                        // when the root hasn't yet been created, we have no possibility to access
                        // the value used to store vault's revision, so just ignore this stuff
                        throw new NotSupportedException(String.Format(
                            "Cannot acquire Revision unless the vault is initialized."));
                    }
                    else
                    {
                        var host = Root.GetOrCreateValue(RevisionVPath, 0.ToString(), ValueKind.Internal);
                        return UInt64.Parse(host.ContentString);
                    }
                }
            }

            internal set
            {
                int exposedRO, exposedRW;
                using (this.ExposeReadOnly(out exposedRO, out exposedRW))
                {
                    int exposedInternal;
                    using (InternalExpose(out exposedInternal))
                    {
                        if (Root == null)
                        {
                            // when the root hasn't yet been created, we have no possibility to access
                            // the value used to store vault's revision, so just ignore this stuff
                            throw new NotSupportedException(String.Format(
                                "Cannot set Revision unless the vault is initialized."));
                        }
                        else
                        {
                            // increment the revision when and only when...
                            if (exposedInternal != 0)
                            {
                                // Case A: 
                                //   the vault is exposed internally, and gets its revision reset to 0 (special case for SaveAs)
                                if (value == 0)
                                {
                                    var host = Root.GetOrCreateValue(RevisionVPath, 0.ToString(), ValueKind.Internal);
                                    host.SetContent(value.ToString());
                                }
                            }
                            else
                            {
                                // Case B: 
                                //   the vault was unexposed for r/w, i.e. the very enclosing read/write expose is about to finish
                                //   the vault was unexposed internally, i.e. no internal operation is pending

                                // todo. this is a hack, but leads only to performance degradation, but not to logic errors
                                // so for now i leave this as it is, since i'm sick and tired on synchronizing shit
                                Func<bool> causedByExpositionDispose = () =>
                                {
                                    var setterCaller = new StackTrace().GetFrame(2).GetMethod();
                                    return setterCaller == typeof(VaultExposition).GetMethod("Dispose");
                                };

                                if (exposedRW == 1 && causedByExpositionDispose())
                                {
                                    var host = Root.GetOrCreateValue(RevisionVPath, 0.ToString(), ValueKind.Internal);

                                    (value == Revision + 1).AssertTrue();
                                    host.SetContent(value.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }

        public Branch Root { get; protected set; }
        IBranch IVault.Root { get { return Root; } }

        public abstract IVault Save();
        public abstract IVault SaveAs(String uri);
        public abstract IVault Backup();

        // is used to track all values that belong or ever belonged to this vault.
        // even if the value is deleted from the vault the record stays here 
        //
        // the list is used to fixup all content/metadata streams when the vault is saved
        // and existing stream getters become invalid
        protected List<WeakReference> BoundElements = new List<WeakReference>();

        public void Bind(IBranch branch)
        {
            BoundElements.Add(new WeakReference(branch));
            ((IVaultBoundElement)branch).Bind(this);
        }

        public void Unbind(IBranch branch)
        {
            BoundElements.RemoveAll(wr => wr.Target == branch);
            ((IVaultBoundElement)branch).Unbind();
        }

        public void Bind(IValue value)
        {
            BoundElements.Add(new WeakReference(value));
            ((IVaultBoundElement)value).Bind(this);
        }

        public void Unbind(IValue value)
        {
            BoundElements.RemoveAll(wr => wr.Target == value);
            ((IVaultBoundElement)value).Unbind();
        }

        public virtual void Dispose()
        {
            using (InternalExpose())
            {
                using (ExposeReadWrite())
                {
                }
            }
        }

        public IDisposable ExposeReadOnly() { return new VaultExposition(this, true); }
        public IDisposable ExposeReadOnly(out int prevRecursiveExpositionsRO, out int prevRecursiveExpositionsRW)
        {
            var exposition = new VaultExposition(this, true);
            prevRecursiveExpositionsRO = exposition.PrevRecursiveExpositionsRO;
            prevRecursiveExpositionsRW = exposition.PrevRecursiveExpositionsRW;
            return exposition;
        }

        public IDisposable ExposeReadWrite() { return new VaultExposition(this, false); }
        public IDisposable ExposeReadWrite(out int prevRecursiveExpositionsRO, out int prevRecursiveExpositionsRW)
        {
            var exposition = new VaultExposition(this, false);
            prevRecursiveExpositionsRO = exposition.PrevRecursiveExpositionsRO;
            prevRecursiveExpositionsRW = exposition.PrevRecursiveExpositionsRW;
            return exposition;
        }

        private ReaderWriterLockSlim _expositionLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        internal class VaultExposition : IDisposable
        {
            private readonly VaultBase _vault;
            private ReaderWriterLockSlim _lock { get { return _vault._expositionLock; } }
            private readonly bool _thisExpositionIsReadOnly;
            private bool _thisExpositionIsReadWrite { get { return !_thisExpositionIsReadOnly; } }
            private int _readLocksAbandoned = 0;
            internal int PrevRecursiveExpositionsRO { get; private set; }
            internal int PrevRecursiveExpositionsRW { get; private set; }

            public VaultExposition(VaultBase vault, bool exposeReadOnly)
            {
                _vault = vault;
                _thisExpositionIsReadOnly = exposeReadOnly;
                PrevRecursiveExpositionsRO = _lock.RecursiveReadCount;
                PrevRecursiveExpositionsRW = _lock.RecursiveWriteCount;

                if (exposeReadOnly)
                {
                    if (_lock.IsWriteLockHeld)
                    {
                        // do nothing - holding a write lock allows us to do any reading
                    }
                    else
                    {
                        _lock.EnterReadLock();
                    }
                }
                else
                {
                    // letting read -> write upgrade to be an atomic operation would make us prone to deadlocks
                    // consider the following example: 
                    // thread 1: [exposeRO -> [exposeRW (will wait for t2 to release exposeRO) => locked
                    // thread 2: [exposeRO -> [exposeRW (needs to perform exposeRW in order to quit the using exposeRO block) => locked

                    // so the implementation used below is not randomly coded up, but rather after some considerations
                    _readLocksAbandoned = _lock.RecursiveReadCount;
                    while (_lock.RecursiveReadCount != 0) _lock.ExitReadLock();
                    _lock.EnterWriteLock();

                    // yeah, I'm aware of the danger of such implementation quirk
                    // for a split moment in-between read -> write transition, or, reverse during the write -> read transition
                    // the thread totally loses control of the lock, and an alient thread might hijack the lock just at the moment
                    // when read locks are abandoned but the write lock hasn't yet been acquired (or the reverse)

                    // note. the code under danger in this situation 
                    // is the one that features split r/w expositions within an operation encapsulated in a r/o lock
                    // an author could assume that when protecting the whole op with a r/o lock, he/she excludes the possibility
                    // of someone writing into the vault for the entire duration of the whole op. 
                    // however, due to implementation details this is untrue, and there's a possibility for another thread to interpose
                    // and change vault state in-between seemingly atomary operation.

                    // to fix this trouble, it'd be cool to make every holder of a readonly lock temporarily lose their privileges
                    // in favor of current thread which then freely upgrades to read/write. the following things need to be implemented then:
                    // 1) when r/w request is issued, lock the lock, stop r/o threads from accessing the vault
                    // note. ah, damn this won't work, since at that time alien threads might suddenly find out the vault changed

                    // however after some thought we can conclude that dangerous scenarios made possible by this implementation quirk
                    // are limited to the situation described above. and here's why:
                    // * if the rule described in the n0te above is fulfilled, then no unexpected stuff might happen due to hijacking
                    //   and the only threat is leaving vault in inconsistent state
                    // * the vault might be in inconsistent state only when it's r/w-exposed
                    // * thus, if a thread is in read -> write transition, or, reverse, in the write -> read transition
                    //   it can at max have the r/o-exposition active, i.e. (if the code is designed correctly) at that moment
                    //   the vault is in consistent state, so it will be after the hijacker finishes (and before that the original
                    //   thread would be unable to acquire it's lock)
                    // note. it would be nice if the stuff above worked, but it's untrue
                    // counter-example: the "this[key]" accessor of some map exposes it as r/o, and checks whether the key/value is present
                    // then when it makes sure that the r/w is necessary, the code exposes the map instance as r/w, and adds the new kvp.
                    // error scenario: during the r/o -> r/w transition, an alien thread manages to hijack the lock and write some kvp
                    // that has the same key as the one being added by an original thread. shortly after, the original crashes when attempting
                    // to add the duplicate key to the map.

                    // preventing such situations is a sole responsibility of DataVault users that should use a rule:
                    // note. if code block A depends on the results of code block B, then the most enclosing r/w exposition for both A and B must be the same
                    // examples:
                    // 1) r/o > r/w > { a(); b(); } is fine
                    // 2) r/o > { a(); r/w > b(); } fails the rule (see the map-related synchronization example above)
                    // 3) r/o > { r/w > a(); r/w > b(); } fails the rule as well

                    // some code of this codebase violates this principles in favor of robust Revision tracking
                    // (e.g. Rename first exposes the vault for R/O, then checks the necessity of a modification, and only then uses R/W)
                    // todo. fix the described issue through a careful code-review and replacing read acquisitions with upgradedable reads
                }
            }

            public void Dispose()
            {
                if (_thisExpositionIsReadWrite && _lock.RecursiveWriteCount == 1)
                {
                    int expositionsInternal;
                    using (_vault.InternalExpose(out expositionsInternal)) { }

                    if (expositionsInternal == 0)
                    {
                        try
                        {
                            _vault.Revision++;
                        }
                        catch (NotSupportedException)
                        {
                            // do nothing if revision can't be updated due to internal limitations
                            // e.g. because the Root is not yet assigned
                        }
                    }
                }

                if (_thisExpositionIsReadOnly)
                {
                    if (_lock.IsWriteLockHeld)
                    {
                        // we did nothing in .ctor, so we're doing now
                    }
                    else
                    {
                        _lock.ExitReadLock();
                    }
                }
                else
                {
                    // see comments for the .ctor
                    _lock.ExitWriteLock();
                    while (_lock.RecursiveReadCount != _readLocksAbandoned) _lock.EnterReadLock();
                }
            }
        }

        public IDisposable InternalExpose() { return new InternalVaultExposition(this); }
        public IDisposable InternalExpose(out int prevRecursiveExpositionsInternal)
        {
            var exposition = new InternalVaultExposition(this);
            prevRecursiveExpositionsInternal = exposition.PrevRecursiveExpositionsInternal;
            return exposition;
        }

        private ReaderWriterLockSlim _internalExpositionLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        internal class InternalVaultExposition : IDisposable
        {
            private readonly VaultBase _vault;
            private ReaderWriterLockSlim _lock { get { return _vault._expositionLock; } }
            private ReaderWriterLockSlim _internalLock { get { return _vault._internalExpositionLock; } }
            internal int PrevRecursiveExpositionsInternal { get; private set; }

            public InternalVaultExposition(VaultBase vault)
            {
                _vault = vault;
                PrevRecursiveExpositionsInternal = _internalLock.RecursiveReadCount;

                _lock.EnterReadLock();
                _internalLock.EnterReadLock();
            }

            public void Dispose()
            {
                _internalLock.ExitReadLock();
                _lock.ExitReadLock();
            }
        }

        private bool IsSystemEvent(ElementEventArgs args)
        {
            return IsSystemEvent(args.Subject, args.OldValue, args.NewValue);
        }

        private bool IsSystemEvent(IElement subject, Object oldValue, Object newValue)
        {
            Func<Object, bool> isSystem = o =>
            {
                var el = o as IElement;
                return el != null && el.IsInternal();
            };

            // system values never get renamed except during construction (i.e. null -> system vpath)
            // that's why we shouldn't worry that they'd pass this filter
            return isSystem(subject) || isSystem(oldValue) || isSystem(newValue);
        }

        private List<KeyValuePair<Func<ElementChangingEventArgs, bool>, Action<ElementChangingEventArgs>>> _changingListeners = 
            new List<KeyValuePair<Func<ElementChangingEventArgs, bool>, Action<ElementChangingEventArgs>>>();

        public IVault Changing(Func<ElementChangingEventArgs, bool> filter, Action<ElementChangingEventArgs> listener)
        {
            using (ExposeReadOnly())
            {
                _changingListeners.Add(new KeyValuePair<Func<ElementChangingEventArgs, bool>, Action<ElementChangingEventArgs>>(filter, listener));
                return this;
            }
        }

        internal Guid ReportChanging<T>(EventReason reason, IElement subject, Func<T> oldValue, Object newValue)
        {
            var corrId = Guid.NewGuid();
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return corrId;
                ReportChanging(new ElementChangingEventArgs(corrId, reason, subject, Revision, (Func<Object>)(() => (Object)oldValue()), newValue));
                return corrId;
            }
        }

        internal Guid ReportChanging<T>(EventReason reason, IElement subject, Object oldValue, Func<T> newValue)
        {
            var corrId = Guid.NewGuid();
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return corrId;
                ReportChanging(new ElementChangingEventArgs(corrId, reason, subject, Revision, oldValue, (Func<Object>)(() => (Object)newValue())));
                return corrId;
            }
        }

        internal Guid ReportChanging<T1, T2>(EventReason reason, IElement subject, Func<T1> oldValue, Func<T2> newValue)
        {
            var corrId = Guid.NewGuid();
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return corrId;
                ReportChanging(new ElementChangingEventArgs(corrId, reason, subject, Revision, (Func<Object>)(() => (Object)oldValue()), (Func<Object>)(() => (Object)newValue())));
                return corrId;
            }
        }

        internal Guid ReportChanging(EventReason reason, IElement subject, Object oldValue, Object newValue)
        {
            var corrId = Guid.NewGuid();
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return corrId;
                ReportChanging(new ElementChangingEventArgs(corrId, reason, subject, Revision, oldValue, newValue));
                return corrId;
            }
        }

        private void ReportChanging(ElementChangingEventArgs args)
        {
            if (IsSystemEvent(args))
            {
                return;
            }
            else
            {
                using (ExposeReadOnly())
                {
                    foreach (var kvp in _changingListeners)
                    {
                        if (kvp.Key(args))
                        {
                            kvp.Value(args);
                        }
                    }
                }
            }
        }

        private List<KeyValuePair<Func<ElementChangedEventArgs, bool>, Action<ElementChangedEventArgs>>> _changedListeners =
            new List<KeyValuePair<Func<ElementChangedEventArgs, bool>, Action<ElementChangedEventArgs>>>();

        public IVault Changed(Func<ElementChangedEventArgs, bool> filter, Action<ElementChangedEventArgs> listener)
        {
            using (ExposeReadOnly())
            {
                _changedListeners.Add(new KeyValuePair<Func<ElementChangedEventArgs, bool>, Action<ElementChangedEventArgs>>(filter, listener));
                return this;
            }
        }

        internal void ReportChanged<T>(Guid correlationId, EventReason reason, IElement subject, Func<T> oldValue, Object newValue)
        {
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return;
                ReportChanged(new ElementChangedEventArgs(correlationId, reason, subject, Revision, (Func<Object>)(() => (Object)oldValue()), newValue));
            }
        }

        internal void ReportChanged<T>(Guid correlationId, EventReason reason, IElement subject, Object oldValue, Func<T> newValue)
        {
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return;
                ReportChanged(new ElementChangedEventArgs(correlationId, reason, subject, Revision, oldValue, (Func<Object>)(() => (Object)newValue())));
            }
        }

        internal void ReportChanged<T1, T2>(Guid correlationId, EventReason reason, IElement subject, Func<T1> oldValue, Func<T2> newValue)
        {
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return;
                ReportChanged(new ElementChangedEventArgs(correlationId, reason, subject, Revision, (Func<Object>)(() => (Object)oldValue()), (Func<Object>)(() => (Object)newValue())));
            }
        }

        internal void ReportChanged(Guid correlationId, EventReason reason, IElement subject, Object oldValue, Object newValue)
        {
            int internalExpositions;
            using (InternalExpose(out internalExpositions))
            {
                if (internalExpositions != 0 || IsSystemEvent(subject, oldValue, newValue)) return;
                ReportChanged(new ElementChangedEventArgs(correlationId, reason, subject, Revision, oldValue, newValue));
            }
        }

        private void ReportChanged(ElementChangedEventArgs args)
        {
            if (IsSystemEvent(args))
            {
                return;
            }
            else
            {
                using (ExposeReadOnly())
                {
                    foreach (var kvp in _changedListeners)
                    {
                        if (kvp.Key(args))
                        {
                            kvp.Value(args);
                        }
                    }
                }
            }
        }

        #region Implementation of IElement

        public IVault Vault
        {
            get { return ((IBranch)Root).Vault; }
        }

        public IBranch Parent
        {
            get { return ((IBranch)Root).Parent; }
        }

        public VPath VPath
        {
            get { return ((IBranch)Root).VPath; }
        }

        public String Name
        {
            get { return ((IBranch)Root).Name; }
        }

        public IMetadata Metadata
        {
            get { return ((IBranch)Root).Metadata; }
        }

        public void Delete() { ((IBranch)Root).Delete(); }

        public IElement Rename(String name) { return ((IBranch)Root).Rename(name); }

        IElement IElement.CacheInMemory() { return CacheInMemory(); }

        IElement IElement.Clone() { return ((IBranch)this).Clone(); }

        #endregion

        #region Implementation of IBranch

        public IBranch CacheInMemory() { return ((IBranch)Root).CacheInMemory(); }

        public IBranch[] GetBranches() { return ((IBranch)Root).GetBranches(); }

        public IBranch[] GetBranchesRecursive() { return ((IBranch)Root).GetBranchesRecursive(); }

        public IValue[] GetValues() { return ((IBranch)Root).GetValues(); }

        public IValue[] GetValuesRecursive() { return ((IBranch)Root).GetValuesRecursive(); }

        public IBranch GetBranch(VPath vpath) { return ((IBranch)Root).GetBranch(vpath); }

        public IValue GetValue(VPath vpath) { return ((IBranch)Root).GetValue(vpath); }

        public IBranch CreateBranch(VPath vpath) { return ((IBranch)Root).CreateBranch(vpath); }

        public IValue CreateValue(VPath vpath, Func<Stream> content) { return ((IBranch)Root).CreateValue(vpath, content); }

        public IBranch GetOrCreateBranch(VPath vpath) { return ((IBranch)Root).GetOrCreateBranch(vpath); }

        public IValue GetOrCreateValue(VPath vpath, Func<Stream> content) { return ((IBranch)Root).GetOrCreateValue(vpath, content); }

        public IBranch AttachBranch(IBranch branch) { return ((IBranch)Root).AttachBranch(branch); }

        public IValue AttachValue(IValue value) { return ((IBranch)Root).AttachValue(value); }

        public IBranch ImportBranch(IBranch branch) { return ((IBranch)Root).ImportBranch(branch); }

        public IBranch ImportBranch(IBranch branch, CollisionHandling collisionHandling) { return ((IBranch)Root).ImportBranch(branch, collisionHandling); }

        public IValue ImportValue(IValue value) { return ((IBranch)Root).ImportValue(value); }

        public IValue ImportValue(IValue value, bool overwrite) { return ((IBranch)Root).ImportValue(value, overwrite); }

        IBranch IBranch.Clone() { throw new NotSupportedException(); }

        #endregion
    }
}