using System.Linq;
using System.Threading.Tasks;
using Commons;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace OrchestratorService.Definitions
{
    /// <summary>
    /// Interface responsible for routing individual requests
    /// </summary>
    public interface IRequestRouter
    {
        public Task<GrpcChannel> GetGrpcChannelForRequest(string stepName, DataLocalization dataLocalization);
    }

    public class RequestRouter : IRequestRouter
    {
        private IGrpcChannelPool _grpcChannelPool;

        private IPodSelector _podSelector;

        private IClusterStateProvider _clusterStateProvider;

        private ILogger<RequestRouter> _logger;

        public RequestRouter(IGrpcChannelPool grpcChannelPool, IPodSelector podSelector, IClusterStateProvider clusterStateProvider, ILogger<RequestRouter> logger)
        {
            _grpcChannelPool = grpcChannelPool;
            _podSelector = podSelector;
            _clusterStateProvider = clusterStateProvider;
            _logger = logger;
        }

        public async Task<GrpcChannel> GetGrpcChannelForRequest(string stepName, DataLocalization dataLocalization)
        {
            _logger.LogInformation($"Routing request for {stepName}");

            var possibleSteps = await _clusterStateProvider.GetPossibleChoicesForStep(stepName);
            
            _logger.LogInformation(
                $"Possible choices are : {string.Join(",", possibleSteps.Select(x => x.Metadata.Name))}");

            var targetPod = await _podSelector.SelectBestPod(possibleSteps, dataLocalization);

            _logger.LogInformation($"The choice is {targetPod.Metadata.Name}. Retrieving or creating GRPC channel");
            
            var podAddr = $"http://{targetPod.Status.PodIP}:5000";

            return this._grpcChannelPool.GetChannelForAddress(podAddr);
        }
    }
}