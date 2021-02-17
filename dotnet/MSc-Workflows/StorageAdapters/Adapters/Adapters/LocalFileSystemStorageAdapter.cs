using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;
using Workflows.StorageAdapters.Definitions;

namespace Definitions.Adapters
{
    /// <summary>
    /// <see cref="IStorageAdapter"/> implementation for interacting with the local file system.
    /// </summary>
    public class LocalFileSystemStorageAdapter : IStorageAdapter 
    {
        private readonly ILogger<LocalFileSystemStorageAdapter> _logger;
        private readonly string _permanentStorageBasePath;

        private IDataMasterService _dataMaster;
        private IPeerPool _peerPool;

        private ISet<Guid> _localFiles; 

        public LocalFileSystemStorageAdapter(IDataMasterService dataMaster, IConfiguration configuration, ILogger<LocalFileSystemStorageAdapter> logger, IPeerPool peerPool)
        {
            this._dataMaster = dataMaster;
            _logger = logger;
            _peerPool = peerPool;
            _permanentStorageBasePath = configuration["StorageAdapter:PermStoragePath"];
            _localFiles = new HashSet<Guid>();
        }

        /// <summary>
        /// Copies the file found at the source filepath to a "permanent" storage 
        /// </summary>
        /// <param name="filePath">The source filepath</param>
        /// <returns><see cref="MetadataEvent"/> containing information about where in the "permanent" storage the file was stored</returns>
        public async Task<MetadataEvent> PushDataToStorage(string filePath)
        {
            this._logger.LogInformation($"Pushing data to storage from: {filePath}");
            var destinationFileNameGuid = Guid.NewGuid();
            
            // TODO seems hard linking works because the inodes of the underlying filesystem are shared.
            File.Copy(filePath, $"{_permanentStorageBasePath}/{destinationFileNameGuid}");
            
            this._logger.LogInformation($"The data in permanent storage is {_permanentStorageBasePath}/{destinationFileNameGuid}");
            var localFileSystemMetadata = new LocalFileSystemMetadata
            {
                FileNameGuidBytes =
                    ByteString.CopyFrom(destinationFileNameGuid.ToByteArray())
            };

            var metadata = Any.Pack(localFileSystemMetadata);
            var @event = new MetadataEvent
            {
                Metadata = metadata
            };

            // Publish the information that the data chunk is now available
            await _dataMaster.PublishFile(destinationFileNameGuid);
            return @event;
        }

        /// <summary>
        /// Copies the file from the "permanent" storage to the destination path.
        /// </summary>
        /// <param name="metadata">The metadata used to identify the </param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public async Task PullDataFromStorage(MetadataEvent metadata, string destinationPath)
        { 
            
                var localFileSystemMetadata = metadata.Metadata.Unpack<LocalFileSystemMetadata>();

                var filenameGuid = new Guid(localFileSystemMetadata.FileNameGuidBytes.ToByteArray());

                if (this._localFiles.Contains(filenameGuid))
                {
                    var permanentStoragePath = $"{_permanentStorageBasePath}/{filenameGuid}";

                    // TODO hard linking is a better option
                    File.Copy(permanentStoragePath, destinationPath);
                }
                else
                {
                    // need to get the node ip of the hosting the data so I can actually get it from there
                    var addr = await this._dataMaster.GetAddressForFile(filenameGuid);

                    // Get the service for the peer hosting the data I am interested in
                    var peer = this._peerPool.GetServiceForPeer(addr);

                    // Download the data from the peer directly to the destination path
                    await peer.DownloadDataFromPeer(filenameGuid, destinationPath);
                }
        }
    }
}