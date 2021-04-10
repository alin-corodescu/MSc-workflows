using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;
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
        private readonly string _addr;
        private readonly ActivitySource _activitySource;
        private readonly ILogger _logger;
        private readonly DataLocalization _currentNodeLocalization;

        public PeerDataNodeServiceClient(string addr, ActivitySource activitySource, ILogger logger, DataLocalization currentNodeLocalization)
        {
            _addr = addr;
            _activitySource = activitySource;
            _logger = logger;
            _currentNodeLocalization = currentNodeLocalization;
        }

        public async Task DownloadDataFromPeer(string remoteFileIdentifier, string targetLocalPath, DataLocalization peerLocalization)
        {
            var client = new TcpClient(_addr, 6000);
            
            var stream = client.GetStream();
            using var file = File.OpenWrite(targetLocalPath);

            var fileNameLength = BitConverter.GetBytes(remoteFileIdentifier.Length);
            await stream.WriteAsync(fileNameLength.AsMemory(0, fileNameLength.Length));

            var fnBytes = Encoding.UTF8.GetBytes(remoteFileIdentifier);
            await stream.WriteAsync(fnBytes.AsMemory(0, fnBytes.Length));
            
            await stream.CopyToAsync(file);
            
            var totalSize = (int) file.Length;
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