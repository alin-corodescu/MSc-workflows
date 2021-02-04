using System.Collections.Generic;
using System.IO;
using DummyComputeStep.Definitions;
using Workflows.Models;

namespace DummyComputeStep.ExampleSteps
{
    public class DummyStep : IComputeStepImpl
    {
        public async IAsyncEnumerable<ComputeStepReply> TriggerCompute(ComputeStepRequest request)
        {
            var input = await File.ReadAllTextAsync(request.LocalPath);

            var transformed = input + "\n Dummy step was here";

            var outPath = "/out/result";
            
            await File.WriteAllTextAsync(outPath, transformed);
            var reply = new ComputeStepReply
            {
                OutputFilePath = outPath
            };

            yield return reply;
        }
    }
}