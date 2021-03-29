using System;
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
        private readonly IOrchestrationQueue queue;

        public ExecutorService(IOrchestrationQueue queue,
            ILogger<ExecutorService> logger,
            IOrchestratorImplementation implementation)
        {
            this.queue = queue;
            _logger = logger;
            _implementation = implementation;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executor service starting up. Waiting for work.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var req = await queue.DequeueOrchestrationWork();

                // spin off a background task to do it for me. I could very well do it from the "syncrhonous" part
                // and the Jaeger traces might look better actually
                var _ = Run(async () =>
                {
                    try
                    {
                        await _implementation.ProcessDataEvent(req);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Exception happened during orchestration");
                    }
                });
            }
        }
    }
}