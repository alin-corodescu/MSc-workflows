using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace OrchestratorService.WorkTracking
{
    public interface IWorkTracker
    {
        int GetPositionInWorkflowForRequest(string reqRequestId);

        /// <summary>
        /// Registers the work as started
        /// </summary>
        /// <param name="requestId"></param>
        /// <param name="eventSourcePosition"></param>
        /// <param name="name"></param>
        void MarkWorkAsStarted(string requestId, int eventSourcePosition, string name);

        void MarkWorkAsFinished(string reqRequestId);
        int GetCurrentLoadForPod(string name);
        
        int AreThereOngoingRequests();
    }

    class WorkTracker : IWorkTracker
    {
        private Dictionary<string, int> requestIdToPositionMapping = new();
        
        private Dictionary<string, string> requestIdToNameMapping = new();
        private ConcurrentDictionary<string, int> podNameToCurrentLoad = new();
        
        public int GetPositionInWorkflowForRequest(string reqRequestId)
        {
            if (requestIdToPositionMapping.ContainsKey(reqRequestId))
            {
                return requestIdToPositionMapping[reqRequestId];    
            }
            
            return -1;
        }

        public void MarkWorkAsStarted(string requestId, int eventSourcePosition, string name)
        {
            requestIdToPositionMapping[requestId] = eventSourcePosition;
            requestIdToNameMapping[requestId] = name;
            
            podNameToCurrentLoad.AddOrUpdate(name,
                (s) => 1,
                ((s, i) => i + 1));
        }

        public void MarkWorkAsFinished(string reqRequestId)
        {
            if (string.IsNullOrEmpty(reqRequestId))
            {
                return;
            }
            
            var name = requestIdToNameMapping[reqRequestId];
            requestIdToPositionMapping.Remove(reqRequestId);
            requestIdToNameMapping.Remove(reqRequestId);

            podNameToCurrentLoad.AddOrUpdate(name,
                (s) => 0,
                (s, i) => i - 1
            );
        }

        public int GetCurrentLoadForPod(string name)
        {
            if (!podNameToCurrentLoad.ContainsKey(name))
            {
                return 0;
            }
            
            return podNameToCurrentLoad[name];
        }

        public int AreThereOngoingRequests()
        {
            return requestIdToPositionMapping.Count;
        }
    }
}