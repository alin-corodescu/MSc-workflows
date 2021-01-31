using System;
using System.IO;
using System.Threading.Tasks;
using Definitions.Adapters;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using NUnit.Framework;
using Workflows.Models.DataEvents;

namespace UnitTests
{
    public class LocalStorageTests
    {
        private const string inputDir = "/tmp/workflows/in";
        private const string permDir = "/tmp/workflows/perm";
        
        [SetUp]
        public void Setup()
        {
            Directory.CreateDirectory(permDir);
            Directory.CreateDirectory(inputDir);
        }

        [TearDown]
        public void TearDown()
        {
            Directory.Delete("/tmp/workflows/", recursive: true);   
        }

        [Test]
        public async Task PushData_WorksAsExpected()
        {
            var inputPath = $"{inputDir}/testFile";
            var localFileSystemStorageAdapter = new LocalFileSystemStorageAdapter(permDir);
            
            await File.WriteAllTextAsync(inputPath, "myContents");

            var metadata = await localFileSystemStorageAdapter.PushDataToStorage(inputPath);

            var receivedMetadata = metadata.Metadata.Unpack<LocalFileSystemMetadata>();

            var fileName = new Guid(receivedMetadata.FileNameGuidBytes.ToByteArray()).ToString();

            Assert.IsTrue(File.Exists($"{permDir}/{fileName}"));

            Assert.AreEqual("myContents",File.ReadAllText($"{permDir}/{fileName}"));
        }
        
        [Test]
        public async Task PullData_WorksAsExpected()
        {
            var fileNameGuid = Guid.NewGuid();
            var existingPermFile = $"{permDir}/{fileNameGuid}";
            
            var localFileSystemStorageAdapter = new LocalFileSystemStorageAdapter(permDir);
            
            await File.WriteAllTextAsync(existingPermFile, "myContents");

            var fileMetadata = new LocalFileSystemMetadata
            {
                FileNameGuidBytes = ByteString.CopyFrom(fileNameGuid.ToByteArray())
            };
            var @event = new MetadataEvent
            {
                Metadata = Any.Pack(fileMetadata)
            };

            await localFileSystemStorageAdapter.PullDataFromStorage(@event, $"{inputDir}/destination");

            Assert.IsTrue(File.Exists($"{inputDir}/destination"));

            Assert.AreEqual("myContents",File.ReadAllText($"{inputDir}/destination"));
        }
    }
}