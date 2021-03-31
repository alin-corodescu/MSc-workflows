using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Workflows.Models;

namespace OrchestratorService.RequestQueueing
{
    /// <summary>
    /// Interface for the orchestration queue. Needs to be thread safe.
    /// </summary>
    public interface IOrchestrationQueue
    {
        void QueueOrchestrationWork(DataEventRequest request, Activity existingActivity = null);

        Task<Tuple<DataEventRequest, Activity>> DequeueOrchestrationWork();
    }

    class OrchestrationQueue : IOrchestrationQueue
    {
        private readonly ActivitySource _source;
        private ConcurrentQueue<Tuple<DataEventRequest, Activity>> _events = new();

        private SemaphoreSlim _dataPresentSignal = new SemaphoreSlim(0);

        public OrchestrationQueue(ActivitySource source)
        {
            _source = source;
        }
        
        public void QueueOrchestrationWork(DataEventRequest request, Activity existingActivity = null)
        {
            if (existingActivity == null)
            {
                existingActivity = _source.StartActivity("ProcessDataEvent",
                    ActivityKind.Consumer,
                    new ActivityContext(ActivityTraceId.CreateRandom(),
                        new ActivitySpanId(),
                        ActivityTraceFlags.Recorded));
                
                existingActivity.Start();
                
            }

            _events.Enqueue(new Tuple<DataEventRequest, Activity>(request, existingActivity));
            _dataPresentSignal.Release();
        }

        public async Task<Tuple<DataEventRequest, Activity>> DequeueOrchestrationWork()
        {
            // Wait for data to be available in the queue.
            await _dataPresentSignal.WaitAsync();
            
            _events.TryDequeue(out var ev);
            
            return ev;
        }
    }
}