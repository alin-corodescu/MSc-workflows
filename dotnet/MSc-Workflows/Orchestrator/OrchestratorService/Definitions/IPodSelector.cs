using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s.Models;
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
        public async Task<V1Pod> SelectBestPod(IEnumerable<V1Pod> possibleTargets, DataLocalization dataLocalization)
        {
            return await Task.FromResult(possibleTargets.First());
        }
    }
}