using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Workflows.Models;

namespace OrchestratorService.Definitions
{
    public class OrchestratorImplementation : IOrchestratorImplementation
    {
        private readonly ILogger<OrchestratorImplementation> _logger;
        private SidecarService.SidecarServiceClient firstStepClient;
        private SidecarService.SidecarServiceClient secondStepClient;

        public OrchestratorImplementation(ILogger<OrchestratorImplementation> logger)
        {
            _logger = logger;
            var firstStepChannel = GrpcChannel.ForAddress("http://localhost:5000");
            var secondStepChannel = GrpcChannel.ForAddress("http://localhost:5010");

            this.firstStepClient = new SidecarService.SidecarServiceClient(firstStepChannel);
            this.secondStepClient = new SidecarService.SidecarServiceClient(secondStepChannel);
        }
        public async Task<DataEventReply> ProcessDataEvent(DataEventRequest req)
        {

            if (string.IsNullOrEmpty(req.RequestId))
            {
                _logger.LogInformation("Received empty request id, means external trigger");
                // trigger the first step
                var stepTriggerRequest = new StepTriggerRequest
                {
                    Metadata = req.Metadata,
                    RequestId = "test1"
                };

                _logger.LogInformation("Triggering the first step computation");
                var result = await firstStepClient.TriggerStepAsync(stepTriggerRequest);

                return new DataEventReply
                {
                    IsSuccess = result.IsSuccess
                };
            }
            if (req.RequestId == "test1")
            {
                var stepTriggerRequest = new StepTriggerRequest
                {
                    Metadata = req.Metadata,
                    RequestId = "test2"
                };

                var result = await secondStepClient.TriggerStepAsync(stepTriggerRequest);
            }

            return new DataEventReply
            {
                IsSuccess = true
            };
        }
    }
}