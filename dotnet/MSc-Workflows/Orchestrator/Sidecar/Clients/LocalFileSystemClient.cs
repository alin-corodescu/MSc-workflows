using System.Threading.Tasks;
using Grpc.Net.Client;
using TestGrpcService.Definitions;
using Workflows.Models.DataEvents;

namespace TestGrpcService.Clients
{
    public class LocalFileSystemClient : IDataSinkAdapter, IDataSourceAdapter
    {
        private StorageAdapter.StorageAdapterClient _client;

        public LocalFileSystemClient()
        {
            var channel = GrpcChannel.ForAddress("localhost:5001");
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