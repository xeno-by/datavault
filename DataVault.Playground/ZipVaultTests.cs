using System;
using DataVault.Core.Api;
using NUnit.Framework;

namespace DataVault.Playground
{
    [TestFixture]
    public class ZipVaultTests : VaultTestsDual
    {
        protected override IVault OpenVaultImpl(String uri)
        {
            return VaultApi.OpenZip(uri);
        }

        protected override IVault LoadVaultBlueprintImpl(String uri)
        {
            return VaultApi.OpenZip(uri);
        }

        [Test]
        public override void Simple()
        {
            base.Simple();
        }

        [Test]
        public override void Simple2()
        {
            base.Simple2();
        }

        [Test]
        public override void RecursiveDeleteBranchAndItsContents()
        {
            base.RecursiveDeleteBranchAndItsContents();
        }

        [Test]
        public override void SaveBranchesWithoutValues()
        {
            base.SaveBranchesWithoutValues();
        }

        [Test]
        public override void SaveBranchAndValueWithTheSameName()
        {
            base.SaveBranchAndValueWithTheSameName();
        }

        [Test]
        public override void TestDotNamesThatAreSpecialForFs()
        {
            base.TestDotNamesThatAreSpecialForFs();
        }

        [Test]
        public override void ReadExternallyComposedVault()
        {
            base.ReadExternallyComposedVault();
        }

        [Test]
        public override void ConsistencyTest()
        {
            base.ConsistencyTest();
        }

        [Test]
        public override void ConsistencyAfterDeleteTest()
        {
            base.ConsistencyAfterDeleteTest();
        }

        [Test]
        public override void TestAttachFromAlienVault()
        {
            base.TestAttachFromAlienVault();
        }

        [Test]
        public override void MetadataStuff()
        {
            base.MetadataStuff();
        }

        [Test]
        public override void InternalPropertiesTest()
        {
            base.InternalPropertiesTest();
        }

        [Test]
        public override void SmartSaveTest()
        {
            base.SmartSaveTest();
        }

        [Test]
        public override void TestThreadSafety()
        {
            base.TestThreadSafety();
        }

        [Test]
        public override void SomeEventAndRevisionOptimizations()
        {
            base.SomeEventAndRevisionOptimizations();
        }

        [Test]
        public override void NewMetadataTest()
        {
            base.NewMetadataTest();
        }

        [Test]
        public override void NewMetadataTest2()
        {
            base.NewMetadataTest2();
        }

        [Test]
        public override void TestEmptyAndNullBeingDifferentiated()
        {
            base.TestEmptyAndNullBeingDifferentiated();
        }
    }
}