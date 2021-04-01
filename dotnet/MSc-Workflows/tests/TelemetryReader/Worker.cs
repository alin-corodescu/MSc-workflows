using System;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Jaeger.ApiV2;
using Microsoft.Extensions.Configuration;

namespace TelemetryReader
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
            Console.WriteLine("Executing the work");

            var jaegerUrl = $"http://{_configuration["JaegerUrl"]}";

            var channel = GrpcChannel.ForAddress(jaegerUrl);

            var client = new QueryService.QueryServiceClient(channel);

            
            var findTracesRequest = new FindTracesRequest
            {
                Query = new TraceQueryParameters
                {
                    OperationName = "ProcessDataEvent",
                    ServiceName = "Orchestrator",
                    StartTimeMin = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow - TimeSpan.FromDays(1)),
                    StartTimeMax = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                    // this is not actually depth, but rather a TopN parameter
                    SearchDepth = 2
                }
            };
            var streamResult = client.FindTraces(findTracesRequest, cancellationToken: stoppingToken).ResponseStream;

            while (await streamResult.MoveNext(CancellationToken.None))
            {
                Console.WriteLine("Starting new chunk");
                var currentChunk = streamResult.Current;

                // I need to identify the numbers on my figure.
                // And then, for each trace, calculate the aggregates
                foreach (var span in currentChunk.Spans)
                {
                    Console.WriteLine(span.TraceId.ToBase64());
                    Console.WriteLine(span.Process.ServiceName + ":" + span.OperationName);
                    //span.Duration.ToTimeSpan().Ticks;
                }
            }
            
            // Top Level breakdown: All Up, Local Data, Remote data
            // per zone-zone combination for : b/w and latency.
            
            // trace id, 
            // from 
            // to 
            // dataSize
            // type = local/ remote
            // duration of ProcessDataEvent. (control 2).
            // duration of TriggerStep client span. (control 1)
            // duration of data pulling
            // duration of data pushing
            // duration of compute
            
            // --- local communication pull:
            // copy vs hard linking
            // --- remote communication pull:
            // data master, data peer, flushing to disk
            
            // --- pushing
            // copy vs hard linking
            
        }
    }
}