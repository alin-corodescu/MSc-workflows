using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Workflows.Models;

namespace OrchestratorService.Definitions
{
    public class OrchestratorImplementation : IOrchestratorImplementation
    {
        private readonly ILogger<OrchestratorImplementation> _logger;

        private readonly IRequestRouter _requestRouter;

        public OrchestratorImplementation(ILogger<OrchestratorImplementation> logger, IRequestRouter requestRouter)
        {
            _logger = logger;
            _requestRouter = requestRouter;
        }
        public async Task<DataEventReply> ProcessDataEvent(DataEventRequest req)
        {

            if (string.IsNullOrEmpty(req.RequestId))
            {
                _logger.LogInformation("Received empty request id, means external trigger");

                var channel = await this._requestRouter.GetGrpcChannelForRequest("step1", req.Metadata.DataLocalization);

                var client = new SidecarService.SidecarServiceClient(channel);
                
                var stepTriggerRequest = new StepTriggerRequest
                {
                    Metadata = req.Metadata,
                    RequestId = "test1"
                };

                _logger.LogInformation("Triggering the first step computation");
                var result = await client.TriggerStepAsync(stepTriggerRequest);

                return new DataEventReply
                {
                    IsSuccess = result.IsSuccess
                };
            }
            if (req.RequestId == "test1")
            {
                _logger.LogInformation("Received request id == test1, meaning we need to trigger step2");
                var stepTriggerRequest = new StepTriggerRequest
                {
                    Metadata = req.Metadata,
                    RequestId = "test2"
                };

                
                var channel = await this._requestRouter.GetGrpcChannelForRequest("step2", req.Metadata.DataLocalization);

                var client = new SidecarService.SidecarServiceClient(channel);
                
                var result = await client.TriggerStepAsync(stepTriggerRequest);
            }

            return new DataEventReply
            {
                IsSuccess = true
            };
        }
    }
}