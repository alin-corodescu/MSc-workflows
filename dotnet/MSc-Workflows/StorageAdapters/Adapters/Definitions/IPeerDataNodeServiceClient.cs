using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace Workflows.StorageAdapters.Definitions
{
    /// <summary>
    /// Interface definining the communication standard between data storage peers
    /// </summary>
    public interface IPeerDataNodeServiceClient
    {
        /// <summary>
        /// Downloads the data from a peer node
        /// </summary>
        /// <param name="remoteFileIdentifier">The identifier of the data chunk on the remote node</param>
        /// <param name="targetLocalPath">The local path at which to download the data chunk</param>
        /// <returns></returns>
        public Task DownloadDataFromPeer(string remoteFileIdentifier, string targetLocalPath, DataLocalization peerLocalization);
    }

    public class PeerDataNodeServiceClient : IPeerDataNodeServiceClient
    {
        private readonly ActivitySource _activitySource;
        private readonly ILogger _logger;
        private readonly DataLocalization _currentNodeLocalization;
        private DataPeerService.DataPeerServiceClient client;

        public PeerDataNodeServiceClient(GrpcChannel channel, ActivitySource activitySource, ILogger logger, DataLocalization currentNodeLocalization)
        {
            _activitySource = activitySource;
            _logger = logger;
            _currentNodeLocalization = currentNodeLocalization;
            client = new DataPeerService.DataPeerServiceClient(channel);
        }

        public async Task DownloadDataFromPeer(string remoteFileIdentifier, string targetLocalPath, DataLocalization peerLocalization)
        {
            var dataRequest = new PeerDataRequest
            {
                Identifier = new LocalFileSystemMetadata
                {
                    FileName = remoteFileIdentifier
                }
            };

            using var file = File.OpenWrite(targetLocalPath);
            
            var streamedResults = client.GetData(dataRequest);

            var totalSize = 0;
            var chunkCount = 0;
            using (var memoryStream = new MemoryStream())
            {
                var dld = _activitySource.StartActivity("DownloadingChunks");
                dld.Start();
                await foreach (var chunk in streamedResults.ResponseStream.ReadAllAsync())
                {
                    await memoryStream.WriteAsync(chunk.Payload.Memory, CancellationToken.None);
                    chunkCount++;
                }
                dld.Stop();


                var dcp = _activitySource.StartActivity("Decompressing");
                dcp.Start();
                memoryStream.Seek(0, SeekOrigin.Begin);
                // now I have the entire compressed file in the memory stream.
                using (var decompressionStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    await decompressionStream.CopyToAsync(file);
                }
                dcp.Stop();
            }
            
            totalSize = (int) file.Length;
            string from = $"{peerLocalization.Host}-{peerLocalization.Zone}";
            string to = $"{_currentNodeLocalization.Host}-{_currentNodeLocalization.Zone}";

            var currentActivity = Activity.Current;
            currentActivity.SetTag("wf-from", from);
            currentActivity.SetTag("wf-chunk-count", chunkCount);
            currentActivity.SetTag("wf-to", to);
            currentActivity.SetTag("wf-ds", (long) totalSize);
            
            
            _logger.LogInformation("Downloaded data {from} {to} {totalSize}", from, to, totalSize);
        }
    }
}