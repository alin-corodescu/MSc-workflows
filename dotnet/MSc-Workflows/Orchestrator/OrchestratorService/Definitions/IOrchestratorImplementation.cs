using System.Threading.Tasks;
using Workflows.Models;

namespace OrchestratorService.Definitions
{
    public interface IOrchestratorImplementation
    {
        /// <summary>
        /// Method responsible for processing a data even coming from a sidecar or from elsewhere.
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        Task<DataEventReply> ProcessDataEvent(DataEventRequest req);
    }
}