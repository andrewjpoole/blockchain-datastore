using System;
using System.IO;
using DataStore.BlockchainDB;
using DataStore.ConfigOptions;
using FakeItEasy;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Serilog;

namespace DataStore.Tests
{
    [TestFixture]
    public class AzureBlobStorageIntegrationTests
    {
        [Test]
        public void TestUploadDownloadAndDeleteBlob()
        {
            var fakeLogger = A.Fake<ILogger>();
            var fakeAppSettings =  new NodeAppSettingsOptions
            {
                AzureBlobStoreConnectionString = "DefaultEndpointsProtocol=https;AccountName=storage4bc24aff;AccountKey=v+6FRH8H3xFOlLoHaRzLsrdeFtKRoTrSC+Y/dAoE2W48054GHwXQ4LDRgl2tWonforFjOagmuPK2Ei4E45/r9g==;EndpointSuffix=core.windows.net",
                AzureBlobContainerReference = "test-datastoreblobs"
            };
            var fakeAppSettingsOptions = Options.Create(fakeAppSettings);
            
            var sut = new AzureBlobStore(fakeLogger, fakeAppSettingsOptions);
            sut.Connect();

            var filePath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestData\\test1.jpg");
            var fileStream = File.OpenRead(filePath);
            var fakeBlobRef = $"fakeBlobRef";
            
            sut.UploadFile(fakeBlobRef, fileStream);

            Assert.That(sut.FileExists(fakeBlobRef), Is.True);

            var downloadedFileStream = (MemoryStream)sut.DownloadFile(fakeBlobRef);
            var downloadedFileBytes = downloadedFileStream.ToArray();

            Assert.That(downloadedFileBytes.Length, Is.EqualTo(3941897));

            sut.DeleteFile(fakeBlobRef);

            Assert.That(sut.FileExists(fakeBlobRef), Is.False);
        }
    }
}