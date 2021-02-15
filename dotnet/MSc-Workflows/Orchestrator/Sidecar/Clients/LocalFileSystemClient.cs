using System.Threading.Tasks;
using Commons;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestGrpcService.Definitions;
using Workflows.Models.DataEvents;

namespace TestGrpcService.Clients
{
    public class LocalFileSystemClient : IDataSinkAdapter, IDataSourceAdapter
    {
        private readonly ILogger<LocalFileSystemClient> _logger;
        private StorageAdapter.StorageAdapterClient _client;

        public LocalFileSystemClient(ILogger<LocalFileSystemClient> logger, IConfiguration configuration,
            IGrpcChannelPool channelPool)
        {
            _logger = logger;
            var addr = configuration["NODE_IP"];
            
            _logger.LogInformation($"Connecting to the nodeip: {addr}");
            var channel = channelPool.GetChannelForAddress($"http://{addr}:5001");
            this._client = new StorageAdapter.StorageAdapterClient(channel);
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