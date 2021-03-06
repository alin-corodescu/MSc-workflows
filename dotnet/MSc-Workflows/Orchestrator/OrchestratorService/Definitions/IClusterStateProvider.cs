using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace OrchestratorService.Definitions
{
    /// <summary>
    /// Interface for classes responsible with reading the cluster state
    /// </summary>
    public interface IClusterStateProvider
    {
        /// <summary>
        /// Gets the possible choices (as IPs - such a dumb unique identifier) for a given
        /// </summary>
        /// <param name="stepName"></param>
        /// <returns></returns>
        public Task<IEnumerable<V1Pod>> GetPossibleChoicesForStep(string stepName);
    }

    public class KubernetesClusterStateProvider : IClusterStateProvider
    {
        private IKubernetes k8s;

        private ConcurrentDictionary<string, V1PodList> cache = new ();

        public KubernetesClusterStateProvider(ILogger<KubernetesClusterStateProvider> logger)
        {
            k8s = new Kubernetes(KubernetesClientConfiguration.InClusterConfig());
        }
        public async Task<IEnumerable<V1Pod>> GetPossibleChoicesForStep(string stepName)
        {
            if (cache.ContainsKey(stepName))
            {
                return cache[stepName].Items;
            }
            
            var pods = await k8s.ListNamespacedPodAsync("default", 
                labelSelector:$"stepName={stepName}");
            cache[stepName] = pods;
            return pods.Items;
        }
    }
}