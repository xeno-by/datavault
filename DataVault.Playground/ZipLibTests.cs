using System;
using System.IO;
using DataVault.Core.Impl.Zip.ZipLib;
using NUnit.Framework;
using System.Linq;

namespace DataVault.Playground
{
    [TestFixture]
    public class ZipLibTests
    {
        [SetUp]
        public void SetUp()
        {
            if (File.Exists("testarc.zip")) File.Delete("testarc.zip");
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists("testarc.zip")) File.Delete("testarc.zip");
        }

        [Test]
        public void MainSuccessScenario()
        {
            var content1 = "content1 " + Guid.NewGuid();
            var content2 = "content2 " + Guid.NewGuid();

            using (var write = new ZipFile())
            {
                write.AddFileFromString("file1.txt", "", content1);
                write.AddFileFromString("file2.txt", @"Dir1\Dir2\", content2);
                write.AddDirectoryByName("Dir2/Dir3");
                write.Save("testarc.zip");
                write.Dispose();
            }

            using(var read = new ZipFile("testarc.zip"))
            {
                Assert.AreEqual(1, read.Entries.Where(e => e.IsDirectory).Count());
                Assert.AreEqual(2, read.Entries.Where(e => !e.IsDirectory).Count());

                var ms1 = new MemoryStream();
                var ms2 = new MemoryStream();
                read.Extract(@"file1.txt", ms1);
                read.Extract(@"Dir1\Dir2\file2.txt", ms2);
                ms1.Seek(0, SeekOrigin.Begin);
                ms2.Seek(0, SeekOrigin.Begin);
                read.Dispose();

                Assert.AreEqual(content1, new StreamReader(ms1).ReadToEnd());
                Assert.AreEqual(content2, new StreamReader(ms2).ReadToEnd());
            }

            using (var write2 = new ZipFile("testarc.zip"))
            {
                write2.RemoveEntry(@"file1.txt");
                write2.Save();
                write2.Dispose();
            }

            using (var read2 = new ZipFile("testarc.zip"))
            {
                Assert.AreEqual(1, read2.Entries.Where(e => !e.IsDirectory).Count());
                Assert.AreEqual(1, read2.Entries.Where(e => e.IsDirectory).Count());
            }
        }
    }
}
