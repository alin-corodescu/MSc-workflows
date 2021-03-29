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
        void QueueOrchestrationWork(DataEventRequest request, ActivityContext context);

        Task<Tuple<DataEventRequest, ActivityContext>> DequeueOrchestrationWork();
    }

    class OrchestrationQueue : IOrchestrationQueue
    {
        private ConcurrentQueue<Tuple<DataEventRequest, ActivityContext>> _events = new();

        private SemaphoreSlim _dataPresentSignal = new SemaphoreSlim(0);


        public void QueueOrchestrationWork(DataEventRequest request, ActivityContext currentContext)
        {
            _events.Enqueue(new Tuple<DataEventRequest, ActivityContext>(request, currentContext));
            _dataPresentSignal.Release();
        }

        public async Task<Tuple<DataEventRequest, ActivityContext>> DequeueOrchestrationWork()
        {
            // Wait for data to be available in the queue.
            await _dataPresentSignal.WaitAsync();
            
            _events.TryDequeue(out var ev);
            
            return ev;
        }
    }
}