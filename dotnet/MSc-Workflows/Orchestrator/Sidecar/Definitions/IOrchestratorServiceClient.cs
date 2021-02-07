using System.Threading.Tasks;
using Workflows.Models;

namespace TestGrpcService.Definitions
{
    /// <summary>
    /// Interface for the sidecar to communicate with the orchestrator service
    /// </summary>
    public interface IOrchestratorServiceClient
    {
        Task<DataEventReply> PublishData(DataEventRequest request);
    }
}