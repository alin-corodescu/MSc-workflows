using System.Collections.Generic;
using System.IO;
using DummyComputeStep.Definitions;
using Microsoft.Extensions.Configuration;
using Workflows.Models;

namespace DummyComputeStep.ExampleSteps
{
    public class DummyStep : IComputeStepImpl
    {
        private string outputPath;
        private string stepId;

        public DummyStep(IConfiguration configuration)
        {
            this.outputPath = configuration["ComputeStep:OutputPath"];
            this.stepId = configuration["ComputeStep:StepId"];
        }
        
        public async IAsyncEnumerable<ComputeStepReply> TriggerCompute(ComputeStepRequest request)
        {
            var input = await File.ReadAllTextAsync(request.LocalPath);

            var transformed = input + $"\n Dummy step {stepId} was here";

            var outputFilePath = $"{this.outputPath}/Step{stepId}";
            await File.WriteAllTextAsync(outputFilePath, transformed);
            var reply = new ComputeStepReply
            {
                OutputFilePath = outputFilePath
            };

            yield return reply;
        }
    }
}