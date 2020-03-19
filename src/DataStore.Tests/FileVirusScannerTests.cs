

using System.IO;
using DataStore.FileHelpers;
using NUnit.Framework;

namespace DataStore.Tests
{

    [TestFixture]
    public class FileVirusScannerTests
    {
        [Test]
        public void ScannerFindsNoThreatsInATestJpegFile()
        {
            var testDataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            var testFilePath = Path.Combine(testDataDirectory, "test1.JPG");

            var scanner = new FileVirusScannerWindowsDefender();
            var result = scanner.ScanFile(testFilePath);

            Assert.That(result.FileContainsVirus, Is.EqualTo(false));
        }
    }
}
