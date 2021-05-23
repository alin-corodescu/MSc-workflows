using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            this.stepId = configuration["POD_NAME"];
        }
        
        public async IAsyncEnumerable<ComputeStepReply> TriggerCompute(ComputeStepRequest request)
        {
            await using var inputFile = File.OpenRead(request.LocalPath);
            using var binaryReader = new BinaryReader(inputFile);

            var outputFilePath = $"{this.outputPath}/Step_{Guid.NewGuid().ToString()}";
            await using var outputFile = File.OpenWrite(outputFilePath);

            await inputFile.CopyToAsync(outputFile);
            
            var reply = new ComputeStepReply
            {
                OutputFilePath = outputFilePath
            };
            
            inputFile.Close();
            outputFile.Close();
            
            yield return reply;
        }
    }
}