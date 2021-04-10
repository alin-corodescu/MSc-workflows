using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace StorageAdapters.Peers
{
    /// <summary>
    /// Implementation for the data peer service
    /// </summary>
    public class DataPeerService : Workflows.Models.DataEvents.DataPeerService.DataPeerServiceBase
    {
        private readonly ILogger<DataPeerService> _logger;
        private IConfiguration _configuration;
        private readonly ActivitySource _source;

        private string _permStorageBasePath;
        private string _addr;

        public DataPeerService(ILogger<DataPeerService> logger, IConfiguration configuration, ActivitySource source)
        {
            _logger = logger;
            _configuration = configuration;
            _source = source;
            _addr = configuration["NODE_IP"];
            _permStorageBasePath = configuration["StorageAdapter:PermStoragePath"];
        }


        public override async Task GetData(PeerDataRequest request, IServerStreamWriter<PeerDataReplyChunk> responseStream, ServerCallContext context)
        {
            _logger.LogInformation($"Serving the file at: {_permStorageBasePath}/{request.Identifier.FileName}");
            
            var dataChunkSize = int.Parse(this._configuration["DataChunkSize"]);
            var path = $"{_permStorageBasePath}/{request.Identifier.FileName}";
            await using var file = File.OpenRead(path);

            using (var compressedValue = new MemoryStream())
            {
                var comprActivity = _source.StartActivity("Compression");
                comprActivity.Start();
                using (var compressionStream = new GZipStream(compressedValue, CompressionLevel.Optimal, leaveOpen:true))
                {
                    await file.CopyToAsync(compressionStream);
                }
                comprActivity.Stop();
                
                compressedValue.Seek(0, SeekOrigin.Begin);

                var sendingActivity = _source.StartActivity("SendingChunks");
                sendingActivity.Start();
                while (compressedValue.Position != compressedValue.Length)
                {
                    var bytes = new byte[dataChunkSize];
                    var read = compressedValue.Read(bytes, 0, bytes.Length);
                    var peerDataReplyChunk = new PeerDataReplyChunk
                    {
                        Payload = ByteString.CopyFrom(bytes, 0, read)
                    };
                    
                    _logger.LogInformation($"Sending message of size: {peerDataReplyChunk.CalculateSize()}");
                
                    await responseStream.WriteAsync(peerDataReplyChunk);
                }
                sendingActivity.Stop();
            }
            
            file.Dispose();
            if (this._configuration["DeleteDataAfterUse"] == "true")
            {
                File.Delete(path);
            }
        }
    }
}