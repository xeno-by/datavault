using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.ApiExtensions;
using DataVault.UI.Properties;

namespace DataVault.UI.Api.Versioning
{
    public static class StructureVersion
    {
#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
        public static readonly String Current = "v1.rev299";
#else
        public static readonly String Current = "v1.rev492";
#endif

        private static IValue GetStructureVersionHolder(this IVault vault)
        {
            // Load all possible version holders
            var versionHolder1 = vault.GetValue("$version");
            var versionHolder2 = vault.GetValue("$format");
            var versionHolder3 = vault.GetValue("$structureversion");

            // Get version token from the latest holder
            var genuineHolder = versionHolder3 ?? versionHolder2 ?? versionHolder1;
            genuineHolder = genuineHolder ?? vault.CreateValue("$structureversion");
            var genuineVersion = genuineHolder.ContentString;

            // Ensure that all kinds of version holders are present in the vault (backward compat!)
            versionHolder1 = versionHolder1 ?? vault.GetOrCreateValue("$version");
            versionHolder2 = versionHolder2 ?? vault.GetOrCreateValue("$format");
            versionHolder3 = versionHolder3 ?? vault.GetOrCreateValue("$structureversion");
            var allHolders = new []{versionHolder1, versionHolder2, versionHolder3};

            // Now synchronize all version signatures across all holders
            foreach (var anyVersionHolder in allHolders)
            {
                if (anyVersionHolder != null)
                    anyVersionHolder.SetContent(genuineVersion);
            }

            // Return only the latest holder
            return genuineHolder;
        }

        private static String GetStructureVersionToken(this IVault vault)
        {
            var token = vault.GetStructureVersionHolder().ContentString;
            return token.IsNullOrEmpty() ? null : token;
        }

        private static void UpdateStructureVersionToken(this IVault vault, String token)
        {
            vault.GetStructureVersionHolder().SetContent(token);
        }

        private static bool IsOfLatestStructureVersion(this IVault vault)
        {
            return vault.GetStructureVersionToken() == Current;
        }

        private static void UpgradeToLatestStructureVersion(this IVault vault)
        {
            var pos = History.FindIndex(p => p.First == vault.GetStructureVersionToken());
            if (pos == -1) throw new ArgumentException(String.Format("Version '{0}' is not supported", vault.GetStructureVersionToken()));
            if (pos == History.Count - 1) return;

            var origFilename = vault.Uri;
            Func<int, String> namegen = i => origFilename + "." + vault.GetStructureVersionToken() + ".bak" + (i == 1 ? null : i.ToString());
            var lastUsedIndex = 1.Seq(i => i + 1, i => File.Exists(namegen(i))).LastOrDefault();
            var bakFilename = namegen(lastUsedIndex + 1);

            vault.SaveAs(bakFilename);
            History.Skip(pos + 1).ForEach(p => { p.Second(vault); vault.UpdateStructureVersionToken(p.First); });
            (History.Last().First == Current).AssertTrue();
            vault.UpdateStructureVersionToken(Current);
            vault.SaveAs(origFilename);
        }

