using System.Threading.Tasks;
using Commons;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Cmp;
using TestGrpcService.Definitions;
using Workflows.Models.DataEvents;

namespace TestGrpcService.Clients
{
    public class LocalFileSystemClient : IDataSinkAdapter, IDataSourceAdapter
    {
        private readonly ILogger<LocalFileSystemClient> _logger;
        private StorageAdapter.StorageAdapterClient _client;

        public LocalFileSystemClient(ILogger<LocalFileSystemClient> logger, IConfiguration configuration,
            IGrpcChannelPool channelPool, int port = 5001)
        {
            _logger = logger;
            var addr = configuration["NODE_IP"];
            
            _logger.LogInformation($"Connecting to the nodeip: {addr}");
            var channel = channelPool.GetChannelForAddress($"http://{addr}:{port}");
            
            this._client = new StorageAdapter.StorageAdapterClient(channel);
        }

        public static LocalFileSystemClient CreateClientForOtherPort(IConfiguration configuration, IGrpcChannelPool channelPool, int port)
        {
            var addr = configuration["NODE_IP"];
            
            var channel = channelPool.GetChannelForAddress($"http://{addr}:{port}");

            return new LocalFileSystemClient(new StorageAdapter.StorageAdapterClient(channel));
        }

        private LocalFileSystemClient(StorageAdapter.StorageAdapterClient client)
        {
            this._client = client;
        }

        public async Task<PushDataReply> PushData(PushDataRequest metadata)
        {
            return await Task.Run(() => this._client.PushData(metadata));
        }

        public async Task<PullDataReply> DownloadData(PullDataRequest metadata)
        {
            return await Task.Run(() => this._client.PullData(metadata));
        }
    }
}