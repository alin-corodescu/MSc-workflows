using System.Diagnostics;
using System.Threading.Tasks;
using Grpc.Core;
using OrchestratorService.Definitions;
using OrchestratorService.RequestQueueing;
using Workflows.Models;

namespace OrchestratorService.Transports
{
    public class GrpcOrchestratorTransport : Workflows.Models.OrchestratorService.OrchestratorServiceBase
    {
        private IOrchestratorImplementation _impl;
        private readonly IOrchestrationQueue _workQueue;

        public GrpcOrchestratorTransport(IOrchestratorImplementation implementation, IOrchestrationQueue workQueue)
        {
            this._impl = implementation;
            _workQueue = workQueue;
        }

        public override async Task<DataEventReply> NotifyDataAvailable(DataEventRequest request, ServerCallContext context)
        {
            _workQueue.QueueOrchestrationWork(request);
            return await Task.FromResult(new DataEventReply
            {
                IsSuccess = true
            });
        }
    }
}