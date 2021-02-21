using System;
using System.IO;
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
        private string _inputPath;

        public Sidecar(IDataSourceAdapter dataSource, IComputeStep computeStep, IDataSinkAdapter dataSink,
            IOrchestratorServiceClient orchestrator, ILogger<Sidecar> logger, IConfiguration configuration)
        {
            _dataSource = dataSource;
            _computeStep = computeStep;
            _dataSink = dataSink;
            _orchestrator = orchestrator;
            _logger = logger;
            this._inputPath = configuration["Sidecar:InputPath"];
        }
        public async Task<StepTriggerReply> TriggerStep(StepTriggerRequest request)
        {
            var metadata = request.Metadata;
            var reqId = request.RequestId;

            var fileName = Guid.NewGuid().ToString();
            var targetPathForDataDaeomn = $"/store/inputs/{fileName}";
            // 1. Ask the data source to download the data.
            _logger.LogInformation($"Downloading data from the data source. TargetPath = {targetPathForDataDaeomn}");
            var pullDataRequest = new PullDataRequest
            {
                Metadata = metadata,
                TargetPath = targetPathForDataDaeomn
            };
            await _dataSource.DownloadData(pullDataRequest);
            
            _logger.LogInformation("Passing the data to the compute step");

            var computeStepRequest = new ComputeStepRequest
            {
                LocalPath = $"/in/{fileName}"
            };

            _logger.LogInformation("Triggered the compute step, awaiting responses");
            await foreach (var response in _computeStep.TriggerCompute(computeStepRequest))
            {
                // TODO these calls do not need to be awaited actually. We could parallelize the work across multiple files.
                //response.OutputFilePath
                // the compute step request contains the data in a format that the compute step understood
                // /out/{filename}
                
                var fName = Path.GetFileName(response.OutputFilePath);
                _logger.LogInformation($"Publishing data to the data sink /store/outputs/{fName}");
                var reply = await _dataSink.PushData(new PushDataRequest
                {
                    SourceFilePath = $"/store/outputs/{fName}"
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
    }
}