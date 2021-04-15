using System.Collections.Generic;
using Workflows.Models.Spec;

namespace OrchestratorService.WorkflowSpec
{
    public interface IWorkflowRegistry
    {
        public void StoreWorkflow(Workflow workflow);

        public Workflow RetrieveWorkflow();
    }
}