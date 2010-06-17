using System.Collections.Generic;
using System.Linq;
using DataVault.Core.Helpers.Assertions;
using DataVault.UI.Api.Extensibility;
using DataVault.Core.Helpers;

namespace DataVault.UI.Api.VaultViewz
{
    // views are purposefully made to be struct-like expendable objects
    //
    // this is done to 
    // 1) let anyone (not only environment) to create and attach views (this requires views to be id'd by name, not by reference)
    // 2) provide environment/user a possibility of soft reset by disabling/enabling a view
    //    important state can be easily stored in a vault so resetting state is not a problem
    //
    // tho unlike views other extensions objects either have lifetime of an application (UI extensions)
    // or are designed to be stateless services (vault formats, content types) so they don't need lifetime policy

    public static class VaultViewFactories
    {
        private static IVaultViewFactory[] _factories;
        public static IEnumerable<IVaultViewFactory> All
        {
            get
            {
                if (_factories == null)
                {
                    var views = Codebase.Assemblies.SelectMany(a => a.GetTypes())
                        .Where(t => t.IsDefined(typeof(VaultViewAttribute), false))
                        .Select(t => new VaultViewFactory(t));

                    (views.Select(v => v.Name).Distinct().Count() == views.Count()).AssertTrue();

                    _factories = views.ToArray();
                }

                return _factories;
            }
        }
    }
}