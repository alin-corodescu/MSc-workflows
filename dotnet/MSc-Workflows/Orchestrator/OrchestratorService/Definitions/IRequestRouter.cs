using System.Linq;
using System.Threading.Tasks;
using Commons;
using Grpc.Net.Client;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace OrchestratorService.Definitions
{
    /// <summary>
    /// Interface responsible for routing individual requests
    /// </summary>
    public interface IRequestRouter
    {
        public Task<ChannelChoice> GetGrpcChannelForRequest(string stepName, DataLocalization dataLocalization);
    }

    public class ChannelChoice
    {
        public GrpcChannel GrpcChannel { get; set; }
        
        public V1Pod PodChoice { get; set; }
    }

    public class RequestRouter : IRequestRouter
    {
        private IGrpcChannelPool _grpcChannelPool;

        private IPodSelector _podSelector;

        private IClusterStateProvider _clusterStateProvider;

        private ILogger<RequestRouter> _logger;
        
        private readonly IConfiguration _config;

        public RequestRouter(IGrpcChannelPool grpcChannelPool,
            IPodSelector podSelector,
            IClusterStateProvider clusterStateProvider,
            ILogger<RequestRouter> logger,
            IConfiguration config)
        {
            _grpcChannelPool = grpcChannelPool;
            _podSelector = podSelector;
            _clusterStateProvider = clusterStateProvider;
            _logger = logger;
            _config = config;
        }

        public async Task<ChannelChoice> GetGrpcChannelForRequest(string stepName, DataLocalization dataLocalization)
        {
            _logger.LogInformation($"Routing request for {stepName}");

            var possibleSteps = await _clusterStateProvider.GetPossibleChoicesForStep(stepName);
            
            _logger.LogInformation(
                $"Possible choices are : {string.Join(",", possibleSteps.Select(x => x.Metadata.Name))}");

            var targetPod = await _podSelector.SelectBestPod(possibleSteps, dataLocalization);

            _logger.LogInformation($"The choice is {targetPod.Metadata.Name}. Retrieving or creating GRPC channel");
            
            var podAddr = $"http://{targetPod.Status.PodIP}:5000";

            return new ChannelChoice
            {
                GrpcChannel = this._grpcChannelPool.GetChannelForAddress(podAddr),
                PodChoice = targetPod
            };
        }
    }
}