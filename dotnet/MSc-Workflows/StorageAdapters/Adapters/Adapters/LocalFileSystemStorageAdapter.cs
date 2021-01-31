using System;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Workflows.Models.DataEvents;
using Workflows.StorageAdapters.Definitions;

namespace Definitions.Adapters
{
    /// <summary>
    /// <see cref="IStorageAdapter"/> implementation for interacting with the local file system.
    /// </summary>
    public class LocalFileSystemStorageAdapter : IStorageAdapter 
    {
        private readonly string _permanentStorageBasePath;

        public LocalFileSystemStorageAdapter(string permanentStorageBasePath)
        {
            _permanentStorageBasePath = permanentStorageBasePath;
        }

        /// <summary>
        /// Copies the file found at the source filepath to a "permanent" storage 
        /// </summary>
        /// <param name="filePath">The source filepath</param>
        /// <returns><see cref="MetadataEvent"/> containing information about where in the "permanent" storage the file was stored</returns>
        public async Task<MetadataEvent> PushDataToStorage(string filePath)
        {
            return await Task.Run(() =>
            {
                var destinationFileNameGuid = Guid.NewGuid();
                // Copy the file from filePath to some permanent storage
                File.Copy(filePath, $"{_permanentStorageBasePath}/{destinationFileNameGuid}");

                var test = new LocalFileSystemMetadata
                {
                    FileNameGuidBytes =
                        ByteString.CopyFrom(destinationFileNameGuid.ToByteArray())
                };

                var metadata = Any.Pack(test);
                var @event = new MetadataEvent
                {
                    Metadata = metadata
                };

                return @event;
            });
        }

        /// <summary>
        /// Copies the file from the "permanent" storage to the destination path.
        /// </summary>
        /// <param name="metadata">The metadata used to identify the </param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public Task PullDataFromStorage(MetadataEvent metadata, string destinationPath)
        {
            return Task.Run(() =>
            {
                var localFileSystemMetadata = metadata.Metadata.Unpack<LocalFileSystemMetadata>();

                var filenameGuid = new Guid(localFileSystemMetadata.FileNameGuidBytes.ToByteArray());

                var permanentStoragePath = $"{_permanentStorageBasePath}/{filenameGuid}";

                File.Copy(permanentStoragePath, destinationPath);
            });
        }
    }
}