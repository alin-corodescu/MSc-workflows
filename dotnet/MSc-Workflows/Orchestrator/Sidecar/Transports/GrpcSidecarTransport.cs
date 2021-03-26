using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TestGrpcService.Definitions;
using Workflows.Models;

namespace TestGrpcService.Transports
{
    public class GrpcSidecarTransport : SidecarService.SidecarServiceBase
    {
        private readonly ILogger _logger;
        private readonly ISidecar _implementation;

        public GrpcSidecarTransport(ILogger<GrpcSidecarTransport> logger, ISidecar impl)
        {
            this._logger = logger;
            this._implementation = impl;
        }

        public override async Task<StepTriggerReply> TriggerStep(StepTriggerRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Sidecar received a trigger step notification");
            return await this._implementation.TriggerStep(request);
        }
    }
}