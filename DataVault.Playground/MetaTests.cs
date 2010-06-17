using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DataVault.Core.Api;
using DataVault.Core.Api.Events;
using DataVault.Core.Helpers;
using DataVault.Core.Impl.Xml;
using DataVault.Core.Impl.Zip.ZipLib.Exceptions;
using NUnit.Framework;

namespace DataVault.Playground
{
    [TestFixture]
    public class MetaTests
    {
        [Test]
        public void RestrictMostClassesToInternal()
        {
            var coreTypes = Assembly.LoadFrom("DataVault.Core.dll").GetTypes();
            Assert.IsNull(coreTypes.Where(t => 
                t.IsPublic &&
                !t.Namespace.StartsWith(typeof(EnumerableExtensions).Namespace) &&
                t.Namespace != typeof(EventReason).Namespace &&
                t.Namespace != typeof(VaultApi).Namespace &&
                t.Namespace != typeof(XmlValueDto).Namespace &&
                t.Name != typeof(ZipException).Name &&
                !t.IsDefined(typeof(CompilerGeneratedAttribute), false)).FirstOrDefault());
        }
    }
}