using System.Threading.Tasks;
using Grpc.Core;
using OrchestratorService.Definitions;
using Workflows.Models;

namespace OrchestratorService.Transports
{
    public class GrpcOrchestratorTransport : Workflows.Models.OrchestratorService.OrchestratorServiceBase
    {
        private IOrchestratorImplementation _impl;

        public GrpcOrchestratorTransport(IOrchestratorImplementation implementation)
        {
            this._impl = implementation;
        }

        public override async Task<DataEventReply> NotifyDataAvailable(DataEventRequest request, ServerCallContext context)
        {
            return await _impl.ProcessDataEvent(request);
        }
    }
}