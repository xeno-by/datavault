using System;
using System.Collections.Generic;
using System.Linq;
using DataVault.UI.Api.Extensibility;

namespace DataVault.UI.Api.UIExtensionz
{
    public static class DataVaultUIExtensions
    {
        private static Func<IDataVaultUIExtension>[] _extensions;
        public static IEnumerable<IDataVaultUIExtension> All
        {
            get
            {
                if (_extensions == null)
                {
                    var extensions = Codebase.Assemblies.SelectMany(a => a.GetTypes())
                        .Where(t => t.IsDefined(typeof(DataVaultUIExtensionAttribute), false))
                        .Select(t => (Func<IDataVaultUIExtension>)(() => (IDataVaultUIExtension)Activator.CreateInstance(t)));
                    _extensions = extensions.ToArray();
                }

                return _extensions.Select(e => e()).ToArray();
            }
        }
    }
}
