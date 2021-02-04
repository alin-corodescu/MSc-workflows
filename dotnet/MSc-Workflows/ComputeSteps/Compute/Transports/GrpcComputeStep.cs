using System.Threading.Tasks;
using DummyComputeStep.Definitions;
using Grpc.Core;
using Workflows.Models;

namespace DummyComputeStep.Service
{
    public class GrpcComputeStep : ComputeStepService.ComputeStepServiceBase
    {
        private IComputeStepImpl implementation;

        public GrpcComputeStep(IComputeStepImpl impl)
        {
            this.implementation = impl;
        }
        
        public override async Task TriggerCompute(ComputeStepRequest request, IServerStreamWriter<ComputeStepReply> responseStream, ServerCallContext context)
        {
            await foreach (var reply in implementation.TriggerCompute(request))
            {
                await responseStream.WriteAsync(reply);
            }
        }
    }
}