using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrchestratorService.Definitions;
using static System.Threading.Tasks.Task;

namespace OrchestratorService.RequestQueueing
{
    public class ExecutorService : BackgroundService
    {
        private readonly ILogger<ExecutorService> _logger;
        private readonly IOrchestratorImplementation _implementation;
        private readonly ActivitySource _source;
        private readonly IOrchestrationQueue queue;

        public ExecutorService(IOrchestrationQueue queue,
            ILogger<ExecutorService> logger,
            IOrchestratorImplementation implementation,
            ActivitySource source)
        {
            this.queue = queue;
            _logger = logger;
            _implementation = implementation;
            _source = source;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executor service starting up. Waiting for work.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var req = await queue.DequeueOrchestrationWork();
                
                _logger.LogInformation($"Executor spinning up background work for reqId: {req.Item1.RequestId}");

                var activity = _source.StartActivity("ProcessDataEvent",
                    ActivityKind.Consumer,
                    new ActivityContext(ActivityTraceId.CreateRandom(),
                        new ActivitySpanId(),
                        ActivityTraceFlags.Recorded));
                
                activity.Start();
                
                // spin off a background task to do it for me. I could very well do it from the "syncrhonous" part
                // and the Jaeger traces might look better actually
                var _ = Task.Run(async () =>
                {
                    try
                    {
                        var result = await _implementation.ProcessDataEvent(req.Item1);
                        if (result.IsSuccess == false)
                        {
                            // TODO should guard against infinite loops here.
                            queue.QueueOrchestrationWork(req.Item1, req.Item2);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception happened during orchestration");
                    }
                    finally
                    {
                        activity.Stop();
                    }
                });
            }
        }
    }
}