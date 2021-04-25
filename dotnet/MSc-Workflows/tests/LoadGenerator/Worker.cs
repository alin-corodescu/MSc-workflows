using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var numberOfZones = int.Parse(_configuration["Zones"]);
            GrpcChannel secondaryDataInjector = null;
            
            var iterations = int.Parse(_configuration["Iterations"]);

            var events = new List<MetadataEvent>();
            
            int dataSize = int.Parse(_configuration["DataSize"]);
            int dataCount = int.Parse(_configuration["DataCount"]);
            
            for (int i = 0; i < iterations; i++)
            {
                var dataInjectorChannel = GrpcChannel.ForAddress($"http://{_configuration["DataInjectorUrl"]}");
                var dataInjectionClient = new DataInjectionService.DataInjectionServiceClient(dataInjectorChannel);

                var reply = await dataInjectionClient.InjectDataAsync(new DataInjectionRequest
                {
                    Count = dataCount,
                    ContentSize = dataSize
                });
                
                events.AddRange(reply.Events);

                if (numberOfZones == 2)
                {
                    secondaryDataInjector = GrpcChannel.ForAddress($"http://{_configuration["DataInjectorUrl2"]}");
                    var secondInjector = new DataInjectionService.DataInjectionServiceClient(secondaryDataInjector);
                    var reply2 = await secondInjector.InjectDataAsync(new DataInjectionRequest
                    {
                        Count = dataCount,
                        ContentSize = dataSize
                    });

                    events.AddRange(reply2.Events);
                }
            }

            Console.WriteLine("Injected data into the cluster");
            
            for (var i = 0; i < iterations; i++)
            {
                Console.WriteLine($"Executing iteration {i}");
                
                var orchestrationChannel = GrpcChannel.ForAddress($"http://{_configuration["OrchestratorUrl"]}");
                var orchestrationClient = new OrchestratorService.OrchestratorServiceClient(orchestrationChannel);

                var sw = Stopwatch.StartNew();
                Console.WriteLine("Forwarding the events to the orchestrator");

                var tasks = events.Skip(i * dataCount * numberOfZones).Take(dataCount * numberOfZones).Select(ev =>
                        Task.Run(
                            async () =>
                            {
                                await orchestrationClient.NotifyDataAvailableAsync(new DataEventRequest
                                    {Metadata = ev, RequestId = ""});
                            }, stoppingToken))
                    .ToList();

                await Task.WhenAll(tasks);

                // wait 1 sec at least
                await Task.Delay(1000);
                // This waits for processing to be done.
                await orchestrationClient.IsThereWorkOnGoingAsync(new OngoingWorkRequest());
                
                var elapsed = sw.ElapsedMilliseconds;
                
                Console.WriteLine($"Iteration {i} took {elapsed} ms.");
            }
        }
    }
}