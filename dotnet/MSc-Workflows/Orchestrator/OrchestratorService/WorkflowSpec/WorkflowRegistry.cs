using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Workflows.Models.Spec;

namespace OrchestratorService.WorkflowSpec
{
    public class WorkflowRegistry : IWorkflowRegistry
    {
        private Workflow currentWorkflow;
        
        public string StoreWorkflow(Workflow workflow)
        {
            currentWorkflow = workflow;
            return "stored";
        }

        public Workflow RetrieveWorkflow(string workflowId)
        {
            return currentWorkflow;
        }

        public IEnumerable<Workflow> GetAllWorkflows()
        {
            return new List<Workflow> {currentWorkflow};
        }
    }
}