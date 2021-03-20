using System.Collections.Generic;
using Workflows.Models.Spec;

namespace OrchestratorService.WorkflowSpec
{
    public interface IWorkflowRegistry
    {
        public string StoreWorkflow(Workflow workflow);

        public Workflow RetrieveWorkflow(string workflowId);
        
        IEnumerable<Workflow> GetAllWorkflows();
    }
}