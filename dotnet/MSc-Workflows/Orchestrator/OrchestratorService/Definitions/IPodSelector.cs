using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.Extensions.Configuration;
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
        private readonly int _maxLoad;

        public KubernetesPodSelector(ILogger<KubernetesPodSelector> logger, IConfiguration configuration,
            IProximityTable proximityTable,
            IWorkTracker workTracker)
        {
            _logger = logger;
            _configuration = configuration;
            _proximityTable = proximityTable;
            _workTracker = workTracker;
            _maxLoad = Convert.ToInt32(_configuration["MaxLoad"]);
        }
        
        public async Task<V1Pod> SelectBestPod(IEnumerable<V1Pod> possibleTargets, DataLocalization dataLocalization)
        {
            // Here I need to add stuff about the distance between different regions
            // sort the possibilities based on their proximity
            // and then based on the current load of each pod.
            if (_configuration["UseDataLocality"] == "false")
            {
                // choose a random node from the possible choices.
                // should I do round-robin instead?
                // or just based on the current load?
                
                var v1Pods = possibleTargets.ToList();
                var max = v1Pods.Count - 1;
                var idx = new Random().Next(0, max);

                return v1Pods.ElementAt(idx);
            }
            
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
                .Where(x => x.Load < _maxLoad)
                .OrderBy(arg => arg.Distance)
                .First()
                .Pod);
        }

        private DataLocalization ExtractDataLocalization(V1Pod pod)
        {
            var nodeIp = pod.Status.HostIP;
            var zone = pod.Metadata.Labels["zone"];
            var region = pod.Metadata.Labels["region"];
            
            return new DataLocalization
            {
                LocalizationCoordinates = { nodeIp, zone, region }
            };
        }
    }
}