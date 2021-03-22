using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using k8s.Models;
using Microsoft.Extensions.Logging;
using OrchestratorService.Proximity;
using OrchestratorService.WorkTracking;
using Workflows.Models.DataEvents;

namespace OrchestratorService.Definitions
{
    /// <summary>
    /// Interface defining the pod selection logic
    /// </summary>
    public interface IPodSelector
    {
        /// <summary>
        /// Selects the best available pod based on the target data localization.
        /// </summary>
        /// <param name="possibleTargets"></param>
        /// <param name="dataLocalization"></param>
        /// <returns></returns>
        public Task<V1Pod> SelectBestPod(IEnumerable<V1Pod> possibleTargets, DataLocalization dataLocalization);
    }

    public class KubernetesPodSelector : IPodSelector
    {
        private readonly ILogger<KubernetesPodSelector> _logger;
        private readonly IConfiguration _configuration;
        private readonly IProximityTable _proximityTable;
        private readonly IWorkTracker _workTracker;

        public KubernetesPodSelector(ILogger<KubernetesPodSelector> logger, IConfiguration configuration,
            IProximityTable proximityTable,
            IWorkTracker workTracker)
        {
            _logger = logger;
            _configuration = configuration;
            _proximityTable = proximityTable;
            _workTracker = workTracker;
        }
        
        public async Task<V1Pod> SelectBestPod(IEnumerable<V1Pod> possibleTargets, DataLocalization dataLocalization)
        {
            // Here I need to add stuff about the distance between different regions
            // sort the possibilities based on their proximity
            // and then based on the current load of each pod.

            return await Task.FromResult(possibleTargets.Select(pod =>
                {
                    var podLocalization = this.ExtractDataLocalization(pod);
                    var currentLoad = _workTracker.GetCurrentLoadForPod(pod.Name());
                    // pair these two up and take a routing decision based on that.

                    var distance = _proximityTable.GetDistance(dataLocalization, podLocalization);

                    return new
                    {
                        Pod = pod,
                        Load = currentLoad,
                        Distance = distance
                    };
                })
                .Where(x => x.Load < 5)
                .OrderBy(arg => arg.Distance)
                .First()
                .Pod);
        }

        private DataLocalization ExtractDataLocalization(V1Pod pod)
        {
            // todo need to parse some labels.
            return new DataLocalization();
        }
    }
}