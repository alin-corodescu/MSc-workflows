using System.Threading.Tasks;
using Workflows.Models.DataEvents;

namespace TestGrpcService.Definitions
{
    /// <summary>
    /// Interface used by the sidecar to interact with the data sink associated with this step.
    /// </summary>
    public interface IDataSinkAdapter
    {
        Task<PushDataReply> PushData(PushDataRequest metadata);
    }
}