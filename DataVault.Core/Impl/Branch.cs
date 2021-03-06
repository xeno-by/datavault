using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using DataVault.Core.Api;
using System.Linq;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;

namespace DataVault.Core.Impl
{
    internal class Branch : Element, IBranch
    {
        public Branch(VaultBase vault, String name, IEnumerable<Element> children)
            : base(vault, name, children)
        {
        }

        public override String ToString()
        {
            return String.Format("{0} (B: {1}+{3}/{2}+{4}V, {5}/{6}B)", VPath,
                GetValues(ValueKind.Regular).Count(), GetValuesRecursive(ValueKind.Regular).Count(),
                GetValues(ValueKind.Internal).Count(), GetValuesRecursive(ValueKind.Internal).Count(),
                GetBranches().Count(), GetBranchesRecursive().Count());
        }

        IBranch IBranch.CacheInMemory()
        {
            return (IBranch)CacheInMemory();
        }

        public override IElement CacheInMemory()
        {
            base.CacheInMemory();

            GetValues(ValueKind.RegularAndInternal).ForEach(v => v.CacheInMemory());
            GetBranches().ForEach(b1 => b1.CacheInMemory());

            return this;
        }

        protected override IElement IElementClone()
        {
            return Clone();
        }

        public IBranch Clone()
        {
            using (Vault.ExposeReadWrite())
            {
                var clone = new Branch(Vault, Name, null);
                Children.Select(c => (Element)((IElement)c).Clone()).ForEach(c => c.Parent = clone);
                return (IBranch)clone.SetMetadata(Metadata);
            }
        }

        public override IElement Rename(String name)
        {
            base.Rename(name);

            // propagate changes to children
            GetBranchesRecursive().ForEach(b => b.Rename(b.Name));
            GetValuesRecursive(ValueKind.Regular).ForEach(v => v.Rename(v.Name));

            return this;
        }

        public IBranch GetBranch(VPath vpath)
        {
            using (Vault.ExposeReadOnly())
            {
                return vpath.Steps.Aggregate(this, (curr, step) => curr == null ? null : curr.Children.Branches[step]);
            }
        }

        public IValue GetValue(VPath vpath)
        {
            using (Vault.ExposeReadOnly())
            {
                var steps = vpath.Steps.ToArray();
                var parent = (Branch)GetBranch(steps.Take(steps.Length - 1).ToArray());
                return parent == null ? null : parent.Children.Values[steps.Last()];
            }
        }

        public IBranch[] GetBranches()
        {
            using (Vault.ExposeReadOnly())
            {
                // no laziness here since it's a thread-unsafe approach
                return Children.Branches.Cast<IBranch>().ToArray();
            }
        }

        public IBranch[] GetBranchesRecursive()
        {
            using (Vault.ExposeReadOnly())
            {
                // no laziness here since it's a thread-unsafe approach
                return this.Flatten(b => b.Children.Branches).Except(this.MkArray()).Cast<IBranch>().ToArray();
            }
        }

        IValue[] IBranch.GetValues()
        {
            return GetValues(ValueKind.Regular);
        }

        public IValue[] GetValues(ValueKind valueKind)
        {
            using (Vault.ExposeReadOnly())
            {
                var regularValues = (valueKind & ValueKind.Regular) != 0 ?
                    Children.Values.Cast<IValue>() :
                    Enumerable.Empty<IValue>();

                var internalValues = (valueKind & ValueKind.Internal) != 0 ?
                    Children.InternalValues.Cast<IValue>() :
                    Enumerable.Empty<IValue>();

                // no laziness here since it's a thread-unsafe approach
                return regularValues.Concat(internalValues).ToArray();
            }
        }

        IValue[] IBranch.GetValuesRecursive()
        {
            return GetValuesRecursive(ValueKind.Regular);
        }

        public IValue[] GetValuesRecursive(ValueKind valueKind)
        {
            using (Vault.ExposeReadOnly())
            {
                var regularValues = (valueKind & ValueKind.Regular) != 0 ?
                    this.Flatten(b => b.Children.Branches).SelectMany(b => b.Children.Values).Cast<IValue>() :
                    Enumerable.Empty<IValue>();

                var internalValues = (valueKind & ValueKind.Internal) != 0 ?
                    this.Flatten(b => b.Children.Branches).SelectMany(b => b.Children.InternalValues).Cast<IValue>() :
                    Enumerable.Empty<IValue>();

                // no laziness here since it's a thread-unsafe approach
                return regularValues.Concat(internalValues).ToArray();
            }
        }

