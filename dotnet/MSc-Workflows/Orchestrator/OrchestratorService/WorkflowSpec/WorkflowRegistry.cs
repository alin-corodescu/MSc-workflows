using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Workflows.Models.Spec;

namespace OrchestratorService.WorkflowSpec
{
    public class WorkflowRegistry : IWorkflowRegistry
    {
        
        private ConcurrentDictionary<string, Workflow> _registry = new();
        
        public string StoreWorkflow(Workflow workflow)
        {
            var workflowId = Guid.NewGuid();
            _registry[workflowId.ToString()] = workflow;

            return workflowId.ToString();
        }

        public Workflow RetrieveWorkflow(string workflowId)
        {
            return _registry[workflowId];
        }

        public IEnumerable<Workflow> GetAllWorkflows()
        {
            return _registry.Values;
        }
    }
}