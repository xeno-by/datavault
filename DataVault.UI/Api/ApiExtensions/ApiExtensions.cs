using System;
using System.Drawing;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Impl.Api;
using DataVault.UI.Properties;
using DataVault.UI.Api.ContentTypez;

namespace DataVault.UI.Api.ApiExtensions
{
    public static class ApiExtensions
    {
        public static String ResolveIfSpecial(this String s)
        {
            return s == null ? Resources.Tree_Root :
               s == "default" ? Resources.Default : s;
        }

        public static TreeNode AsUIElement(this IBranch b)
        {
            var tn = new TreeNode(b.Name ?? Resources.Tree_Root);
            tn.Tag = b;
            if (tn.Text.StartsWith("$")) tn.ForeColor = Color.LightGray;
            return tn;
        }

        public static String LocalizedContentString(this IValue v)
        {
            var typeToken = v.GetTypeToken2();
            if (typeToken == "binary")
            {
                return Resources.BinaryType_ContentStringStub;
            }
            else
            {
                var locContent = ContentTypes.ApplyCType(v).AsLocalizedString;
                return locContent == null ? null : locContent.Replace(Environment.NewLine, " ");
            }
        }

        public static String LocalizedTypeToken(this IValue v)
        {
            var typeToken = v.GetTypeToken2() ?? "text";
            if (typeToken == "binary")
            {
                return Resources.ValueType_Binary;
            }
            else
            {
                return typeToken.GetCTypeFromToken().LocTypeName;
            }
        }

        public static ListViewItem AsUIElement(this IValue v)
        {
            var lvi = new ListViewItem(new String[]{
                v.Name.ResolveIfSpecial(), 
                v.LocalizedTypeToken(), 
                v.LocalizedContentString()});

            lvi.Tag = v;
            if (lvi.Text.StartsWith("$")) lvi.ForeColor = Color.LightGray;
            return lvi;
        }

        public static IValue SetTypeToken2(this IValue v, String typeToken)
        {
#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            return v.SetTypeToken(typeToken);
#else
            v.Metadata["type-token"] = typeToken;
            return v;
#endif
        }

        public static String GetTypeToken2(this IValue v)
        {
#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            return v.GetTypeToken();
#else
            // i wish i could resort to something better
            var tillerTypeToken = v.Metadata.Default;
            tillerTypeToken = tillerTypeToken == null ? null : tillerTypeToken.Trim();
            tillerTypeToken = tillerTypeToken == "binary" ? "binary" : null;

            return v.Metadata["type-token"] ?? tillerTypeToken;
#endif
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("This is an obsolete way to operate with value's typetoken. Use SetTypeToken2 instead.")]
#endif
        public static IValue SetTypeToken(this IValue v, String typeToken)
        {
            ValueMetadata.ForValue(v).TypeToken = typeToken;
            return v;
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("This is an obsolete way to operate with value's typetoken. Use GetTypeToken2 instead.")]
#endif
        public static String GetTypeToken(this IValue v)
        {
            return ValueMetadata.ForValue(v).TypeToken;
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("Since rev492 ids are inherent characteristic of every vault's element," +
            " and no longer need to be stored in metadata.")]
#endif
        public static IValue SetId(this IValue v, Guid? id)
        {
            ValueMetadata.ForValue(v).Id = id;
            return v;
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("Since rev492 ids are inherent characteristic of every vault's element," +
            " and no longer need to be stored in metadata.")]
#endif
        public static Guid? GetId(this IValue v)
        {
            return ValueMetadata.ForValue(v).Id;
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("Since rev492 ids are inherent characteristic of every vault's element," +
            " and no longer need to be stored in metadata.")]
#endif
        public static IBranch SetId(this IBranch b, Guid? id)
        {
            BranchMetadata.ForBranch(b).Id = id;
            return b;
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("Since rev492 ids are inherent characteristic of every vault's element,"+
            " and no longer need to be stored in metadata.")]
#endif
        public static Guid? GetId(this IBranch b)
        {
            return BranchMetadata.ForBranch(b).Id;
        }

        public static IBranch SetDefault2(this IBranch b)
        {
#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            return b.SetDefault();
#else
            (b.Id != Guid.Empty).AssertTrue();
            return b;
#endif
        }

        public static IValue SetDefault2(this IValue v)
        {
#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            return v.SetDefault();
#else
            (v.Id != Guid.Empty).AssertTrue();
            v.SetTypeToken2("text");
            return v;
#endif
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("This is an obsolete way to operate with branch's DataVault.UI-specific metadata. Use SetDefault2 instead.")]
#endif
        public static IBranch SetDefault(this IBranch b)
        {
            b.SetId(Guid.NewGuid());
            b.CreateValue("default").SetDefault();
            return b;
        }

#if !DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        [Obsolete("This is an obsolete way to operate with value's DataVault.UI-specific metadata. Use SetDefault2 instead.")]
#endif
        public static IValue SetDefault(this IValue v)
        {
            v.SetId(Guid.NewGuid());
            v.SetTypeToken2("text");
            return v;
        }
    }
}