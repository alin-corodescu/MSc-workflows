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
        public override async Task NotifyDataAvailable(IAsyncStreamReader<DataEventRequest> requestStream, IServerStreamWriter<DataEventReply> responseStream,
            ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var req = requestStream.Current;

               var response =  await _impl.ProcessDataEvent(req);

               await responseStream.WriteAsync(response);
            }
        }
    }
}