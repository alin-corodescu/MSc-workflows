using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Net.Client;
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
        public Task DownloadDataFromPeer(string remoteFileIdentifier, string targetLocalPath);
    }

    public class PeerDataNodeServiceClient : IPeerDataNodeServiceClient
    {
        private readonly ActivitySource _activitySource;
        private DataPeerService.DataPeerServiceClient client;

        public PeerDataNodeServiceClient(GrpcChannel channel, ActivitySource activitySource)
        {
            _activitySource = activitySource;
            client = new DataPeerService.DataPeerServiceClient(channel);
        }

        public async Task DownloadDataFromPeer(string remoteFileIdentifier, string targetLocalPath)
        {
            var dataRequest = new PeerDataRequest
            {
                Identifier = new LocalFileSystemMetadata
                {
                    FileName = remoteFileIdentifier
                }
            };

            var streamedResults = client.GetData(dataRequest);

            while (await streamedResults.ResponseStream.MoveNext(new CancellationToken()))
            {
                var activity = _activitySource.StartActivity("FlushToDisk");
                activity.Start();
                var chunk = streamedResults.ResponseStream.Current;
                
                await File.WriteAllBytesAsync(targetLocalPath, chunk.Payload.ToByteArray());
                activity.Stop();
            }
        }
    }
}