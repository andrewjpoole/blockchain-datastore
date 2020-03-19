

using System.IO;
using DataStore.FileHelpers;
using NUnit.Framework;

namespace DataStore.Tests
{
    [TestFixture]
    public class FileTypeScannerTests
    {
        [Test]
        public void ScannerCorrectlyDeterminesJpegFileType()
        {
            var testDataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            var testFilePath = Path.Combine(testDataDirectory, "test1.JPG");

            var scanner = new FileTypeScannerMyrmec();
            var result = scanner.CheckFileType(testFilePath);
            
            Assert.That(result.FileType, Is.EqualTo("jpg"));
            Assert.That(result.FileTypeIsOk, Is.EqualTo(true));
        }

        [Test]
        public void ScannerCorrectlyDeterminesPdfFileType()
        {
            var testDataDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData");
            var testFilePath = Path.Combine(testDataDirectory, "test2.pdf");

            var scanner = new FileTypeScannerMyrmec();
            var result = scanner.CheckFileType(testFilePath);

            Assert.That(result.FileType, Is.EqualTo("pdf"));
            Assert.That(result.FileTypeIsOk, Is.EqualTo(true));
        }
    }
}