        public static bool EnsureIsOfLatestStructureVersion(this IVault vault)
        {
            if (vault == null)
            {
                return false;
            }

            if (vault.GetBranchesRecursive().Count() == 0 &&
                vault.GetValuesRecursive().Count() == 0 &&
                vault.GetStructureVersionToken().IsNullOrEmpty())
            {
                vault.UpdateStructureVersionToken(StructureVersion.Current);
                vault.SetDefault2();
                vault.Save();
            }

            if (!vault.IsOfLatestStructureVersion())
            {
                var oldVersion = vault.GetStructureVersionToken();
                if (History.Any(kvp => kvp.First == oldVersion))
                {
                    Func<String, String> fmt = s => String.Format(s, oldVersion ?? Resources.Version_Empty, StructureVersion.Current);
                    if (MessageBox.Show(fmt(Resources.Version_UpgradeVaultToLatestVersion), Resources.Confirmation_Title,
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                    {
                        try
                        {
                            vault.UpgradeToLatestStructureVersion();

                            MessageBox.Show(fmt(Resources.Version_UpgradeSucceeded), Resources.Version_UpgradeSucceeded_Title,
                                MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                            return true;
                        }
                        catch (Exception)
                        {
                            MessageBox.Show(fmt(Resources.Version_UpgradeFailed), Resources.Version_UpgradeFailed_Title,
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                            vault.Dispose();
                            return false;
                        }
                    }
                    else
                    {
                        vault.Dispose();
                        return false;
                    }
                }
                else
                {
                    Func<String, String> fmt = s => String.Format(s, oldVersion ?? Resources.Version_Empty, StructureVersion.Current);
                    MessageBox.Show(fmt(Resources.Version_UnknownVersion), Resources.Version_UnknownVersion_Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static IVault OpenAsLatestStructureVersion(Func<IVault> opener)
        {
            var vault = opener();
            return vault.EnsureIsOfLatestStructureVersion() ? vault : null;
        }

        private class Tuple<T1, T2>
        {
            public T1 First { get; private set; }
            public T2 Second { get; private set; }

            public Tuple(T1 first, T2 second)
            {
                First = first;
                Second = second;
            }
        }

        private static readonly List<Tuple<String, Action<IVault>>> History =
            new List<Tuple<String, Action<IVault>>>();

        static StructureVersion()
        {
#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            History.Add(new Tuple<String, Action<IVault>>(null, null));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev122", UpgradeFromNullToRev122));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev142", UpgradeFromRev122ToRev142));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev299", UpgradeFromRev142ToRev299));
#else 
            History.Add(new Tuple<String, Action<IVault>>(null, null));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev122", UpgradeFromNullToRev122));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev142", UpgradeFromRev122ToRev142));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev299", UpgradeFromRev142ToRev299));
            History.Add(new Tuple<String, Action<IVault>>("v1.rev492", UpgradeFromRev299ToRev492));
#endif
        }

        // so that neither Resharper no compiler whine about using legacy routines in code that supports legacy storage formats
#pragma warning disable 618

        private static void UpgradeFromNullToRev122(IVault vault)
        {
            (vault.GetStructureVersionToken().IsNullOrEmpty()).AssertTrue();

            foreach (var branch in vault.GetBranchesRecursive().Concat(vault.Root.MkArray()))
            {
                // all branches get a GUID generated and associated if they don't have one yet
                if (branch.GetId() == null) branch.SetId(Guid.NewGuid());

                // all branches get a default value created if they don't have one yet
                var @default = branch.GetValue("default");
                if (@default == null) branch.CreateValue("default").SetDefault();
            }
        }

        private static void UpgradeFromRev122ToRev142(IVault vault)
        {
            (vault.GetStructureVersionToken() == "v1.rev122").AssertTrue();

            foreach (var value in vault.GetValuesRecursive())
            {
                // all values get a GUID generated and associated if they don't have one yet
                if (value.GetId() == null) value.SetId(Guid.NewGuid());
            }
        }

        private static void UpgradeFromRev142ToRev299(IVault vault)
        {
            (vault.GetStructureVersionToken() == "v1.rev142").AssertTrue();

            (vault.Id != Guid.Empty).AssertTrue();
            (vault.Revision is UInt64).AssertTrue();
        }

        private static void UpgradeFromRev299ToRev492(IVault vault)
        {
            (vault.GetStructureVersionToken() == "v1.rev299").AssertTrue();

#if DOWNGRADE_STRUCTURE_VERSION_TO_REV299
            AssertionHelper.Fail();
#else
            // branches: remove all empty default values from branches
            foreach (var branch in vault.GetBranchesRecursive().Concat(vault.Root.MkArray()))
            {
                var @default = branch.GetValue("default");
                if (@default != null && @default.ContentString.IsNullOrEmpty())
                {
                    @default.Delete();
                }
            }

            // branches: upgrade metadata -> id is no more stored in general-purpose section
            foreach (var branch in vault.GetBranchesRecursive().Concat(vault.Root.MkArray()))
            {
                try
                {
                    var oldSchoolId = branch.GetId() ?? Guid.NewGuid();

                    branch.SetMetadata(null);
                    branch.Metadata["$id"] = oldSchoolId.ToString();

                    (branch.Id == oldSchoolId).AssertTrue();
                }
                catch (Exception)
                {
                    // do nothing in case of invalid format
                }
            }

            // values: 
            // 1) upgrade metadata -> id is no more stored in general-purpose section
            // 2) upgrade metadata -> type token is no more stored in general-purpose section
            foreach (var value in vault.GetValuesRecursive())
            {
                try
                {
                    var oldSchoolId = value.GetId() ?? Guid.NewGuid();
                    var oldSchoolTypeToken = value.GetTypeToken();

                    value.SetMetadata(null);
                    value.Metadata["$id"] = oldSchoolId.ToString();
                    value.Metadata["type-token"] = oldSchoolTypeToken;

                    (value.Id == oldSchoolId).AssertTrue();
                    (value.GetTypeToken2() == oldSchoolTypeToken).AssertTrue();
                }
                catch (Exception)
                {
                    // do nothing in case of invalid format
                }
            }
#endif
        }

#pragma warning restore 618

    }
}