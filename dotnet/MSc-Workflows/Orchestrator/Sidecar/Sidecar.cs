using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        private readonly ActivitySource _source;
        private string _inputPath;
        private static readonly SemaphoreSlim _parallelCallSemaphore = new SemaphoreSlim(3, 3);

        public Sidecar(IDataSourceAdapter dataSource, IComputeStep computeStep, IDataSinkAdapter dataSink,
            IOrchestratorServiceClient orchestrator, ILogger<Sidecar> logger, IConfiguration configuration,
            ActivitySource source)
        {
            _dataSource = dataSource;
            _computeStep = computeStep;
            _dataSink = dataSink;
            _orchestrator = orchestrator;
            _logger = logger;
            _source = source;
            this._inputPath = configuration["Sidecar:InputPath"];
        }
        
        public async Task<StepTriggerReply> TriggerStep(StepTriggerRequest request)
        {
            // Make sure we are not processing more than 3 requests at a time
            await _parallelCallSemaphore.WaitAsync();
            try
            {
                var metadata = request.Metadata;
                var reqId = request.RequestId;

                var fileName = Guid.NewGuid().ToString();
                var targetPathForDataDaeomn = $"/store/inputs/{fileName}";

                // 1. Ask the data source to download the data.
                _logger.LogInformation(
                    $"Downloading data from the data source. TargetPath = {targetPathForDataDaeomn}");
                var pullDataRequest = new PullDataRequest
                {
                    Metadata = metadata,
                    TargetPath = targetPathForDataDaeomn
                };

                // TODO select data source and data sink based on the parameters.
                await _dataSource.DownloadData(pullDataRequest);

                _logger.LogInformation("Passing the data to the compute step");

                var computeStepRequest = new ComputeStepRequest
                {
                    LocalPath = $"/in/{fileName}"
                };

                _logger.LogInformation("Triggered the compute step, awaiting responses");
                var activity = _source.StartActivity("ComputeStepService/TriggerCompute-Single");
                await foreach (var response in _computeStep.TriggerCompute(computeStepRequest))
                {
                    var fName = Path.GetFileName(response.OutputFilePath);
                    
                    _logger.LogInformation($"Publishing data to the data sink /store/outputs/{fName}");
                    
                    // stop the activity cause we got the response and set it to null.
                    activity?.Stop();
                    activity = null;
                    
                    var reply = await _dataSink.PushData(new PushDataRequest
                    {
                        SourceFilePath = $"/store/outputs/{fName}",
                        // Delete the input file.
                        DeletePath = targetPathForDataDaeomn
                    });

                    _logger.LogInformation("Publishing metadata to the orchestrator service");
                    await _orchestrator.PublishData(new DataEventRequest
                    {
                        Metadata = reply.GeneratedMetadata,
                        RequestId = reqId
                    });
                }

                return new StepTriggerReply
                {
                    IsSuccess = true
                };
            }
            finally
            {
                // indicate 1 thread finished.
                _parallelCallSemaphore.Release();
            }
        }
    }
}