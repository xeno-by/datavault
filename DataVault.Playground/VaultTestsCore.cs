using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using DataVault.Core.Api;
using DataVault.Core.Helpers;
using DataVault.Core.Helpers.Assertions;
using DataVault.Core.Impl;
using NUnit.Framework;

namespace DataVault.Playground
{
    public abstract class VaultTestsCore : VaultTestsBoilerplate
    {
        public virtual void Simple()
        {
            var content1 = "content1 " + Guid.NewGuid();
            var content2 = "content2 " + Guid.NewGuid();
            var meta1 = "metapew for branch";
            var meta2 = "pewmeta for value";

            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateValue(@"file-1.txt", content1);
                write.BeforeMutate(_history).CreateValue(@"Dir-1\Dir2\file2.txt", content2);
                write.BeforeMutate(_history).GetBranch(@"Dir-1\Dir2\").SetMetadata(meta1);
                write.BeforeMutate(_history).GetValue(@"file-1.txt").SetMetadata(meta2);
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(1, read.BeforeInv(_history).GetBranches().Count());
                Assert.AreEqual(1, read.BeforeInv(_history).GetValues().Count());
                Assert.AreEqual(2, read.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(2, read.BeforeInv(_history).GetValuesRecursive().Count());

                Assert.AreEqual(content1, read.BeforeInv(_history).GetValue(@"file-1.txt").ContentString);
                Assert.AreEqual(content2, read.BeforeInv(_history).GetValue(@"Dir-1\Dir2\file2.txt").ContentString);
                Assert.AreEqual(meta1, (String)read.BeforeInv(_history).GetBranch(@"Dir-1\Dir2\").Metadata);
                Assert.AreEqual(meta2, (String)read.BeforeInv(_history).GetValue(@"file-1.txt").Metadata);

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(12, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/2B)] null -> \\file-1.txt (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\file-1.txt (V)] null -> \\ (B: 1+2/2+2V, 1/2B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                AssertAreEqualExceptGuids("[Content: \\file-1.txt (V)] null -> content1 af28f773-3007-45df-a411-53a109494a7b", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/2B)] null -> \\Dir-1 (B: 0+0/1+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir-1 (B: 0+0/1+0V, 1/1B)] null -> \\ (B: 1+2/2+2V, 1/2B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir-1 (B: 0+0/1+0V, 1/1B)] null -> \\Dir-1\\Dir2 (B: 1+0/1+0V, 0/0B)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir-1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> \\Dir-1 (B: 0+0/1+0V, 1/1B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir-1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> \\Dir-1\\Dir2\\file2.txt (V)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir-1\\Dir2\\file2.txt (V)] null -> \\Dir-1\\Dir2 (B: 1+0/1+0V, 0/0B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                AssertAreEqualExceptGuids("[Content: \\Dir-1\\Dir2\\file2.txt (V)] null -> content2 216fb822-5a62-4317-b698-b2f64898093e", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir-1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> metapew for branch", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\file-1.txt (V)] null -> pewmeta for value", _changed[11].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void Simple2()
        {
            var content1 = "content1 " + Guid.NewGuid();
            var content2 = "content2 " + Guid.NewGuid();

            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateValue(@"file1.txt", content1);
                write.BeforeMutate(_history).CreateValue(@"Dir1\Dir2\file2.txt", content2);
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(1, read.BeforeInv(_history).GetBranches().Count());
                Assert.AreEqual(1, read.BeforeInv(_history).GetValues().Count());
                Assert.AreEqual(2, read.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(2, read.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual(content1, read.BeforeInv(_history).GetValue(@"file1.txt").BeforeInv(_history).ContentString);
                Assert.AreEqual(content2, read.BeforeInv(_history).GetValue(@"Dir1\Dir2\file2.txt").BeforeInv(_history).ContentString);

                var v1 = read.BeforeInv(_history).GetValue(@"file1.txt");
                var v2 = read.BeforeInv(_history).GetValue(@"Dir1\Dir2\file2.txt");
                v1.BeforeMutate(_history).Rename("file5");
                v2.BeforeMutate(_history).Delete();
                read.BeforeInv(_history).Save();

                Assert.AreEqual(content1, v1.BeforeInv(_history).ContentString);
                Assert.AreEqual(content2, v2.BeforeInv(_history).ContentString);

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(13, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/2B)] null -> \\file1.txt (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\file1.txt (V)] null -> \\ (B: 1+2/2+2V, 1/2B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                AssertAreEqualExceptGuids("[Content: \\file1.txt (V)] null -> content1 7af0d0ef-cb42-49e5-9e90-01199a9dbbce", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/2B)] null -> \\Dir1 (B: 0+0/1+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (B: 0+0/1+0V, 1/1B)] null -> \\ (B: 1+2/2+2V, 1/2B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 0+0/1+0V, 1/1B)] null -> \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> \\Dir1 (B: 0+0/1+0V, 1/1B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> \\Dir1\\Dir2\\file2.txt (V)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2\\file2.txt (V)] null -> \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                AssertAreEqualExceptGuids("[Content: \\Dir1\\Dir2\\file2.txt (V)] null -> content2 3617a1aa-ec6a-48d0-9522-b2583c9e2843", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\file5 (V)] file1.txt -> file5", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementRemove: \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)] \\Dir1\\Dir2\\file2.txt (V) -> null", _changed[11].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Remove: \\Dir1\\Dir2\\file2.txt (V)] \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B) -> null", _changed[12].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void RecursiveDeleteBranchAndItsContents()
        {
            var content1 = "content1 " + Guid.NewGuid();
            var content2 = "content2 " + Guid.NewGuid();

            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateValue(@"file1.txt", content1);
                write.BeforeMutate(_history).CreateValue(@"Dir1\Dir2\file2.txt", content2);
                write.BeforeInv(_history).Save();
            }

            using (var write = OpenVault("testvault"))
            {
                write.BeforeInv(_history).GetBranch(@"Dir1").BeforeMutate(_history).Delete();
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(0, read.BeforeInv(_history).GetBranches().Count());
                Assert.AreEqual(1, read.BeforeInv(_history).GetValues().Count());
                Assert.AreEqual(0, read.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(1, read.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual(content1, read.BeforeInv(_history).GetValue(@"file1.txt").ContentString);

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(12, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/2B)] null -> \\file1.txt (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\file1.txt (V)] null -> \\ (B: 1+2/2+2V, 1/2B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                AssertAreEqualExceptGuids("[Content: \\file1.txt (V)] null -> content1 13898511-8b58-4b2c-b604-067691d929f8", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/2B)] null -> \\Dir1 (B: 0+0/1+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (B: 0+0/1+0V, 1/1B)] null -> \\ (B: 1+2/2+2V, 1/2B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 0+0/1+0V, 1/1B)] null -> \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> \\Dir1 (B: 0+0/1+0V, 1/1B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)] null -> \\Dir1\\Dir2\\file2.txt (V)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2\\file2.txt (V)] null -> \\Dir1\\Dir2 (B: 1+0/1+0V, 0/0B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                AssertAreEqualExceptGuids("[Content: \\Dir1\\Dir2\\file2.txt (V)] null -> content2 b6f00461-da5b-40b5-89e6-bc3aa274a8b2", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementRemove: \\ (B: 1+2/1+2V, 0/0B)] \\Dir1 (B: 0+0/1+0V, 1/1B) -> null", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Remove: \\Dir1 (B: 0+0/1+0V, 1/1B)] \\ (B: 1+2/1+2V, 0/0B) -> null", _changed[11].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void SaveBranchesWithoutValues()
        {
            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateBranch("/Dir1/Dir15");
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(1, read.BeforeInv(_history).GetBranches().Count());
                Assert.AreEqual(0, read.BeforeInv(_history).GetValues().Count());
                Assert.AreEqual(2, read.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(0, read.BeforeInv(_history).GetValuesRecursive().Count());

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(4, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 0+2/0+2V, 1/2B)] null -> \\Dir1 (B: 0+0/0+0V, 1/1B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (B: 0+0/0+0V, 1/1B)] null -> \\ (B: 0+2/0+2V, 1/2B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 0+0/0+0V, 1/1B)] null -> \\Dir1\\Dir15 (B: 0+0/0+0V, 0/0B)", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir15 (B: 0+0/0+0V, 0/0B)] null -> \\Dir1 (B: 0+0/0+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void SaveBranchAndValueWithTheSameName()
        {
            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateBranch("/Dir1").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("Dir1", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeMutate(_history).CreateBranch("/Dir1/Dir2").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("/Dir1/Dir2", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeMutate(_history).CreateBranch("/Dir1/Dir3").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("/Dir1/File1", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(1, read.BeforeInv(_history).GetBranches().Count());
                Assert.AreEqual(1, read.BeforeInv(_history).GetValues().Count());
                Assert.AreEqual(3, read.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(3, read.BeforeInv(_history).GetValuesRecursive().Count());

                AssertState(read.BeforeInv(_history).GetBranch("/Dir1"), "dir2data");
                AssertState(read.BeforeInv(_history).GetValue("/Dir1"), "pewdata", "pew");
                AssertState(read.BeforeInv(_history).GetBranch("/Dir1/Dir2"), "dir2data");
                AssertState(read.BeforeInv(_history).GetValue("/Dir1/Dir2"), "pewdata", "pew");
                AssertState(read.BeforeInv(_history).GetBranch("/Dir1/Dir3"), "dir2data");
                AssertState(read.BeforeInv(_history).GetValue("/Dir1/File1"), "pewdata", "pew");

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(21, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 1/3B)] null -> \\Dir1 (B: 2+0/2+0V, 2/2B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (B: 2+0/2+0V, 2/2B)] null -> \\ (B: 1+2/3+2V, 1/3B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1 (B: 2+0/2+0V, 2/2B)] null -> dir2data", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 1/3B)] null -> \\Dir1 (V)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (V)] null -> \\ (B: 1+2/3+2V, 1/3B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\Dir1 (V)] null -> pew", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1 (V)] null -> pewdata", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 2+0/2+0V, 2/2B)] null -> \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)] null -> \\Dir1 (B: 2+0/2+0V, 2/2B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)] null -> dir2data", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 2+0/2+0V, 2/2B)] null -> \\Dir1\\Dir2 (V)", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2 (V)] null -> \\Dir1 (B: 2+0/2+0V, 2/2B)", _changed[11].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\Dir1\\Dir2 (V)] null -> pew", _changed[12].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1\\Dir2 (V)] null -> pewdata", _changed[13].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 2+0/2+0V, 2/2B)] null -> \\Dir1\\Dir3 (B: 0+0/0+0V, 0/0B)", _changed[14].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir3 (B: 0+0/0+0V, 0/0B)] null -> \\Dir1 (B: 2+0/2+0V, 2/2B)", _changed[15].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1\\Dir3 (B: 0+0/0+0V, 0/0B)] null -> dir2data", _changed[16].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 2+0/2+0V, 2/2B)] null -> \\Dir1\\File1 (V)", _changed[17].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\File1 (V)] null -> \\Dir1 (B: 2+0/2+0V, 2/2B)", _changed[18].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\Dir1\\File1 (V)] null -> pew", _changed[19].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1\\File1 (V)] null -> pewdata", _changed[20].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void TestDotNamesThatAreSpecialForFs()
        {
            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateBranch("/..").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("..", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeMutate(_history).CreateBranch("/../Dir2").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("/../Dir2", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeMutate(_history).CreateBranch("/../.").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("/../File1", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(1, read.BeforeInv(_history).GetBranches().Count());
                Assert.AreEqual(1, read.BeforeInv(_history).GetValues().Count());
                Assert.AreEqual(3, read.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(3, read.BeforeInv(_history).GetValuesRecursive().Count());

                AssertState(read.BeforeInv(_history).GetBranch("/.."), "dir2data");
                AssertState(read.BeforeInv(_history).GetValue("/.."), "pewdata", "pew");
                AssertState(read.BeforeInv(_history).GetBranch("/../Dir2"), "dir2data");
                AssertState(read.BeforeInv(_history).GetValue("/../Dir2"), "pewdata", "pew");
                AssertState(read.BeforeInv(_history).GetBranch("/../."), "dir2data");
                AssertState(read.BeforeInv(_history).GetValue("/../File1"), "pewdata", "pew");

                if (_trackingEvents)
                {
                    Assert.AreEqual(21, _changed.Count);
                    Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 1/3B)] null -> \\.. (B: 2+0/2+0V, 2/2B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Add: \\.. (B: 2+0/2+0V, 2/2B)] null -> \\ (B: 1+2/3+2V, 1/3B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Metadata: \\.. (B: 2+0/2+0V, 2/2B)] null -> dir2data", _changed[2].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 1/3B)] null -> \\.. (V)", _changed[3].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Add: \\.. (V)] null -> \\ (B: 1+2/3+2V, 1/3B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Content: \\.. (V)] null -> pew", _changed[5].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Metadata: \\.. (V)] null -> pewdata", _changed[6].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[ElementAdd: \\.. (B: 2+0/2+0V, 2/2B)] null -> \\..\\Dir2 (B: 0+0/0+0V, 0/0B)", _changed[7].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Add: \\..\\Dir2 (B: 0+0/0+0V, 0/0B)] null -> \\.. (B: 2+0/2+0V, 2/2B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Metadata: \\..\\Dir2 (B: 0+0/0+0V, 0/0B)] null -> dir2data", _changed[9].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[ElementAdd: \\.. (B: 2+0/2+0V, 2/2B)] null -> \\..\\Dir2 (V)", _changed[10].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Add: \\..\\Dir2 (V)] null -> \\.. (B: 2+0/2+0V, 2/2B)", _changed[11].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Content: \\..\\Dir2 (V)] null -> pew", _changed[12].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Metadata: \\..\\Dir2 (V)] null -> pewdata", _changed[13].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[ElementAdd: \\.. (B: 2+0/2+0V, 2/2B)] null -> \\..\\. (B: 0+0/0+0V, 0/0B)", _changed[14].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Add: \\..\\. (B: 0+0/0+0V, 0/0B)] null -> \\.. (B: 2+0/2+0V, 2/2B)", _changed[15].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Metadata: \\..\\. (B: 0+0/0+0V, 0/0B)] null -> dir2data", _changed[16].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[ElementAdd: \\.. (B: 2+0/2+0V, 2/2B)] null -> \\..\\File1 (V)", _changed[17].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Add: \\..\\File1 (V)] null -> \\.. (B: 2+0/2+0V, 2/2B)", _changed[18].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Content: \\..\\File1 (V)] null -> pew", _changed[19].ToStringThatsFriendlyToUnitTests());
                    Assert.AreEqual("[Metadata: \\..\\File1 (V)] null -> pewdata", _changed[20].ToStringThatsFriendlyToUnitTests());
                }
            }
        }

        public virtual void MetadataStuff()
        {
            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateBranch("/Dir1");
                write.BeforeMutate(_history).CreateBranch("/Dir1/Dir2").BeforeMutate(_history).SetMetadata("dir2data");
                write.BeforeMutate(_history).CreateValue("Dir1", "pew").BeforeMutate(_history).SetMetadata("pewdata");
                write.BeforeInv(_history).Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(null, (String)read.BeforeInv(_history).GetBranch("/Dir1").BeforeInv(_history).Metadata);
                Assert.AreEqual("dir2data", (String)read.BeforeInv(_history).GetBranch("/Dir1/Dir2").BeforeInv(_history).Metadata);
                Assert.AreEqual("pewdata", (String)read.BeforeInv(_history).GetValue("Dir1").BeforeInv(_history).Metadata);

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(9, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/2B)] null -> \\Dir1 (B: 0+0/0+0V, 1/1B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (B: 0+0/0+0V, 1/1B)] null -> \\ (B: 1+2/1+2V, 1/2B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\Dir1 (B: 0+0/0+0V, 1/1B)] null -> \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)] null -> \\Dir1 (B: 0+0/0+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1\\Dir2 (B: 0+0/0+0V, 0/0B)] null -> dir2data", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/2B)] null -> \\Dir1 (V)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\Dir1 (V)] null -> \\ (B: 1+2/1+2V, 1/2B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\Dir1 (V)] null -> pew", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\Dir1 (V)] null -> pewdata", _changed[8].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void ReadExternallyComposedVault()
        {
            using (var sr = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("DataVault.Playground.vault.zip"))
            {
                using (var sw = File.OpenWrite("blueprint.zip"))
                {
                    int @byte;
                    while ((@byte = sr.ReadByte()) != -1)
                    {
                        sw.WriteByte((byte)@byte);
                    }
                }
            }

            using (var read = LoadVaultBlueprint("blueprint.zip"))
            {
                // this tests against a sneaky bug
                read.Backup();

                var root = read.Root;
                Assert.AreEqual(null, root.Name);
                Assert.AreEqual("root mdata", (String)root.Metadata);
                Assert.AreEqual(1, read.GetBranches().Count());
                Assert.AreEqual(0, read.GetValues().Count());
                Assert.AreEqual(4, read.GetBranchesRecursive().Count());
                Assert.AreEqual(2, read.GetValuesRecursive().Count());

                var testarc = root.GetBranches().Single();
                Assert.AreEqual("Testarc", testarc.Name);
                Assert.AreEqual("tesarc! metadata", (String)testarc.Metadata);
                Assert.AreEqual(1, testarc.GetBranches().Count());
                Assert.AreEqual(1, testarc.GetValues().Count());
                Assert.AreEqual(3, testarc.GetBranchesRecursive().Count());
                Assert.AreEqual(2, testarc.GetValuesRecursive().Count());

                var hai = testarc.GetValues().Single();
                Assert.AreEqual("hai.txt", hai.Name);
                Assert.AreEqual("pewpewpew hai!", hai.ContentString);
                Assert.AreEqual("hai metadata", (String)hai.Metadata);

                var dir1 = testarc.GetBranches().Single();
                Assert.AreEqual("Dir1", dir1.Name);
                Assert.AreEqual(null, (String)dir1.Metadata);
                Assert.AreEqual(2, dir1.GetBranches().Count());
                Assert.AreEqual(0, dir1.GetValues().Count());
                Assert.AreEqual(2, dir1.GetBranchesRecursive().Count());
                Assert.AreEqual(1, dir1.GetValuesRecursive().Count());

                var dir2 = dir1.GetBranches().First();
                Assert.AreEqual("Dir2", dir2.Name);
                Assert.AreEqual(null, (String)dir2.Metadata);
                Assert.AreEqual(0, dir2.GetBranches().Count());
                Assert.AreEqual(1, dir2.GetValues().Count());
                Assert.AreEqual(0, dir2.GetBranchesRecursive().Count());
                Assert.AreEqual(1, dir2.GetValuesRecursive().Count());

                var yaf = dir2.GetValues().Single();
                Assert.AreEqual("yaf", yaf.Name);
                Assert.AreEqual("yet another file", yaf.ContentString);
                Assert.AreEqual(null, (String)yaf.Metadata);

                var dir3 = dir1.GetBranches().Last();
                Assert.AreEqual("Dir3", dir3.Name);
                Assert.AreEqual("dir3 metadata", (String)dir3.Metadata);
                Assert.AreEqual(0, dir3.GetBranches().Count());
                Assert.AreEqual(0, dir3.GetValues().Count());
                Assert.AreEqual(0, dir3.GetBranchesRecursive().Count());
                Assert.AreEqual(0, dir3.GetValuesRecursive().Count());
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(0, _changed.Count);
            }
        }

        public virtual void ConsistencyTest()
        {
            using (var write = OpenVault("testvault"))
            {
                var val = write.BeforeMutate(_history).CreateValue("val1", "pew");
                AssertState(val, null, "pew", false, true);
                write.BeforeInv(_history).Save();
                AssertState(val, null, "pew", false, false);

                val.BeforeMutate(_history).SetContent("newpew").BeforeMutate(_history).SetMetadata("mdata");
                AssertState(val, "mdata", "newpew", true, true);
                write.BeforeInv(_history).Save();
                AssertState(val, "mdata", "newpew", false, false);

                val.BeforeMutate(_history).Rename("hallo world");
                AssertState(val, "mdata", "newpew", true, true);
                write.BeforeInv(_history).Save();
                AssertState(val, "mdata", "newpew", false, false);

                var br = (IBranch)write.BeforeMutate(_history).CreateBranch("br1").BeforeMutate(_history).SetMetadata("bleh");
                AssertState(br, "bleh", true);
                write.BeforeInv(_history).Save();
                AssertState(br, "bleh", false);

                br.BeforeMutate(_history).Rename("hallo world");
                AssertState(br, "bleh", true);
                write.BeforeInv(_history).Save();
                AssertState(br, "bleh", false);
            }

            using (var read = OpenVault("testvault"))
            {
                var val = read.BeforeInv(_history).GetValue("hallo world");
                AssertState(val, "mdata", "newpew", false, false);
                read.BeforeInv(_history).Save();
                AssertState(val, "mdata", "newpew", false, false);

                var br = read.BeforeInv(_history).GetBranch("hallo world");
                AssertState(br, "bleh", false);
                read.BeforeInv(_history).Save();
                AssertState(br, "bleh", false);

                read.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(10, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\hallo world (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\hallo world (V)] null -> \\ (B: 1+2/1+2V, 1/1B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\hallo world (V)] null -> pew", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\hallo world (V)] pew -> newpew", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\hallo world (V)] null -> mdata", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\hallo world (V)] val1 -> hallo world", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\hallo world (B: 0+0/0+0V, 0/0B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\hallo world (B: 0+0/0+0V, 0/0B)] null -> \\ (B: 1+2/1+2V, 1/1B)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\hallo world (B: 0+0/0+0V, 0/0B)] null -> bleh", _changed[8].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\hallo world (B: 0+0/0+0V, 0/0B)] br1 -> hallo world", _changed[9].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void ConsistencyAfterDeleteTest()
        {
            using (var write = OpenVault("testvault"))
            {
                var val = (IValue)write.BeforeMutate(_history).CreateValue("val1", "pew").BeforeMutate(_history).SetMetadata("metapew");
                AssertState(val, "metapew", "pew", true, true);
                write.BeforeInv(_history).Save();
                AssertState(val, "metapew", "pew", false, false);

                val.BeforeMutate(_history).Delete();
                AssertState(val, "metapew", "pew", false, false);
                write.BeforeInv(_history).Save();
                AssertState(val, "metapew", "pew", false, false);

                write.BeforeMutate(_history).AttachValue(val);
                AssertState(val, "metapew", "pew", false, false);
                write.BeforeInv(_history).Save();
                AssertState(val, "metapew", "pew", false, false);

                var br = (IBranch)write.BeforeMutate(_history).CreateBranch("br1").BeforeMutate(_history).SetMetadata("brmeta");
                AssertState(br, "brmeta", true);
                write.BeforeInv(_history).Save();
                AssertState(br, "brmeta", false);

                br.BeforeMutate(_history).Delete();
                AssertState(br, "brmeta", false);
                write.BeforeInv(_history).Save();
                AssertState(br, "brmeta", false);

                write.BeforeMutate(_history).AttachBranch(br);
                AssertState(br, "brmeta", false);
                write.BeforeInv(_history).Save();
                AssertState(br, "brmeta", false);

                write.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(15, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\val1 (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (V)] null -> \\ (B: 1+2/1+2V, 1/1B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val1 (V)] null -> pew", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\val1 (V)] null -> metapew", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementRemove: \\ (B: 1+2/1+2V, 1/1B)] \\val1 (V) -> null", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Remove: \\val1 (V)] \\ (B: 1+2/1+2V, 1/1B) -> null", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\val1 (V)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (V)] \\ (B: 1+2/1+2V, 1/1B) -> \\ (B: 1+2/1+2V, 1/1B)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\br1 (B: 0+0/0+0V, 0/0B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\br1 (B: 0+0/0+0V, 0/0B)] null -> \\ (B: 1+2/1+2V, 1/1B)", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\br1 (B: 0+0/0+0V, 0/0B)] null -> brmeta", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementRemove: \\ (B: 1+2/1+2V, 1/1B)] \\br1 (B: 0+0/0+0V, 0/0B) -> null", _changed[11].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Remove: \\br1 (B: 0+0/0+0V, 0/0B)] \\ (B: 1+2/1+2V, 1/1B) -> null", _changed[12].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\br1 (B: 0+0/0+0V, 0/0B)", _changed[13].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\br1 (B: 0+0/0+0V, 0/0B)] \\ (B: 1+2/1+2V, 1/1B) -> \\ (B: 1+2/1+2V, 1/1B)", _changed[14].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void TestAttachFromAlienVault()
        {
            using (var write = OpenVault("testvault"))
            {
                write.BeforeMutate(_history).CreateValue("val1", "val1");
                write.BeforeMutate(_history).CreateValue(@"val1\val2", "val1val2");
                write.BeforeMutate(_history).CreateBranch(@"val1\bra1");
                write.BeforeMutate(_history).CreateValue(@"val2\val3", "val2val3");
                write.BeforeInv(_history).Save();

                var history2 = new RevisionHistory();
                using (var write2 = OpenVault("testvault2"))
                {
                    write2.BeforeMutate(history2).ImportFrom(write.BeforeInv(_history));
                    write2.BeforeInv(history2).Save();

                    Assert.AreEqual("val1", write2.BeforeInv(history2).GetValue("val1").ContentString);
                    Assert.AreEqual("val1val2", write2.BeforeInv(history2).GetValue(@"val1\val2").ContentString);
                    Assert.IsNotNull(write2.BeforeInv(history2).GetBranch(@"val1\bra1"));
                    Assert.AreEqual("val2val3", write2.BeforeInv(history2).GetValue(@"val2\val3").ContentString);

                    write2.Verify(history2);
                }

                write.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(40, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 2/3B)] null -> \\val1 (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (V)] null -> \\ (B: 1+2/3+2V, 2/3B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val1 (V)] null -> val1", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 2/3B)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\ (B: 1+2/3+2V, 2/3B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\val1\\val2 (V)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1\\val2 (V)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val1\\val2 (V)] null -> val1val2", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\val1\\bra1 (B: 0+0/0+0V, 0/0B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1\\bra1 (B: 0+0/0+0V, 0/0B)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 2/3B)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\ (B: 1+2/3+2V, 2/3B)", _changed[11].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\val2\\val3 (V)", _changed[12].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2\\val3 (V)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[13].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val2\\val3 (V)] null -> val2val3", _changed[14].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\val1\\bra1 (B: 0+0/0+0V, 0/0B)", _changed[15].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1\\bra1 (B: 0+0/0+0V, 0/0B)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[16].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\val1\\val2 (V)", _changed[17].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1\\val2 (V)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[18].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\val2\\val3 (V)", _changed[19].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2\\val3 (V)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[20].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+0/3+0V, 2/3B)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[21].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\ (B: 1+0/3+0V, 2/3B)", _changed[22].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+0/3+0V, 2/3B)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[23].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\ (B: 1+0/3+0V, 2/3B)", _changed[24].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+0/3+0V, 2/3B)] null -> \\val1 (V)", _changed[25].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (V)] null -> \\ (B: 1+0/3+0V, 2/3B)", _changed[26].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\ (B: 1+2/3+2V, 2/3B)] null -> null", _changed[27].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\val1\\bra1 (B: 0+0/0+0V, 0/0B)", _changed[28].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1\\bra1 (B: 0+0/0+0V, 0/0B)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[29].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\val1\\val2 (V)", _changed[30].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1\\val2 (V)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[31].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 2/3B)] null -> \\val1 (B: 1+0/1+0V, 1/1B)", _changed[32].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (B: 1+0/1+0V, 1/1B)] null -> \\ (B: 1+2/3+2V, 2/3B)", _changed[33].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\val2\\val3 (V)", _changed[34].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2\\val3 (V)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[35].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 2/3B)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[36].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\ (B: 1+2/3+2V, 2/3B)", _changed[37].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/3+2V, 2/3B)] null -> \\val1 (V)", _changed[38].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val1 (V)] null -> \\ (B: 1+2/3+2V, 2/3B)", _changed[39].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void InternalPropertiesTest()
        {
            Guid id, id2, id3, id4, id5, bid, bid2;
            UInt64 rev, rev2, rev3, rev4, rev5;

            using (var write = OpenVault("testvault"))
            {
                write.BeforeInv(_history).Save();
                id = write.BeforeInv(_history).Id;
                rev = write.BeforeInv(_history).Revision;

                Assert.AreNotEqual(Guid.Empty, id);
                Assert.AreEqual(0, rev);

                var b = write.BeforeMutate(_history).CreateBranch("test");
                bid = b.Id;
                write.BeforeInv(_history).Save();

                id2 = write.BeforeInv(_history).Id;
                rev2 = write.BeforeInv(_history).Revision;

                Assert.AreEqual(id, id2);
                Assert.IsTrue(rev2 > rev);
            }

            using (var read = OpenVault("testvault"))
            {
                id3 = read.BeforeInv(_history).Id;
                bid2 = read.GetBranch("test").Id;
                rev3 = read.BeforeInv(_history).Revision;

                Assert.AreEqual(id, id3);
                Assert.AreEqual(bid, bid2);
                Assert.AreEqual(rev2, rev3);

                _history.Verify(read);
                var history2 = new RevisionHistory();

                read.SaveAs("testvault2");
                id4 = read.BeforeInv(history2).Id;
                rev4 = read.BeforeInv(history2).Revision;

                Assert.AreNotEqual(id3, id4);
                Assert.AreEqual(0, rev4);

                using (var read2 = OpenVault("testvault2"))
                {
                    id5 = read2.BeforeInv(history2).Id;
                    rev5 = read2.BeforeInv(history2).Revision;

                    Assert.AreEqual(id4, id5);
                    Assert.AreEqual(rev4, rev5);

                    history2.Verify(read2);
                }

                history2.Verify(read);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(2, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 0+2/0+2V, 1/1B)] null -> \\test (B: 0+0/0+0V, 0/0B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\test (B: 0+0/0+0V, 0/0B)] null -> \\ (B: 0+2/0+2V, 1/1B)", _changed[1].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void SmartSaveTest()
        {
            using (var write = OpenVault("testvault"))
            {
                // step 1. nothing special
                write.BeforeMutate(_history).CreateValue("val1", "val1");
                write.BeforeMutate(_history).CreateBranch(@"val3\bra1");
                write.BeforeInv(_history).Save();

                Assert.AreEqual(2, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(1, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val1", write.BeforeInv(_history).GetValue("val1").BeforeInv(_history).ContentString);

                // step 2. updating content of the same file
                write.BeforeInv(_history).GetValue("val1").BeforeMutate(_history).SetContent("val1*");
                write.BeforeInv(_history).Save();

                Assert.AreEqual(2, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(1, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val1*", write.BeforeInv(_history).GetValue("val1").BeforeInv(_history).ContentString);

                // step 3. updating content of renamed file
                // additional twist: name of the file matches one of a dir
                write.BeforeInv(_history).GetValue("val1").BeforeMutate(_history).Rename("val3");
                write.BeforeMutate(_history).CreateValue("val2", "val2");
                write.BeforeInv(_history).Save();

                Assert.AreEqual(2, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(2, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val2", write.BeforeInv(_history).GetValue("val2").ContentString);
                Assert.AreEqual("val1*", write.BeforeInv(_history).GetValue("val3").ContentString);

                // step 4. renaming file just into a freshly deleted brother
                // additional twist: do not update content, i.e. no hints about modified
                write.BeforeInv(_history).GetValue("val2").BeforeMutate(_history).Delete();
                write.BeforeInv(_history).GetValue("val3").BeforeMutate(_history).Rename("val2");
                write.BeforeInv(_history).Save();

                Assert.AreEqual(2, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(1, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val1*", write.BeforeInv(_history).GetValue("val2").BeforeInv(_history).ContentString);

                // step 5. renaming directory to match existing filename
                // author's comment: sigh, this sucks
                write.BeforeInv(_history).GetBranch("val3").BeforeMutate(_history).Rename("val2");
                write.BeforeInv(_history).Save();

                Assert.AreEqual(2, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(1, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val1*", write.BeforeInv(_history).GetValue("val2").BeforeInv(_history).ContentString);

                // step 6. preliminary
                write.BeforeInv(_history).GetBranch("val2").BeforeMutate(_history).CreateValue("valx", "hai2");
                write.BeforeMutate(_history).CreateBranch("valx").BeforeMutate(_history).CreateValue("valx", "haix");
                write.BeforeInv(_history).Save();

                Assert.AreEqual(3, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(3, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val1*", write.BeforeInv(_history).GetValue("val2").ContentString);
                Assert.AreEqual("hai2", write.BeforeInv(_history).GetValue("val2/valx").ContentString);
                Assert.AreEqual("haix", write.BeforeInv(_history).GetValue("valx/valx").ContentString);

                // step 7. renaming directory so that its subvalue matches existing filename
                write.BeforeInv(_history).GetBranch("val2").BeforeMutate(_history).Delete();
                write.BeforeInv(_history).GetBranch("valx").BeforeMutate(_history).Rename("val2");

                Assert.AreEqual(1, write.BeforeInv(_history).GetBranchesRecursive().Count());
                Assert.AreEqual(2, write.BeforeInv(_history).GetValuesRecursive().Count());
                Assert.AreEqual("val1*", write.BeforeInv(_history).GetValue("val2").ContentString);
                Assert.AreEqual("haix", write.BeforeInv(_history).GetValue("val2/valx").ContentString);

                write.Verify(_history);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(27, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/1B)] null -> \\val2 (V)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (V)] null -> \\ (B: 1+2/2+2V, 1/1B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val2 (V)] null -> val1", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/1B)] null -> \\val2 (B: 1+0/1+0V, 1/1B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (B: 1+0/1+0V, 1/1B)] null -> \\ (B: 1+2/2+2V, 1/1B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val2 (B: 1+0/1+0V, 1/1B)] null -> \\val2\\bra1 (B: 0+0/0+0V, 0/0B)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2\\bra1 (B: 0+0/0+0V, 0/0B)] null -> \\val2 (B: 1+0/1+0V, 1/1B)", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val2 (V)] val1 -> val1*", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\val2 (V)] val1 -> val3", _changed[8].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/1B)] null -> \\val2 (V)", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (V)] null -> \\ (B: 1+2/2+2V, 1/1B)", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val2 (V)] null -> val2", _changed[11].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementRemove: \\ (B: 1+2/2+2V, 1/1B)] \\val2 (V) -> null", _changed[12].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Remove: \\val2 (V)] \\ (B: 1+2/2+2V, 1/1B) -> null", _changed[13].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\val2 (V)] val3 -> val2", _changed[14].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\val2 (B: 1+0/1+0V, 1/1B)] val3 -> val2", _changed[15].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val2 (B: 1+0/1+0V, 1/1B)] null -> \\val2\\valx (V)", _changed[16].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2\\valx (V)] null -> \\val2 (B: 1+0/1+0V, 1/1B)", _changed[17].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val2\\valx (V)] null -> hai2", _changed[18].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/2+2V, 1/1B)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[19].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\ (B: 1+2/2+2V, 1/1B)", _changed[20].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\val2 (B: 1+0/1+0V, 0/0B)] null -> \\val2\\valx (V)", _changed[21].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\val2\\valx (V)] null -> \\val2 (B: 1+0/1+0V, 0/0B)", _changed[22].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\val2\\valx (V)] null -> haix", _changed[23].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementRemove: \\ (B: 1+2/2+2V, 1/1B)] \\val2 (B: 1+0/1+0V, 1/1B) -> null", _changed[24].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Remove: \\val2 (B: 1+0/1+0V, 1/1B)] \\ (B: 1+2/2+2V, 1/1B) -> null", _changed[25].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Rename: \\val2 (B: 1+0/1+0V, 0/0B)] valx -> val2", _changed[26].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void TestThreadSafety()
        {
            using (var write = OpenVault("testvault"))
            {
                // this is necessary to test multi-threaded reading
                write.CreateValue("multiread", "multiread");
                write.Save();
            }

            using (var read = OpenVault("testvault"))
            {
                const int maxThreads = 20;

                var arrivedToTheStartLine = 0;
                var everyoneArrivedToTheStart = new ManualResetEvent(false);
                var starter = new ManualResetEvent(false);
                var crossroads = 0;
                var fancyTicket = 0;
                var haveBeenToFancyPlace = 0;
                var finished = 0;
                var everyoneFinished = new ManualResetEvent(false);

                var errorLog = new Dictionary<String, Exception>();
                ThreadStart threadRoutine = () =>
                {
                    var startPosition = -1;

                    try
                    {
                        using (read.ExposeReadOnly())
                        {
                            startPosition = Interlocked.Increment(ref arrivedToTheStartLine);
                            if (startPosition == maxThreads)
                            {
                                everyoneArrivedToTheStart.Set();
                            }

                            starter.WaitOne();
                            Assert.AreEqual("multiread", read.GetValue("multiread").ContentString);

                            var ticket = Interlocked.Increment(ref crossroads);
                            if (ticket <= 2)
                            {
                                using (read.ExposeReadWrite())
                                {
                                    fancyTicket = Interlocked.Increment(ref fancyTicket);
                                    if (fancyTicket == 1)
                                    {
                                        // doesn't guarantee anything, but strengthens the test
                                        Thread.Sleep(500);
                                        var arrivalOrder = Interlocked.Increment(ref haveBeenToFancyPlace);
                                        Assert.AreEqual(1, arrivalOrder);

                                        read.CreateBranch("conflict");
                                    }
                                    else if (fancyTicket == 2)
                                    {
                                        var arrivalOrder = Interlocked.Increment(ref haveBeenToFancyPlace);
                                        Assert.AreEqual(2, arrivalOrder);

                                        var exceptionOk = false;
                                        try
                                        {
                                            read.CreateBranch("conflict");
                                        }
                                        // todo. yeah, messy exception vaults have, I know
                                        catch (Exception)
                                        {
                                            exceptionOk = true;
                                        }

                                        Assert.IsTrue(exceptionOk, "Expected exception wasn't thrown");
                                    }
                                    else
                                    {
                                        Assert.Fail("You need to rewrite this test");
                                    }
                                }
                            }

                            Assert.AreEqual("multiread", read.GetValue("multiread").ContentString);

                            var correlationId = Guid.NewGuid().ToString();
                            var correlationContent = Guid.NewGuid().ToString();
                            read.CreateValue("v1_" + correlationId, correlationContent);
                            read.CreateValue("v2_" + correlationId, correlationContent);
                            read.Save();
                        }
                    }
                    catch (Exception ex)
                    {
                        errorLog[String.Format("#={0}, id={1}", startPosition, Thread.CurrentThread.ManagedThreadId)] = ex;
                    }
                    finally
                    {
                        var finishPosition = Interlocked.Increment(ref finished);
                        if (finishPosition == maxThreads)
                        {
                            everyoneFinished.Set();
                        }
                    }
                };

                // Prepare the participants
                1.Seq(i => i + 1, i => i <= maxThreads).ForEach(i => new Thread(threadRoutine).Start());
                everyoneArrivedToTheStart.WaitOne(500);
                Assert.AreEqual(maxThreads, arrivedToTheStartLine);

                // Start the race and wait for the finish
                starter.Set();
                everyoneFinished.WaitOne(5000);

                // Check the error log and crash if it has any entries
                errorLog.ForEach(kvp => Assert.Fail(
                    String.Format("Thread '{1}' has got an error: {0}{2}", Environment.NewLine, kvp.Key, kvp.Value)));

                // Analyze the results (as well certain tests were already done inside some threads)
                Assert.AreEqual(maxThreads, crossroads);
                Assert.AreEqual(2, haveBeenToFancyPlace);
                Assert.AreEqual(maxThreads, finished);

                // Verify simultaneous writes
                using (var verify = OpenVault("testvault"))
                {
                    var correlations = verify.GetValues().Where(v => v.Name != "multiread");
                    Assert.AreEqual(maxThreads * 2, correlations.Count());

                    foreach (var v in correlations)
                    {
                        var name = v.Name.Contains("v1_") ? "v2_" : "v1_";
                        var corrId = v.Name.Substring(v.Name.IndexOf("_") + 1);

                        var v_corr = verify.GetValue(name + corrId);
                        Assert.IsNotNull(v_corr);
                        Assert.AreEqual(v.ContentString, v_corr.ContentString);
                    }
                }
            }
        }

        public virtual void SomeEventAndRevisionOptimizations()
        {
            UInt64 rev, rev2, rev3, rev4, rev5, rev6, rev7;

            using (var write = OpenVault("testvault"))
            {
                rev = write.BeforeInv(_history).Revision;
                Assert.AreEqual(0, rev);

                var b = write.BeforeMutate(_history).CreateBranch("test");
                b.BeforeMutate(_history).SetMetadata("mdata");
                var v = write.BeforeMutate(_history).CreateValue("test", "content");
                v.BeforeMutate(_history).SetMetadata("mdata");
                rev2 = write.BeforeInv(_history).Revision;
                Assert.IsTrue(rev2 > rev);

                b.BeforeInv(_history).Rename("test");
                rev3 = write.BeforeInv(_history).Revision;
                Assert.IsTrue(rev3 == rev2);

                b.BeforeInv(_history).SetMetadata("mdata");
                rev4 = write.BeforeInv(_history).Revision;
                Assert.IsTrue(rev4 == rev3);

                v.BeforeInv(_history).Rename("test");
                rev5 = write.BeforeInv(_history).Revision;
                Assert.IsTrue(rev5 == rev4);

                v.BeforeInv(_history).SetMetadata("mdata");
                rev6 = write.BeforeInv(_history).Revision;
                Assert.IsTrue(rev6 == rev5);

                v.BeforeInv(_history).SetContent("content");
                rev7 = write.BeforeInv(_history).Revision;
                Assert.IsTrue(rev7 > rev6);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(8, _changed.Count);
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\test (B: 0+0/0+0V, 0/0B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\test (B: 0+0/0+0V, 0/0B)] null -> \\ (B: 1+2/1+2V, 1/1B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\test (B: 0+0/0+0V, 0/0B)] null -> mdata", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 1+2/1+2V, 1/1B)] null -> \\test (V)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\test (V)] null -> \\ (B: 1+2/1+2V, 1/1B)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\test (V)] null -> content", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\test (V)] null -> mdata", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\test (V)] content -> content", _changed[7].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void NewMetadataTest()
        {
            var yas = "yet another sect[[[ion" + Environment.NewLine + "(multiline, doesn't end with a newline)";
            var id = "!]system section that hosts the [=[guid]] of the element (single line)";
            var @default =
                "]!default section content with an opening bracket [ in it" + Environment.NewLine +
                "also continues at the next line with a closing bracket ]" + Environment.NewLine +
                "and at the next line as well (and ends with newline)" + Environment.NewLine;

            var defaultEscaped =
                "]!default section content with an opening bracket [[ in it" + Environment.NewLine +
                "also continues at the next line with a closing bracket ]" + Environment.NewLine +
                "and at the next line as well (and ends with newline)" + Environment.NewLine;

            var yasEscaped = "yet another sect[[[[[[ion" + Environment.NewLine + "(multiline, doesn't end with a newline)";
            var idEscaped = "!!]system section that hosts the [[=[[guid]] of the element (single line)";
            var defaultEscaped2 =
                "!]!default section content with an opening bracket [[ in it" + Environment.NewLine +
                "also continues at the next line with a closing bracket ]" + Environment.NewLine +
                "and at the next line as well (and ends with newline)" + Environment.NewLine;

            using (var write = OpenVault("testvault"))
            {
                var b = write.CreateBranch("test");
                Func<String> im = () => b.Metadata;
                Func<Metadata> m = () => (Metadata)b.Metadata;
                Assert.AreEqual(null, m().Raw);

                b.SetMetadata(@default);
                Assert.AreEqual(@default, im());
                Assert.AreEqual(defaultEscaped, m().Raw);

                b.Metadata["[!id]!"] = id;
                b.Metadata["![yas!!]"] = yas;
                Assert.AreEqual(@default, im());
                Assert.AreEqual(
                    "[!![yas!!]]]" + yasEscaped + Environment.NewLine +
                    "[![!id]]!]" + idEscaped + Environment.NewLine +
                    "[" + CoreConstants.DefaultSection + "]" + defaultEscaped2,
                    m().Raw);
                write.SaveAs("testvault3");

                b.SetMetadata(null);
                b.Metadata["![yas!!]"] = null;
                Assert.AreEqual(null, im());
                Assert.AreEqual("[![!id]]!]" + idEscaped, m().Raw);

                b.Metadata["[!id]!"] = null;
                Assert.AreEqual(null, im());
                Assert.AreEqual(null, m().Raw);
            }

            using (var read = OpenVault("testvault3"))
            {
                var b = read.GetBranch("test");

                Assert.AreEqual(@default, (String)b.Metadata);
                Assert.AreEqual(id, b.Metadata["[!id]!"]);
                Assert.AreEqual(yas, b.Metadata["![yas!!]"]);
            }

            using (var write = OpenVault("testvault2"))
            {
                var b = write.CreateBranch("test");
                b.SetMetadata(String.Empty);
                b.Metadata["ea"] = "a";
                b.Metadata["emid"] = String.Empty;
                b.Metadata["ex"] = "x";
                b.Metadata["ez"] = String.Empty;

                Assert.AreEqual(
                    "[" + CoreConstants.DefaultSection + "]" + Environment.NewLine +
                    "[ea]a" + Environment.NewLine +
                    "[emid]" + Environment.NewLine +
                    "[ex]x" + Environment.NewLine +
                    "[ez]", ((Metadata)b.Metadata).Raw);
                write.Save();
            }

            using (var read = OpenVault("testvault2"))
            {
                var b = read.GetBranch("test");

                Assert.AreEqual(String.Empty, (String)b.Metadata);
                Assert.AreEqual("a", b.Metadata["ea"]);
                Assert.AreEqual(String.Empty, b.Metadata["emid"]);
                Assert.AreEqual("x", b.Metadata["ex"]);
                Assert.AreEqual(String.Empty, b.Metadata["ez"]);
            }

            // no mutation tests since they look ugly
            // no event history here since it looks ugly
            // don't believe me? try by yourself
        }

        public virtual void NewMetadataTest2()
        {
            // tests discovered flaws in parser's DFA
            // also tests ability of the vault to store root's metadata

            using (var write = OpenVault("testvault"))
            {
                write.Metadata["]section"] = "[content";
                write.Save();
            }

            using (var read = OpenVault("testvault"))
            {
                Assert.AreEqual(2, read.Metadata.Count);
                Assert.AreEqual("[content", read.Metadata["]section"]);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(1, _changed.Count);
                Assert.AreEqual("[Metadata: \\ (B: 0+2/0+2V, 0/0B)] null -> []]section][[content", _changed[0].ToStringThatsFriendlyToUnitTests());
            }
        }

        public virtual void TestEmptyAndNullBeingDifferentiated()
        {
            // todo. also test behavior of non-default metadata sections, but right now i'm cba to write that test
            // ideally, we should also check 2x more cases for metadata: default+non-default, and neither of those

            // initialize the following: branch.metadata, value.metadata, value.content
            using (var write = OpenVault("testvault"))
            {
                var b_emptymetadata = write.BeforeMutate(_history).CreateBranch("b_emptymetadata");
                var b_nullmetadata = write.BeforeMutate(_history).CreateBranch("b_nullmetadata");
                var v_emptymetadata = write.BeforeMutate(_history).CreateValue("v_emptymetadata");
                var v_nullmetadata = write.BeforeMutate(_history).CreateValue("v_nullmetadata");
                var v_emptycontent = write.BeforeMutate(_history).CreateValue("v_emptycontent");
                var v_nullcontent = write.BeforeMutate(_history).CreateValue("v_nullcontent");

                b_emptymetadata.BeforeMutate(_history).SetMetadata(String.Empty);
                b_nullmetadata.BeforeMutate(_history).SetMetadata(null);
                v_emptymetadata.BeforeMutate(_history).SetMetadata(String.Empty);
                v_nullmetadata.BeforeMutate(_history).SetMetadata(null);
                v_emptycontent.BeforeMutate(_history).SetContent(String.Empty);
                v_nullcontent.BeforeMutate(_history).SetContent(null);

                write.BeforeInv(_history).Save();
            }

            // check correct strings after the save/load roundtrip
            using (var read = OpenVault("testvault"))
            {
                var b_emptymetadata = read.BeforeInv(_history).GetBranch("b_emptymetadata");
                var b_nullmetadata = read.BeforeInv(_history).GetBranch("b_nullmetadata");
                var v_emptymetadata = read.BeforeInv(_history).GetValue("v_emptymetadata");
                var v_nullmetadata = read.BeforeInv(_history).GetValue("v_nullmetadata");
                var v_emptycontent = read.BeforeInv(_history).GetValue("v_emptycontent");
                var v_nullcontent = read.BeforeInv(_history).GetValue("v_nullcontent");

                Assert.AreEqual(String.Empty, (String)b_emptymetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(null, (String)b_nullmetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(String.Empty, (String)v_emptymetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(null, (String)v_nullmetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(String.Empty, v_emptycontent.BeforeInv(_history).ContentString);
                Assert.AreEqual(null, v_nullcontent.BeforeInv(_history).ContentString);
            }

            // swap String.Empty and nulls and make sure that all these swaps generate change events
            using (var write = OpenVault("testvault"))
            {
                var b_emptymetadata = write.BeforeInv(_history).GetBranch("b_emptymetadata");
                var b_nullmetadata = write.BeforeInv(_history).GetBranch("b_nullmetadata");
                var v_emptymetadata = write.BeforeInv(_history).GetValue("v_emptymetadata");
                var v_nullmetadata = write.BeforeInv(_history).GetValue("v_nullmetadata");
                var v_emptycontent = write.BeforeInv(_history).GetValue("v_emptycontent");
                var v_nullcontent = write.BeforeInv(_history).GetValue("v_nullcontent");

                b_emptymetadata.BeforeMutate(_history).SetMetadata(null);
                b_nullmetadata.BeforeMutate(_history).SetMetadata(String.Empty);
                v_emptymetadata.BeforeMutate(_history).SetMetadata(null);
                v_nullmetadata.BeforeMutate(_history).SetMetadata(String.Empty);
                v_emptycontent.BeforeMutate(_history).SetContent(null);
                v_nullcontent.BeforeMutate(_history).SetContent(String.Empty);

                write.BeforeInv(_history).Save();
            }

            // check correct strings after the save/load roundtrip once again (x2)
            using (var read = OpenVault("testvault"))
            {
                var b_emptymetadata = read.BeforeInv(_history).GetBranch("b_emptymetadata");
                var b_nullmetadata = read.BeforeInv(_history).GetBranch("b_nullmetadata");
                var v_emptymetadata = read.BeforeInv(_history).GetValue("v_emptymetadata");
                var v_nullmetadata = read.BeforeInv(_history).GetValue("v_nullmetadata");
                var v_emptycontent = read.BeforeInv(_history).GetValue("v_emptycontent");
                var v_nullcontent = read.BeforeInv(_history).GetValue("v_nullcontent");

                Assert.AreEqual(null, (String)b_emptymetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(String.Empty, (String)b_nullmetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(null, (String)v_emptymetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(String.Empty, (String)v_nullmetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(null, v_emptycontent.BeforeInv(_history).ContentString);
                Assert.AreEqual(String.Empty, v_nullcontent.BeforeInv(_history).ContentString);
            }

            // swap String.Empty and nulls again and make sure that all these swaps generate change events
            using (var write = OpenVault("testvault"))
            {
                var b_emptymetadata = write.BeforeInv(_history).GetBranch("b_emptymetadata");
                var b_nullmetadata = write.BeforeInv(_history).GetBranch("b_nullmetadata");
                var v_emptymetadata = write.BeforeInv(_history).GetValue("v_emptymetadata");
                var v_nullmetadata = write.BeforeInv(_history).GetValue("v_nullmetadata");
                var v_emptycontent = write.BeforeInv(_history).GetValue("v_emptycontent");
                var v_nullcontent = write.BeforeInv(_history).GetValue("v_nullcontent");

                b_emptymetadata.BeforeMutate(_history).SetMetadata(String.Empty);
                b_nullmetadata.BeforeMutate(_history).SetMetadata(null);
                v_emptymetadata.BeforeMutate(_history).SetMetadata(String.Empty);
                v_nullmetadata.BeforeMutate(_history).SetMetadata(null);
                v_emptycontent.BeforeMutate(_history).SetContent(String.Empty);
                v_nullcontent.BeforeMutate(_history).SetContent(null);

                write.BeforeInv(_history).Save();
            }

            // check correct strings after the save/load roundtrip once again (x3)
            using (var read = OpenVault("testvault"))
            {
                var b_emptymetadata = read.BeforeInv(_history).GetBranch("b_emptymetadata");
                var b_nullmetadata = read.BeforeInv(_history).GetBranch("b_nullmetadata");
                var v_emptymetadata = read.BeforeInv(_history).GetValue("v_emptymetadata");
                var v_nullmetadata = read.BeforeInv(_history).GetValue("v_nullmetadata");
                var v_emptycontent = read.BeforeInv(_history).GetValue("v_emptycontent");
                var v_nullcontent = read.BeforeInv(_history).GetValue("v_nullcontent");

                Assert.AreEqual(String.Empty, (String)b_emptymetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(null, (String)b_nullmetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(String.Empty, (String)v_emptymetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(null, (String)v_nullmetadata.BeforeInv(_history).Metadata.AssertNotNull());
                Assert.AreEqual(String.Empty, v_emptycontent.BeforeInv(_history).ContentString);
                Assert.AreEqual(null, v_nullcontent.BeforeInv(_history).ContentString);
            }

            if (_trackingEvents)
            {
                Assert.AreEqual(32, _changed.Count);

                // initialization
                Assert.AreEqual("[ElementAdd: \\ (B: 4+2/4+2V, 2/2B)] null -> \\b_emptymetadata (B: 0+0/0+0V, 0/0B)", _changed[0].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\b_emptymetadata (B: 0+0/0+0V, 0/0B)] null -> \\ (B: 4+2/4+2V, 2/2B)", _changed[1].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 4+2/4+2V, 2/2B)] null -> \\b_nullmetadata (B: 0+0/0+0V, 0/0B)", _changed[2].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\b_nullmetadata (B: 0+0/0+0V, 0/0B)] null -> \\ (B: 4+2/4+2V, 2/2B)", _changed[3].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 4+2/4+2V, 2/2B)] null -> \\v_emptymetadata (V)", _changed[4].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\v_emptymetadata (V)] null -> \\ (B: 4+2/4+2V, 2/2B)", _changed[5].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_emptymetadata (V)] null -> null", _changed[6].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 4+2/4+2V, 2/2B)] null -> \\v_nullmetadata (V)", _changed[7].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\v_nullmetadata (V)] null -> \\ (B: 4+2/4+2V, 2/2B)", _changed[8].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_nullmetadata (V)] null -> null", _changed[9].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 4+2/4+2V, 2/2B)] null -> \\v_emptycontent (V)", _changed[10].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\v_emptycontent (V)] null -> \\ (B: 4+2/4+2V, 2/2B)", _changed[11].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_emptycontent (V)] null -> null", _changed[12].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[ElementAdd: \\ (B: 4+2/4+2V, 2/2B)] null -> \\v_nullcontent (V)", _changed[13].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Add: \\v_nullcontent (V)] null -> \\ (B: 4+2/4+2V, 2/2B)", _changed[14].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_nullcontent (V)] null -> null", _changed[15].ToStringThatsFriendlyToUnitTests());

                // first round
                Assert.AreEqual("[Metadata: \\b_emptymetadata (B: 0+0/0+0V, 0/0B)] null -> ", _changed[16].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\v_emptymetadata (V)] null -> ", _changed[17].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_emptycontent (V)] null -> ", _changed[18].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_nullcontent (V)] null -> null", _changed[19].ToStringThatsFriendlyToUnitTests());

                // second round
                Assert.AreEqual("[Metadata: \\b_emptymetadata (B: 0+0/0+0V, 0/0B)]  -> null", _changed[20].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\b_nullmetadata (B: 0+0/0+0V, 0/0B)] null -> ", _changed[21].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\v_emptymetadata (V)]  -> null", _changed[22].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\v_nullmetadata (V)] null -> ", _changed[23].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_emptycontent (V)]  -> null", _changed[24].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_nullcontent (V)] null -> ", _changed[25].ToStringThatsFriendlyToUnitTests());

                // third round
                Assert.AreEqual("[Metadata: \\b_emptymetadata (B: 0+0/0+0V, 0/0B)] null -> ", _changed[26].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\b_nullmetadata (B: 0+0/0+0V, 0/0B)]  -> null", _changed[27].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\v_emptymetadata (V)] null -> ", _changed[28].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Metadata: \\v_nullmetadata (V)]  -> null", _changed[29].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_emptycontent (V)] null -> ", _changed[30].ToStringThatsFriendlyToUnitTests());
                Assert.AreEqual("[Content: \\v_nullcontent (V)]  -> null", _changed[31].ToStringThatsFriendlyToUnitTests());
            }
        }
    }
}