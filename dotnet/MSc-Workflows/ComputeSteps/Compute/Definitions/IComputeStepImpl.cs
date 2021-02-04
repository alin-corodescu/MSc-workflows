using System.Collections.Generic;
using System.Threading.Tasks;
using Workflows.Models;

namespace DummyComputeStep.Definitions
{
    public interface IComputeStepImpl
    {
        /// <summary>
        /// This implementation is actually quite bad... it's not particularly easy to integrate
        /// steps with this kind of implementation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public IAsyncEnumerable<ComputeStepReply> TriggerCompute(ComputeStepRequest request);
    }
}