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
            if (_configuration["UseLoadOrientedAlgo"] == "true")
            {
                var pt = possibleTargets.Select(pod =>
                    {
                        var podLocalization = this.ExtractDataLocalization(pod);
                        var currentLoad = _workTracker.GetCurrentLoadForPod(pod.Name());
                        // pair these two up and take a routing decision based on that.

                        var distance = _proximityTable.GetDistance(dataLocalization, podLocalization);

                        return new PossibleTarget
                        {
                            Pod = pod,
                            Load = currentLoad,
                            Distance = distance
                        };
                    })
                    .Where(x => x.Load < _maxLoad).ToList();
                
                pt.Sort((p1, p2) =>
                {
                    if (p1.Distance < p2.Distance)
                    {
                        return -1;
                    }

                    if (p1.Distance > p2.Distance)
                    {
                        return 1;
                    }

                    if (p1.Load < p2.Load)
                    {
                        return -1;
                    }

                    if (p1.Load > p2.Load)
                    {
                        return 1;
                    }

                    return 0;
                });

                if (pt.Count == 0)
                {
                    return null;
                }
                
                if (pt[0].Distance == 0)
                {
                    // The load on the next node is the same as the host
                    if (pt.Count > 1 && pt[1].Load >= pt[0].Load)
                    {
                        return pt[0].Pod;
                    }
                    if (pt.Count > 1)
                    {
                        return pt[1].Pod;
                    }

                    return null;
                }

                return pt[0].Pod;
            }


            var result = possibleTargets.Select(pod =>
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
                .FirstOrDefault();
            
            if (result != null)
            {
                return await Task.FromResult(result.Pod);
            }

            return null;
        }

        private DataLocalization ExtractDataLocalization(V1Pod pod)
        {
            var nodeIp = pod.Status.HostIP;
            var zone = pod.Metadata.Labels["zone"];
            
            return new DataLocalization
            {
                Host = nodeIp, 
                Zone = zone
            };
        }
    }

    public class PossibleTarget
    {
        public V1Pod Pod { get; set; }
        public int Load { get; set; }
        public int Distance { get; set; }
    }
    
}