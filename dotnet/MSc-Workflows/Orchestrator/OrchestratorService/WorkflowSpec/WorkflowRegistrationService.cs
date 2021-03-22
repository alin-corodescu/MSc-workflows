using System.Threading.Tasks;
using Grpc.Core;
using Workflows.Models.Spec;

namespace OrchestratorService.WorkflowSpec
{
    public class WorkflowRegistrationService : Workflows.Models.Spec.WorkflowRegistrationService.WorkflowRegistrationServiceBase
    {
        private readonly IWorkflowRegistry _workflowRegistry;

        public WorkflowRegistrationService(IWorkflowRegistry workflowRegistry)
        {
            _workflowRegistry = workflowRegistry;
        }
        
        public override async Task<RegisterWorkflowReply> RegisterWorkflow(RegisterWorkflowRequest request, ServerCallContext context)
        {
            var wfId = this._workflowRegistry.StoreWorkflow(request.Workflow);

            return await Task.FromResult(new RegisterWorkflowReply
            {
                WorkflowId = wfId
            });
        }

        public override async Task<GetWorkflowReply> GetWorkflow(GetWorkflowRequest request, ServerCallContext context)
        {
            var wf = this._workflowRegistry.RetrieveWorkflow(request.WorkflowId);

            return await Task.FromResult(new GetWorkflowReply
            {
                Workflow = wf
            });
        }
    }
}