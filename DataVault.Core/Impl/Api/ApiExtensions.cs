using System;
using System.IO;
using DataVault.Core.Api;
using DataVault.Core.Helpers;

namespace DataVault.Core.Impl.Api
{
    internal static class ApiExtensions
    {
        public static void Bind(this IContentAwareVault cav, IElement element)
        {
            if (element is IBranch)
            {
                cav.Bind((IBranch)element);
            }
            else if (element is IValue)
            {
                cav.Bind((IValue)element);
            }
            else
            {
                throw new NotSupportedException(element == null ? "null" : element.GetType().ToString());
            }
        }

        public static void Unbind(this IContentAwareVault cav, IElement element)
        {
            if (element is IBranch)
            {
                cav.Unbind((IBranch)element);
            }
            else if (element is IValue)
            {
                cav.Unbind((IValue)element);
            }
            else
            {
                throw new NotSupportedException(element == null ? "null" : element.GetType().ToString());
            }
        }

        public static Value CreateValue(this Branch branch, VPath vpath, ValueKind valueKind)
        {
            return (Value)branch.CreateValue(vpath, ((String)null).AsLazyStream(), valueKind);
        }

        public static Value CreateValue(this Branch branch, VPath vpath, String content, ValueKind valueKind)
        {
            return (Value)branch.CreateValue(vpath, content.AsLazyStream(), valueKind);
        }

        public static Value CreateValue(this Branch branch, VPath vpath, Stream content, ValueKind valueKind)
        {
            return (Value)branch.CreateValue(vpath, () => content, valueKind);
        }

        public static Value GetOrCreateValue(this Branch branch, VPath vpath, ValueKind valueKind)
        {
            return (Value)branch.GetOrCreateValue(vpath, ((String)null).AsLazyStream(), valueKind);
        }

        public static Value GetOrCreateValue(this Branch branch, VPath vpath, String content, ValueKind valueKind)
        {
            return (Value)branch.GetOrCreateValue(vpath, content.AsLazyStream(), valueKind);
        }

        public static Value GetOrCreateValue(this Branch branch, VPath vpath, Stream content, ValueKind valueKind)
        {
            return (Value)branch.GetOrCreateValue(vpath, () => content, valueKind);
        }

        public static bool IsInternal(this IElement element)
        {
            using (element.Vault.ExposeReadOnly())
            {
                return element.IsInternal(element.VPath);
            }
        }

        public static bool IsInternal(this IElement element, VPath effectiveVPath)
        {
            using (element.Vault.ExposeReadOnly())
            {
                return element is IValue && (effectiveVPath == VaultBase.IdVPath || effectiveVPath == VaultBase.RevisionVPath);
            }
        }

        public static Metadata InitializeFrom(this Metadata @this, String content)
        {
            return @this.InitializeFrom(content.AsLazyStream());
        }

        public static Metadata InitializeFrom(this Metadata @this, Stream content)
        {
            return @this.InitializeFrom(() => content);
        }

        public static T RawSetMetadata<T>(this T element, String content)
            where T : Element
        {
            element.Metadata.InitializeFrom(content);
            return element;
        }

        public static T RawSetMetadata<T>(this T element, Stream content)
            where T : Element
        {
            element.Metadata.InitializeFrom(content);
            return element;
        }

        public static T RawSetMetadata<T>(this T element, Func<Stream> content)
            where T : Element
        {
            element.Metadata.InitializeFrom(content);
            return element;
        }

        public static Stream FixupForBeingSaved(this Stream s)
        {
            return s ?? new MemoryStream();
        }
    }
}