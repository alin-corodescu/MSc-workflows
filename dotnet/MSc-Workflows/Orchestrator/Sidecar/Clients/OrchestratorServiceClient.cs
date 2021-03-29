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
        }
        
        public async Task<DataEventReply> PublishData(DataEventRequest request)
        {
            return await _client.NotifyDataAvailableAsync(request);
        }
    }
}