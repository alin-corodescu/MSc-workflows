using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using TestGrpcService.Definitions;
using Workflows.Models;

namespace TestGrpcService.Clients
{
    public class OrchestratorServiceClient : IOrchestratorServiceClient
    {
        private OrchestratorService.OrchestratorServiceClient _client;
        private AsyncDuplexStreamingCall<DataEventRequest, DataEventReply> _stream;

        public OrchestratorServiceClient()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5002");
            _client = new OrchestratorService.OrchestratorServiceClient(channel);
            this._stream = _client.NotifyDataAvailable();
        }
        public async Task<DataEventReply> PublishData(DataEventRequest request)
        {
            await this._stream.RequestStream.WriteAsync(request);
            
            // TODO revisit the implementation of this full duplex stream.
            
            // if this is supposed to be multithreaded, we are in deep trouble...
            
            // await the next response in the stream
            await this._stream.ResponseStream.MoveNext();
            
            return this._stream.ResponseStream.Current;
        }
    }
}