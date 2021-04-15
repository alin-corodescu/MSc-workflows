using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
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
        
        public override async Task<Empty> RegisterWorkflow(RegisterWorkflowRequest request, ServerCallContext context)
        {
            this._workflowRegistry.StoreWorkflow(request.Workflow);

            return await Task.FromResult(new Empty());
        }

        public override async Task<GetWorkflowReply> GetWorkflow(Empty request, ServerCallContext context)
        {
            var wf = this._workflowRegistry.RetrieveWorkflow();

            return await Task.FromResult(new GetWorkflowReply
            {
                Workflow = wf
            });
        }
    }
}