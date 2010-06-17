using System;
using System.Collections;
using System.Collections.Generic;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers;
using System.Linq;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl.Api;

namespace DataVault.Core.Impl.Optimize
{
    internal class IndexedNodeCollection : ICollection<Element>
    {
        public Element Host { get; private set; }
        public IndexedBranchCollection Branches { get; private set; }
        public IndexedBranchCollection InternalBranches { get; private set; }
        public IndexedValueCollection Values { get; private set; }
        public IndexedValueCollection InternalValues { get; private set; }

        public IndexedNodeCollection(Element host)
        {
            Host = host;
            Branches = new IndexedBranchCollection();
            InternalBranches = new IndexedBranchCollection();
            Values = new IndexedValueCollection();
            InternalValues = new IndexedValueCollection();
        }

        public IndexedNodeCollection(Element host, IEnumerable<Element> collection) 
            : this(host)
        {
            (collection ?? Enumerable.Empty<Element>()).ForEach(Add);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Element> GetEnumerator()
        {
            return Branches.Cast<Element>().Concat(Values.Cast<Element>()).GetEnumerator();
        }

        public void Reindex(Element item)
        {
            RemoveImpl(item);
            AddImpl(item);
        }

        public void Add(Element item)
        {
            if (item.IsInternal())
            {
                AddImpl(item);
            }
            else
            {
                var corrId1 = Host.BoundVault.ReportChanging(EventReason.ElementAdd, Host, null, item);
                var oldItemParent = item.AssertNotNull().Parent;
                var corrId2 = item.BoundVault.ReportChanging(EventReason.Add, item, oldItemParent, Host);

                // this line is necessary for the following scenario to work correctly
                // * if Added event is raised then args.Subject.Parent should point to a new parent
                item._parent = (Branch)Host;
                AddImpl(item);

                Host.BoundVault.ReportChanged(corrId1, EventReason.ElementAdd, Host, null, item);
                item.BoundVault.ReportChanged(corrId2, EventReason.Add, item, oldItemParent, Host);
            }
        }

        private void AddImpl(Element item)
        {
            if (item is Branch)
            {
                var branch = (Branch)item;
                if (!branch.IsInternal())
                {
                    Branches.Add(branch);
                }
                else
                {
                    InternalBranches.Add(branch);
                }
            }
            else if (item is Value)
            {
                var value = (Value)item;
                if (!value.IsInternal())
                {
                    Values.Add(value);
                }
                else
                {
                    InternalValues.Add(value);
                }
            }
            else
            {
                throw new NotSupportedException(String.Format("Abstract nodes of type '{0}' are not supported.",
                    item == null ? "null" : item.GetType().FullName));
            }
        }

        public void Clear()
        {
            Branches.ForEach(b => Remove(b));
            Values.ForEach(v => Remove(v));
        }

        public bool Contains(Element item)
        {
            if (item is Branch)
            {
                return Branches.Contains((Branch)item);
            }
            else if (item is Value)
            {
                return Values.Contains((Value)item);
            }
            else
            {
                throw new NotSupportedException(String.Format("Abstract nodes of type '{0}' are not supported.",
                    item == null ? "null" : item.GetType().FullName));
            }
        }

        public void CopyTo(Element[] array, int arrayIndex)
        {
            var index = arrayIndex;
            Branches.ForEach(b => { array[index++] = b; });
            Values.ForEach(b => { array[index++] = b; });
        }

        public bool Remove(Element item)
        {
            if (!Contains(item))
            {
                return false;
            }
            else
            {
                if (item.IsInternal())
                {
                    RemoveImpl(item);
                }
                else
                {
                    var corrId1 = Host.BoundVault.ReportChanging(EventReason.ElementRemove, Host, item, null);
                    var oldItemParent = item.AssertNotNull().Parent;
                    var corrId2 = item.BoundVault.ReportChanging(EventReason.Remove, item, oldItemParent, null);

                    RemoveImpl(item);

                    Host.BoundVault.ReportChanged(corrId1, EventReason.ElementRemove, Host, item, null);
                    item.BoundVault.ReportChanged(corrId2, EventReason.Remove, item, oldItemParent, null);
                }

                return true;
            }
        }

        private bool RemoveImpl(Element item)
        {
            if (item is Branch)
            {
                return Branches.Remove((Branch)item);
            }
            else if (item is Value)
            {
                return Values.Remove((Value)item);
            }
            else
            {
                throw new NotSupportedException(String.Format("Abstract nodes of type '{0}' are not supported.",
                    item == null ? "null" : item.GetType().FullName));
            }
        }

        public int Count
        {
            get { return Branches.Count + Values.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}