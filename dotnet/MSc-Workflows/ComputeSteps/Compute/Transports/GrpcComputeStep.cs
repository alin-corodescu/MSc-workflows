using System.Threading.Tasks;
using DummyComputeStep.Definitions;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Workflows.Models;

namespace DummyComputeStep.Service
{
    public class GrpcComputeStep : ComputeStepService.ComputeStepServiceBase
    {
        private IComputeStepImpl implementation;
        private readonly ILogger<GrpcComputeStep> _logger;

        public GrpcComputeStep(IComputeStepImpl impl, ILogger<GrpcComputeStep> logger)
        {
            this.implementation = impl;
            _logger = logger;
        }
        
        public override async Task TriggerCompute(ComputeStepRequest request, IServerStreamWriter<ComputeStepReply> responseStream, ServerCallContext context)
        {
            _logger.LogInformation($"Received computation request: {request}");
            await foreach (var reply in implementation.TriggerCompute(request))
            {
                _logger.LogInformation($"Writing the reply to the response stream");
                await responseStream.WriteAsync(reply);
            }
        }
    }
}