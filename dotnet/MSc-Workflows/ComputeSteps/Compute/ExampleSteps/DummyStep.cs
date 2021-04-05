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
            await using var binaryWriter = new BinaryWriter(outputFile);
            
            while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                var readBytes = binaryReader.ReadBytes(100 * 1024);
                
                // var output = readBytes.OrderBy(x => random.Next()).ToArray();
                var output = readBytes;
                
                binaryWriter.Write(output);
            }

            var reply = new ComputeStepReply
            {
                OutputFilePath = outputFilePath
            };

            yield return reply;
        }
    }
}