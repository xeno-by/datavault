using System.Linq;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using NUnit.Framework;

namespace DataVault.Playground
{
    [TestFixture]
    public class InMemoryVaultTests
    {
        [Test]
        public virtual void TestImportDifferentModes()
        {
            using (var write = VaultApi.OpenInMemory())
            {
                var a = write.CreateBranch("a");
                a.CreateBranch("foo").CreateValue("1", "a.foo.1");
                a.CreateBranch("bar").CreateValue("1", "a.bar.1");

                var b = write.CreateBranch("b");
                var ax = b.ImportBranch(a);
                ax.GetBranch("foo").GetValue("1").SetContent("ax.foo.1");
                ax.GetBranch("foo").CreateValue("2", "ax.foo.2");
                ax.GetBranch("bar").GetValue("1").Delete();

                try
                {
                    var rootError = write.Root.Clone();
                    rootError.GetBranch("b/a").Rename("ax");
                    rootError.ImportBranch(ax);
                }
                catch (AssertionFailedException)
                {
                }

                var rootOverwrite = write.Root.Clone();
                rootOverwrite.ImportBranch(ax, CollisionHandling.Overwrite);
                Assert.AreEqual(2, rootOverwrite.GetBranch("a/foo").GetValues().Count());
                Assert.AreEqual("ax.foo.1", rootOverwrite.GetValue("a/foo/1").ContentString);
                Assert.AreEqual("ax.foo.2", rootOverwrite.GetValue("a/foo/2").ContentString);
                Assert.AreEqual(0, rootOverwrite.GetBranch("a/bar").GetValues().Count());

                var rootMerge = write.Root.Clone();
                rootMerge.ImportBranch(ax, CollisionHandling.Merge);
                Assert.AreEqual(2, rootMerge.GetBranch("a/foo").GetValues().Count());
                Assert.AreEqual("ax.foo.1", rootMerge.GetValue("a/foo/1").ContentString);
                Assert.AreEqual("ax.foo.2", rootMerge.GetValue("a/foo/2").ContentString);
                Assert.AreEqual(1, rootMerge.GetBranch("a/bar").GetValues().Count());
                Assert.AreEqual("a.bar.1", rootMerge.GetValue("a/bar/1").ContentString);
            }
        }
    }
}