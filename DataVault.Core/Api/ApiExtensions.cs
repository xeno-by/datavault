using System;
using System.Collections.Generic;
using System.IO;
using DataVault.Core.Helpers;
using System.Linq;
using DataVault.Core.Impl;

namespace DataVault.Core.Api
{
    public static class ApiExtensions
    {
        public static IValue CreateValue(this IBranch branch, VPath vpath)
        {
            return branch.CreateValue(vpath, ((String)null).AsLazyStream());
        }

        public static IValue CreateValue(this IBranch branch, VPath vpath, String content)
        {
            return branch.CreateValue(vpath, content.AsLazyStream());
        }

        public static IValue CreateValue(this IBranch branch, VPath vpath, Stream content)
        {
            return branch.CreateValue(vpath, () => content);
        }

        public static IValue GetOrCreateValue(this IBranch branch, VPath vpath)
        {
            return branch.GetOrCreateValue(vpath, ((String)null).AsLazyStream());
        }

        public static IValue GetOrCreateValue(this IBranch branch, VPath vpath, String content)
        {
            return branch.GetOrCreateValue(vpath, content.AsLazyStream());
        }

        public static IValue GetOrCreateValue(this IBranch branch, VPath vpath, Stream content)
        {
            return branch.GetOrCreateValue(vpath, () => content);
        }

        public static IValue SetContent(this IValue value, String content)
        {
            return value.SetContent(content.AsLazyStream());
        }

        public static IValue SetContent(this IValue value, Stream content)
        {
            return value.SetContent(() => content);
        }

        public static IVault ImportFrom(this IVault target, IVault source)
        {
            return target.ImportFrom(source, CollisionHandling.Error);
        }

        public static IVault ImportFrom(this IVault target, IVault source, CollisionHandling collisionHandling)
        {
            var cloneOfRoot = source.Root.Clone();
            target.Root.SetEntireMetadata(cloneOfRoot.Metadata);
            cloneOfRoot.GetBranches().ForEach(b => target.ImportBranch(b, collisionHandling));
            ((Branch)cloneOfRoot).GetValues(ValueKind.Regular).ForEach(v => target.ImportValue(v, collisionHandling != CollisionHandling.Error));
            ((Branch)cloneOfRoot).GetValues(ValueKind.Internal).ForEach(v => target.ImportValue(v, true));
            return target;
        }

        public static IDisposable ExposeReadOnly(this IVault vault)
        {
            int aux1, aux2;
            return vault.ExposeReadOnly(out aux1, out aux2);
        }

        public static IDisposable ExposeReadWrite(this IVault vault)
        {
            int aux1, aux2;
            return vault.ExposeReadWrite(out aux1, out aux2);
        }

        public static IBranch[] Parents(this IElement element)
        {
            // no laziness here - it's a thread-unsafe approach
            using (element.Vault.ExposeReadOnly())
            {
                return element.ParentsImpl().ToArray();
            }
        }

        private static IEnumerable<IBranch> ParentsImpl(this IElement element)
        {
            for (var current = element.Parent; current != null; current = current.Parent)
                yield return current;
        }

        public static IElement[] ParentsAndI(this IElement element)
        {
            // no laziness here - it's a thread-unsafe approach
            return element.ParentsAndIImpl().ToArray();
        }

        private static IEnumerable<IElement> ParentsAndIImpl(this IElement element)
        {
            yield return element;

            for (var current = element.Parent; current != null; current = current.Parent)
                yield return current;
        }

        public static IBranch[] ParentsAndI(this IBranch branch)
        {
            return ((IElement)branch).ParentsAndI().Cast<IBranch>().ToArray();
        }

        public static bool IsIndirectChildOf(this IElement element, IBranch branch)
        {
            return element.Parents().Contains(branch);
        }

        public static T SetMetadata<T>(this T element, String defaultSectionContent)
            where T : IElement
        {
            element.Metadata.Default = defaultSectionContent;
            return element;
        }

        public static T SetMetadata<T>(this T element, String section, String content)
            where T : IElement
        {
            element.Metadata[section] = content;
            return element;
        }

        public static T SetEntireMetadata<T>(this T element, IMetadata metadata)
            where T : IElement
        {
            element.Metadata.InitializeFrom(metadata);
            return element;
        }

        public static IVault Virtualize(this IVault @this,
            Func<IVault, IBranch, IBranch> virtualizer, Func<IVault, IBranch, IBranch> materializer)
        {
            return @this.Virtualize(
                new VaultVisitorComposite(virtualizer),
                new VaultVisitorComposite(materializer));
        }

        public static IVault Virtualize(this IVault @this,
            Func<IVault, IValue, IValue> virtualizer, Func<IVault, IValue, IValue> materializer)
        {
            return @this.Virtualize(
                new VaultVisitorComposite(virtualizer),
                new VaultVisitorComposite(materializer));
        }

        public static IVault Virtualize(this IVault @this,
            Func<IVault, IBranch, IBranch> branchVirtualizer, Func<IVault, IValue, IValue> valueVirtualizer,
            Func<IVault, IBranch, IBranch> branchMaterializer, Func<IVault, IValue, IValue> valueMaterializer)
        {
            return @this.Virtualize(
                new VaultVisitorComposite(branchVirtualizer, valueVirtualizer),
                new VaultVisitorComposite(branchMaterializer, valueMaterializer));
        }
    }
}