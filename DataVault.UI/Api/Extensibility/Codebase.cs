using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using DataVault.Core.Helpers.Reflection;
using DataVault.UI.Api.UIExtensionz;
using DataVault.Core.Helpers;

namespace DataVault.UI.Api.Extensibility
{
    public static class Codebase
    {
        private static Assembly[] _assemblies;
        public static IEnumerable<Assembly> Assemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    var dlls = new DirectoryInfo(Application.StartupPath).GetFiles("*.dll");
                    var exes = new DirectoryInfo(Application.StartupPath).GetFiles("*.exe");
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (o, e) => Assembly.ReflectionOnlyLoad(e.Name);
                    foreach (var wannabePluginFile in dlls.Concat(exes))
                    {
                        try
                        {
                            // trim the extension from assembly name
                            var wannabePlugin = Assembly.ReflectionOnlyLoad(wannabePluginFile.Name.Slice(0, -4));

                            // one cannot use HasAttr<> for reflection-only loaded assemblies
                            var attrs = CustomAttributeData.GetCustomAttributes(wannabePlugin);

                            // one cannot use direct type comparison
                            var extAttr = typeof(DataVaultUIExtensionAttribute);
                            if (attrs.Select(cad => cad.Constructor.DeclaringType).Any(t => 
                                (t.MetadataToken == extAttr.MetadataToken) &&
                                (t.Module.MetadataToken == extAttr.Module.MetadataToken) &&
                                (t.Assembly.FullName == extAttr.Assembly.FullName)))
                            {
                                AppDomain.CurrentDomain.Load(wannabePluginFile.Name.Slice(0, -4));
                            }
                        }
                        catch (Exception)
                        {
                            // ignore this: maybe its a native image or an assembly is already loaded
                        }
                    }

                    var @this = Assembly.GetExecutingAssembly().MkArray();
                    var plugins = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.HasAttr<DataVaultUIExtensionAttribute>());
                    _assemblies = @this.Concat(plugins).ToArray();
                }

                return _assemblies;
            }
        }
    }
}
