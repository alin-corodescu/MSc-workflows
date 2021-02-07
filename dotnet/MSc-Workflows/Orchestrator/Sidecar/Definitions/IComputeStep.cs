using System;
using System.Collections.Generic;
using Workflows.Models;

namespace TestGrpcService.Definitions
{
    /// <summary>
    /// Interface used by the sidecar to trigger computation on the step this is co-located with.
    /// </summary>
    public interface IComputeStep
    {
        IAsyncEnumerable<ComputeStepReply> TriggerCompute(ComputeStepRequest request);
    }
}