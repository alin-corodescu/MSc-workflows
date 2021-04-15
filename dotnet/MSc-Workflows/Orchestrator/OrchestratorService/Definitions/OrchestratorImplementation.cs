using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchestratorService.RequestQueueing;
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
        private readonly IOrchestrationQueue _orchestrationQueue;
        private readonly IConfiguration _configuration;

        private static readonly SemaphoreSlim _workTrackingSemaphore = new SemaphoreSlim(1, 1);
        public OrchestratorImplementation(
            ILogger<OrchestratorImplementation> logger, 
            IRequestRouter requestRouter,
            IWorkflowRegistry registry,
            IWorkTracker workTracker,
            IOrchestrationQueue orchestrationQueue,
            IConfiguration configuration)
        {
            _logger = logger;
            _requestRouter = requestRouter;
            _registry = registry;
            _workTracker = workTracker;
            _orchestrationQueue = orchestrationQueue;
            _configuration = configuration;
        }
        
        public async Task<DataEventReply> ProcessDataEvent(DataEventRequest req)
        {
            var workflowDefinition = _registry.RetrieveWorkflow();
            
            int eventSourcePosition = _workTracker.GetPositionInWorkflowForRequest(req.RequestId);

            if (eventSourcePosition == workflowDefinition.Steps.Count - 1)
            {
                // todo here I should finish a span that started all the way when the data chunk was first registered.
                _logger.LogInformation("Workflow finished for 1 data chunk");
                
                await _workTrackingSemaphore.WaitAsync();
                try
                {
                    _workTracker.MarkWorkAsFinished(req.RequestId);
                }
                finally
                {
                    _workTrackingSemaphore.Release();
                }
                
                return new DataEventReply
                {
                    IsSuccess = true
                };
            }
    
            var nextStep = workflowDefinition.Steps[eventSourcePosition + 1];

            GrpcChannel channel = null;
            var stepTriggerRequest = new StepTriggerRequest
            {
                Metadata = req.Metadata,
                RequestId = Guid.NewGuid().ToString(),
                DataSink = nextStep.DataSink,
                DataSource = nextStep.DataSource
            };
            
            if (_configuration["UseDataLocality"] == "false")
            {
                await _workTrackingSemaphore.WaitAsync();
                try
                {
                    _workTracker.MarkWorkAsFinished(req.RequestId);
                }
                finally
                {
                    _workTrackingSemaphore.Release();
                }

                var url =
                    $"http://{_configuration[$"{nextStep.ComputeImage}_SERVICE_HOST"]}:{_configuration[$"{nextStep.ComputeImage}_SERVICE_PORT"]}";

                _logger.LogInformation("Not using data locality, falling back to the service: {url}", url);

                await _workTrackingSemaphore.WaitAsync();
                try
                {
                    _workTracker.MarkWorkAsStarted(stepTriggerRequest.RequestId, eventSourcePosition + 1, "noName");
                }
                finally
                {
                    _workTrackingSemaphore.Release();
                }

                channel = GrpcChannel.ForAddress(url);
            }
            else
            {
                await _workTrackingSemaphore.WaitAsync();
                try
                {
                    _workTracker.MarkWorkAsFinished(req.RequestId);

                    var channelChoice =
                        await this._requestRouter.GetGrpcChannelForRequest(nextStep.ComputeImage,
                            req.Metadata.DataLocalization);

                    // The request router will return null if there is no available pod.
                    if (channelChoice == null)
                    {
                        _logger.LogInformation("Channel choice is null. We will re-queue the request");
                        // TODO this could be a bit more informative (like canRetry=true)
                        return new DataEventReply
                        {
                            IsSuccess = false
                        };
                    }

                    channel = channelChoice.GrpcChannel;

                    _workTracker.MarkWorkAsStarted(stepTriggerRequest.RequestId, eventSourcePosition + 1,
                        channelChoice.PodChoice.Name());
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Exception for data locality stuff");
                    throw;
                }
                finally
                {
                    _workTrackingSemaphore.Release();
                }
            }

            var client = new SidecarService.SidecarServiceClient(channel);
            
            _logger.LogInformation($"Triggering the computation for a step {nextStep.ComputeImage}");

            var result = await client.TriggerStepAsync(stepTriggerRequest);

            return new DataEventReply
            {
                IsSuccess = result.IsSuccess
            };
        }

        public async Task<OngoingWorkReply> IsThereOngoingWork(OngoingWorkRequest request)
        {
            // work tracker.
            var activeWork = _workTracker.AreThereOngoingRequests();
            
            // work queue
            int pendingWork = _orchestrationQueue.Count;
            
            return await Task.FromResult(new OngoingWorkReply
            {
                WorkInFlight = activeWork,
                WorkInQueue = pendingWork
            });
        }
    }
}