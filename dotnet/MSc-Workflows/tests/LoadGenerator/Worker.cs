using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Workflows.Models;
using Workflows.Models.DataEvents;

namespace LoadGenerator
{
    public class Worker
    {
        private readonly IConfiguration _configuration;

        public Worker(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var iterations = int.Parse(_configuration["Iterations"]);
            
            var dataInjectorChannel = GrpcChannel.ForAddress($"http://{_configuration["DataInjectorUrl"]}");
            var dataInjectionClient = new DataInjectionService.DataInjectionServiceClient(dataInjectorChannel);
            int dataSize = int.Parse(_configuration["DataSize"]);
            int dataCount = int.Parse(_configuration["DataCount"]);
            var reply = await dataInjectionClient.InjectDataAsync(new DataInjectionRequest
            {
                Count = iterations * dataCount,
                ContentSize = dataSize
            });
            Console.WriteLine("Injected data into the cluster");
            var events = reply.Events;

            for (var i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Executing iteration {i}");

                
                var orchestrationChannel = GrpcChannel.ForAddress($"http://{_configuration["OrchestratorUrl"]}");
                var orchestrationClient = new OrchestratorService.OrchestratorServiceClient(orchestrationChannel);
                
                Console.WriteLine("Forwarding the events to the orchestrator");

                var tasks = events.Skip(i * dataCount).Take(dataCount).Select(ev =>
                        Task.Run(
                            async () =>
                            {
                                await orchestrationClient.NotifyDataAvailableAsync(new DataEventRequest
                                    {Metadata = ev, RequestId = ""});
                            }, stoppingToken))
                    .ToList();

                await Task.WhenAll(tasks);
                
                Console.WriteLine($"Sleeping {_configuration["SleepTime"]} ms");

                await Task.Delay(int.Parse(_configuration["SleepTime"]));
            }
        }
    }
}