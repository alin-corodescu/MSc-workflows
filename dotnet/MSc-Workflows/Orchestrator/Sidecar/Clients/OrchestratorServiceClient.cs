using System.Threading;
using System.Threading.Tasks;
using Commons;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestGrpcService.Definitions;
using Workflows.Models;

namespace TestGrpcService.Clients
{
    public class OrchestratorServiceClient : IOrchestratorServiceClient
    {
        private readonly ILogger<OrchestratorServiceClient> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGrpcChannelPool _channelPool;
        private OrchestratorService.OrchestratorServiceClient _client;
        private AsyncDuplexStreamingCall<DataEventRequest, DataEventReply> _stream;
        private static SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        public OrchestratorServiceClient(ILogger<OrchestratorServiceClient> logger, IConfiguration configuration, IGrpcChannelPool channelPool)
        {
            _logger = logger;
            _configuration = configuration;
            _channelPool = channelPool;

            // These are the values that kubernetes is supposed to populate
            var orchestratorServiceAddr = configuration["ORCHESTRATOR_SERVICE_HOST"];
            var port = configuration["ORCHESTRATOR_SERVICE_PORT"];
            
            logger.LogInformation($"Orchestrator service add: {orchestratorServiceAddr}:{port}");
            var channel = GrpcChannel.ForAddress($"http://{orchestratorServiceAddr}:{port}");
            _client = new OrchestratorService.OrchestratorServiceClient(channel);
            this._stream = _client.NotifyDataAvailable();
        }
        
        public async Task<DataEventReply> PublishData(DataEventRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                await this._stream.RequestStream.WriteAsync(request);

                // TODO revisit the implementation of this full duplex stream.

                // if this is supposed to be multithreaded, we are in deep trouble...
                // the sole purpose of the streaming model is to be able to send more than one item while waiting for the response on the other one.

                // await the next response in the stream
                await this._stream.ResponseStream.MoveNext();

                var result = this._stream.ResponseStream.Current;
                return result;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}