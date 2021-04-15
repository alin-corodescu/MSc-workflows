using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Workflows.Models;

namespace TestGrpcService.Definitions
{
    /// <summary>
    /// Interface for the functionality that needs to be implemented by the sidecar
    /// </summary>
    public interface ISidecar
    {
        /// <summary>
        /// Method containing the logic to be used whenever the sidecar needs to trigger some processing
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<Empty> TriggerStep(StepTriggerRequest request);
    }
}