        public IBranch GetOrCreateBranch(VPath vpath)
        {
            using (Vault.ExposeReadOnly())
            {
                return vpath.Steps.Aggregate(this, (curr, step) => {
                    var next = curr.Children.Branches[step];
                    if (next != null)
                    {
                        return next;
                    }
                    else
                    {
                        using (Vault.ExposeReadWrite())
                        {
                            return new Branch(Vault, step, null){Parent = curr};
                        }
                    }
                });
            }
        }

        IValue IBranch.GetOrCreateValue(VPath vpath, Func<Stream> content)
        {
            return GetOrCreateValue(vpath, content, ValueKind.Regular);
        }

        public IValue GetOrCreateValue(VPath vpath, Func<Stream> content, ValueKind valueKind)
        {
            using (Vault.ExposeReadOnly())
            {
                var steps = vpath.Steps.ToArray();
                var parent = (Branch)GetOrCreateBranch(steps.Take(steps.Length - 1).ToArray());

                var regularValue = (valueKind & ValueKind.Regular) != 0 ?
                    parent.Children.Values[steps.Last()] : null;
                var internalValue = (valueKind & ValueKind.Internal) != 0 ?
                    parent.Children.InternalValues[steps.Last()] : null;
                var value = regularValue ?? internalValue;

                if (value != null)
                {
                    return value;
                }
                else
                {
                    using (Vault.ExposeReadWrite())
                    {
                        var newValue = new Value(Vault, steps.Last(), null);
                        newValue.Parent = parent;
                        newValue.SetContent(content);
                        return newValue;
                    }
                }
            }
        }

        public IBranch CreateBranch(VPath vpath)
        {
            using (Vault.ExposeReadWrite())
            {
                GetBranch(vpath).AssertNull();
                return GetOrCreateBranch(vpath);
            }
        }

        IValue IBranch.CreateValue(VPath vpath, Func<Stream> content)
        {
            return CreateValue(vpath, content, ValueKind.Regular);
        }

        public IValue CreateValue(VPath vpath, Func<Stream> content, ValueKind valueKind)
        {
            using (Vault.ExposeReadWrite())
            {
                GetValue(vpath).AssertNull();
                return GetOrCreateValue(vpath, content, valueKind);
            }
        }

        public IBranch AttachBranch(IBranch branch)
        {
            using (Vault.ExposeReadWrite())
            {
                using (branch.Vault.ExposeReadWrite())
                {
                    GetBranch(branch.Name).AssertNull();

                    ((Branch)branch).Parent = this;
                    EnsureSameVault(branch);

                    return branch;
                }
            }
        }

        public IValue AttachValue(IValue value)
        {
            using (Vault.ExposeReadWrite())
            {
                using (value.Vault.ExposeReadWrite())
                {
                    GetValue(value.Name).AssertNull();

                    ((Value)value).Parent = this;
                    EnsureSameVault(value);

                    return value;
                }
            }
        }

        private void EnsureSameVault(IElement element)
        {
            if (element.Vault != Vault)
            {
                var hack = typeof(Element).GetProperty("Vault",
                    BindingFlags.NonPublic | BindingFlags.Instance).GetSetMethod(true);
                hack.Invoke(element, new Object[] { Vault });
            }
        }

        public IBranch ImportBranch(IBranch branch)
        {
            return ImportBranch(branch, CollisionHandling.Error);
        }

        public IBranch ImportBranch(IBranch branch, CollisionHandling collisionHandling)
        {
            using (Vault.ExposeReadWrite())
            {
                using (branch.Vault.ExposeReadWrite())
                {
                    var existing = GetBranch(branch.Name);
                    if (existing == null)
                    {
                        return AttachBranch(branch.Clone());
                    }
                    else
                    {
                        (collisionHandling != CollisionHandling.Error).AssertTrue();

                        if (collisionHandling == CollisionHandling.Overwrite)
                        {
                            existing.Delete();
                            return AttachBranch(branch.Clone());
                        }
                        else if (collisionHandling == CollisionHandling.Merge)
                        {
                            // atomicity is not guaranteed. sorry
                            branch.GetBranches().ForEach(b => existing.ImportBranch(b, CollisionHandling.Merge));
                            branch.GetValues().ForEach(v => existing.ImportValue(v, true));
                            return this;
                        }
                        else
                        {
                            throw new NotSupportedException(String.Format(
                                "Collision handling strategy '{0}' is not supported.", collisionHandling));
                        }
                    }
                }
            }
        }

        public IValue ImportValue(IValue value)
        {
            return ImportValue(value, false);
        }

        public IValue ImportValue(IValue value, bool overwrite)
        {
            using (Vault.ExposeReadWrite())
            {
                using (value.Vault.ExposeReadWrite())
                {
                    var existing = GetValue(value.Name);
                    if (existing != null)
                    {
                        overwrite.AssertTrue();
                        existing.Delete();
                    }

                    return AttachValue(value.Clone());
                }
            }
        }
    }
}