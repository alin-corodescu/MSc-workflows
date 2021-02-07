using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TestGrpcService.Definitions;
using Workflows.Models;
using Workflows.Models.DataEvents;

namespace TestGrpcService
{
    public class Sidecar : ISidecar
    {
        private readonly IDataSourceAdapter _dataSource;
        private readonly IComputeStep _computeStep;
        private readonly IDataSinkAdapter _dataSink;
        private readonly IOrchestratorServiceClient _orchestrator;
        private readonly ILogger<Sidecar> _logger;

        public Sidecar(IDataSourceAdapter dataSource, IComputeStep computeStep, IDataSinkAdapter dataSink,
            IOrchestratorServiceClient orchestrator, ILogger<Sidecar> logger)
        {
            _dataSource = dataSource;
            _computeStep = computeStep;
            _dataSink = dataSink;
            _orchestrator = orchestrator;
            _logger = logger;
        }
        public Task<StepTriggerReply> TriggerStep(StepTriggerRequest request)
        {
            Task.Run(async () =>
            {
                var metadata = request.Metadata;
                var reqId = request.RequestId;
                var targetPath = $"/tmp/sandbox/inputs/{Guid.NewGuid()}";
                // 1. Ask the data source to download the data.
                _logger.LogInformation($"Downloading data from the data source. TargetPath = {targetPath}");
                var pullDataRequest = new PullDataRequest
                {
                    Metadata = metadata,
                    TargetPath = targetPath
                };
                await _dataSource.DownloadData(pullDataRequest);
                
                _logger.LogInformation("Passing the data to the compute step");

                var responses = _computeStep.TriggerCompute(new ComputeStepRequest
                {
                    LocalPath = targetPath
                });

                await foreach (var response in responses)
                {
                    _logger.LogInformation("Publishing data to the data sink");
                    // TODO these calls do not need to be awaited actually. We could parallelize the work across multiple files.
                    var reply = await _dataSink.PushData(new PushDataRequest
                    {
                        SourceFilePath = response.OutputFilePath
                    });

                    _logger.LogInformation("Publishing metadata to the orchestrator service");
                    await _orchestrator.PublishData(new DataEventRequest
                    {
                        Metadata = reply.GeneratedMetadata,
                        RequestId = reqId
                    });
                }
            });

            return Task.FromResult(new StepTriggerReply
            {
                IsSuccess = true
            });
        }
    }
}