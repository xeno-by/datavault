using System;
using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Extensibility;
using DataVault.UI.Impl.VaultFormatz;

namespace DataVault.UI.Api.VaultFormatz
{
    public static class VaultFormats
    {
        public static IVaultFormat Default { get { return new ZipVaultFormat(); } }

        private static IVaultFormat[] _vaultFormats;
        public static IEnumerable<IVaultFormat> All
        {
            get
            {
                if (_vaultFormats == null)
                {
                    var vaultFormats = Codebase.Assemblies.SelectMany(a => a.GetTypes())
                        .Where(t => t.IsDefined(typeof(VaultFormatAttribute), false))
                        .Select(t => (IVaultFormat)Activator.CreateInstance(t));

                    (vaultFormats.Select(f => f.Name).Distinct().Count() == vaultFormats.Count()).AssertTrue();

                    _vaultFormats = vaultFormats.ToArray();
                }

                return _vaultFormats;
            }
        }

        public static IVaultFormat Infer(String url)
        {
            return All.SingleOrDefaultDontCrash(fmt => fmt.AcceptCore(url));
        }
    }
}