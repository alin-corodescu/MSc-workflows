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
        
        public void StoreWorkflow(Workflow workflow)
        {
            currentWorkflow = workflow;
        }

        public Workflow RetrieveWorkflow()
        {
            return currentWorkflow;
        }
    }
}