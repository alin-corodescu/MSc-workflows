using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Net.Client;
using k8s.Models;
using Microsoft.Extensions.Logging;
using OrchestratorService.WorkflowSpec;
using OrchestratorService.WorkTracking;
using Workflows.Models;
using Workflows.Models.Spec;

namespace OrchestratorService.Definitions
{
    public class OrchestratorImplementation : IOrchestratorImplementation
    {
        private readonly ILogger<OrchestratorImplementation> _logger;

        private readonly IRequestRouter _requestRouter;
        private readonly IWorkflowRegistry _registry;
        private readonly IWorkTracker _workTracker;

        public OrchestratorImplementation(
            ILogger<OrchestratorImplementation> logger, 
            IRequestRouter requestRouter,
            IWorkflowRegistry registry,
            IWorkTracker workTracker)
        {
            _logger = logger;
            _requestRouter = requestRouter;
            _registry = registry;
            _workTracker = workTracker;
        }
        
        public async Task<DataEventReply> ProcessDataEvent(DataEventRequest req)
        {
            var workflowDefinition = _registry.GetAllWorkflows().First();
            
            int eventSourcePosition = _workTracker.GetPositionInWorkflowForRequest(req.RequestId);
            
            _workTracker.MarkWorkAsFinished(req.RequestId);
            
            if (eventSourcePosition == workflowDefinition.Steps.Count - 1)
            {
                // todo here I should finish a span that started all the way when the data chunk was first registered.
                _logger.LogInformation("Workflow finished for 1 data chunk");
            }
    
            var nextStep = workflowDefinition.Steps[eventSourcePosition + 1];
            
            var channelChoice = await this._requestRouter.GetGrpcChannelForRequest(nextStep.ComputeImage, req.Metadata.DataLocalization);

            var client = new SidecarService.SidecarServiceClient(channelChoice.GrpcChannel);
            
            var stepTriggerRequest = new StepTriggerRequest
            {
                Metadata = req.Metadata,
                RequestId = Guid.NewGuid().ToString(),
                DataSink = nextStep.DataSink,
                DataSource = nextStep.DataSource
            };

            _logger.LogInformation($"Triggering the computation for a step {nextStep.ComputeImage}");
            
            _workTracker.MarkWorkAsStarted(stepTriggerRequest.RequestId, eventSourcePosition + 1, channelChoice.PodChoice.Name());
            
            var result = await client.TriggerStepAsync(stepTriggerRequest);

            return new DataEventReply
            {
                IsSuccess = result.IsSuccess
            };
        }
    }
}