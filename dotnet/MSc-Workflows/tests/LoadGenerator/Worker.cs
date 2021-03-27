using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Workflows.Models;
using Workflows.Models.DataEvents;

namespace LoadGenerator
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Executing the work");
            
            // create a GRPC channel to the data injector

            var dataInjectorChannel = GrpcChannel.ForAddress($"http://{_configuration["DataInjectorUrl"]}");
            var dataInjectionClient = new DataInjectionService.DataInjectionServiceClient(dataInjectorChannel);
            
            int dataSize = int.Parse(_configuration["DataSize"]);

            int dataCount = int.Parse(_configuration["DataCount"]);

            var reply = await dataInjectionClient.InjectDataAsync(new DataInjectionRequest
            {
                Count = dataCount,
                ContentSize = dataSize
            });

            _logger.LogInformation("Injected data into the cluster");
            var events = reply.Events;

            var orchestrationChannel = GrpcChannel.ForAddress($"http://{_configuration["OrchestratorUrl"]}");
            var orchestrationClient = new OrchestratorService.OrchestratorServiceClient(orchestrationChannel);

            _logger.LogInformation("Forwarding the events to the orchestrator");
            var stream = orchestrationClient.NotifyDataAvailable();
            foreach (var ev in events)
            {
                await stream.RequestStream.WriteAsync(new DataEventRequest
                {
                    Metadata = ev,
                    RequestId = ""
                });

                await stream.ResponseStream.MoveNext(CancellationToken.None);
            }

        }
    }
}