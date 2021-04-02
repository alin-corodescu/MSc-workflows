using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
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
            using var binaryWriter = new BinaryWriter(file);
            
            var streamedResults = client.GetData(dataRequest);

            var totalSize = 0;
            while (await streamedResults.ResponseStream.MoveNext(new CancellationToken()))
            {
                // var activity = _activitySource.StartActivity("FlushToDisk");
                // activity.Start();
                var chunk = streamedResults.ResponseStream.Current;

                totalSize += chunk.Payload.Length;
                
                binaryWriter.Write(chunk.Payload.ToByteArray());
                binaryWriter.Flush();
                
                // activity.Stop();
            }

            string from = $"{peerLocalization.Host}-{peerLocalization.Zone}";
            string to = $"{_currentNodeLocalization.Host}-{_currentNodeLocalization.Zone}";

            var currentActivity = Activity.Current;
            currentActivity.SetTag("wf-from", from);
            currentActivity.SetTag("wf-to", to);
            currentActivity.SetTag("wf-ds", (long) totalSize);
            
            
            _logger.LogInformation("Downloaded data {from} {to} {totalSize}", from, to, totalSize);
        }
    }
}