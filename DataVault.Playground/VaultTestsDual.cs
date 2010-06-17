using System;

namespace DataVault.Playground
{
    public abstract class VaultTestsDual : VaultTestsCore
    {
        public override void Simple()
        {
            ShutdownEvents();
            base.Simple();
            TearDown();

            SetUp();
            StartupEvents();
            base.Simple();
        }

        public override void Simple2()
        {
            ShutdownEvents();
            base.Simple2();
            TearDown();

            SetUp();
            StartupEvents();
            base.Simple2();
        }

        public override void RecursiveDeleteBranchAndItsContents()
        {
            ShutdownEvents();
            base.RecursiveDeleteBranchAndItsContents();
            TearDown();

            SetUp();
            StartupEvents();
            base.RecursiveDeleteBranchAndItsContents();
        }

        public override void SaveBranchesWithoutValues()
        {
            ShutdownEvents();
            base.SaveBranchesWithoutValues();
            TearDown();

            SetUp();
            StartupEvents();
            base.SaveBranchesWithoutValues();
        }

        public override void SaveBranchAndValueWithTheSameName()
        {
            ShutdownEvents();
            base.SaveBranchAndValueWithTheSameName();
            TearDown();

            SetUp();
            StartupEvents();
            base.SaveBranchAndValueWithTheSameName();
        }

        public override void TestDotNamesThatAreSpecialForFs()
        {
            ShutdownEvents();
            base.TestDotNamesThatAreSpecialForFs();
            TearDown();

            SetUp();
            StartupEvents();
            base.TestDotNamesThatAreSpecialForFs();
        }

        public override void MetadataStuff()
        {
            ShutdownEvents();
            base.MetadataStuff();
            TearDown();

            SetUp();
            StartupEvents();
            base.MetadataStuff();
        }

        public override void ReadExternallyComposedVault()
        {
            ShutdownEvents();
            base.ReadExternallyComposedVault();
            TearDown();

            SetUp();
            StartupEvents();
            base.ReadExternallyComposedVault();
        }

        public override void ConsistencyTest()
        {
            ShutdownEvents();
            base.ConsistencyTest();
            TearDown();

            SetUp();
            StartupEvents();
            base.ConsistencyTest();
        }

        public override void ConsistencyAfterDeleteTest()
        {
            ShutdownEvents();
            base.ConsistencyAfterDeleteTest();
            TearDown();

            SetUp();
            StartupEvents();
            base.ConsistencyAfterDeleteTest();
        }

        public override void TestAttachFromAlienVault()
        {
            ShutdownEvents();
            base.TestAttachFromAlienVault();
            TearDown();

            SetUp();
            StartupEvents();
            base.TestAttachFromAlienVault();
        }

        public override void InternalPropertiesTest()
        {
            ShutdownEvents();
            base.InternalPropertiesTest();
            TearDown();

            SetUp();
            StartupEvents();
            base.InternalPropertiesTest();
        }

        public override void SmartSaveTest()
        {
            ShutdownEvents();
            base.SmartSaveTest();
            TearDown();

            SetUp();
            StartupEvents();
            base.SmartSaveTest();
        }

        public override void TestThreadSafety()
        {
            ShutdownEvents();
            base.TestThreadSafety();
            TearDown();

            SetUp();
            StartupEvents();
            base.TestThreadSafety();
        }

        public override void SomeEventAndRevisionOptimizations()
        {
            ShutdownEvents();
            base.SomeEventAndRevisionOptimizations();
            TearDown();

            SetUp();
            StartupEvents();
            base.SomeEventAndRevisionOptimizations();
        }

        public override void NewMetadataTest()
        {
            ShutdownEvents();
            base.NewMetadataTest();
            TearDown();

            SetUp();
            StartupEvents();
            base.NewMetadataTest();
        }

        public override void NewMetadataTest2()
        {
            ShutdownEvents();
            base.NewMetadataTest2();
            TearDown();

            SetUp();
            StartupEvents();
            base.NewMetadataTest2();
        }

        public override void TestEmptyAndNullBeingDifferentiated()
        {
            ShutdownEvents();
            base.TestEmptyAndNullBeingDifferentiated();
            TearDown();

            SetUp();
            StartupEvents();
            base.TestEmptyAndNullBeingDifferentiated();
        }
    }
}