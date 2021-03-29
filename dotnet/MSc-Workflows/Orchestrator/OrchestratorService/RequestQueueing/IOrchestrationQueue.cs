using System.Collections.Concurrent;
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
        void QueueOrchestrationWork(DataEventRequest request);

        Task<DataEventRequest> DequeueOrchestrationWork();
    }

    class OrchestrationQueue : IOrchestrationQueue
    {
        private ConcurrentQueue<DataEventRequest> _events = new();

        private SemaphoreSlim _dataPresentSignal = new SemaphoreSlim(0);


        public void QueueOrchestrationWork(DataEventRequest request)
        {
            _events.Enqueue(request);
            _dataPresentSignal.Release();
        }

        public async Task<DataEventRequest> DequeueOrchestrationWork()
        {
            // Wait for data to be available in the queue.
            await _dataPresentSignal.WaitAsync();
            
            _events.TryDequeue(out var ev);
            
            return ev;
        }
    }
}