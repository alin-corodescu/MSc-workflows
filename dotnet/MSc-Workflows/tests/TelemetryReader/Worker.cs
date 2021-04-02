using System;
using System.IO;
using System.Linq;
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

            var currentTraceId = "";
            TraceDetails currentTraceDetails = null;
            using var fileName = File.OpenWrite(_configuration["outputPath"]);
            using var textWriter = new StreamWriter(fileName);

            var headers = string.Join(',', "tId"
                , "dataPullType"
                , "FromZone"
                , "ToZone"
                , "DataSize"
                , "TotalDuration"
                , "TriggerStepDuration"
                , "DataPullDuration"
                , "DataPushDuration"
                , "ComputeDuration"
                , "DataMasterPullCall"
                , "DataPeerPullCall"
                , "DataMasterPushCall");
            
            await textWriter.WriteLineAsync(headers);
            while (await streamResult.MoveNext(CancellationToken.None))
            {
                Console.WriteLine("Starting new chunk");
                var currentChunk = streamResult.Current;

                // I need to identify the numbers on my figure.
                // And then, for each trace, calculate the aggregates
                foreach (var span in currentChunk.Spans)
                {
                    if (span.TraceId.ToBase64() != currentTraceId)
                    {
                        await textWriter.FlushAsync();
                        // new trace is starting.
                        if (currentTraceDetails != null)
                        {
                            await textWriter.WriteLineAsync(currentTraceDetails.ToString());
                        }
                        currentTraceId = span.TraceId.ToBase64();
                        currentTraceDetails.TraceId = currentTraceId;
                    }

                    AddInfoToCurrentTraceDetails(span, currentTraceDetails);
                }
            }
        }

        private static void AddInfoToCurrentTraceDetails(Span span, TraceDetails currentTraceDetails)
        {
            if (span.Process.ServiceName == "Orchestrator")
            {
                if (span.OperationName == "ProcessDataEvent")
                {
                    // set the total duration
                    currentTraceDetails.TotalDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "SidecarService/TriggerStep")
                {
                    // Trigger step duration (excludes the call to the Kubernetes API.
                    currentTraceDetails.TriggerStepDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;

                    if (currentTraceDetails.DataPullType == "none")
                    {
                        currentTraceDetails.DataPullType = "local";
                    }
                }
            }

            if (span.Process.ServiceName == "Sidecar")
            {
                if (span.OperationName == "StorageAdapter/PullData")
                {
                    // 4
                    currentTraceDetails.DataPullDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "StorageAdapter/PushData")
                {
                    // 11
                    currentTraceDetails.DataPushDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "ComputeStepService/TriggerCompute")
                {
                    // 9
                    currentTraceDetails.ComputeDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }
            }

            if (span.Process.ServiceName == "DataAdapter")
            {
                if (span.OperationName == "DataMasterService/GetAddrForDataChunk")
                {
                    currentTraceDetails.DataMasterPullCall = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "DataPeerService/GetData")
                {
                    // Here I need to extract a few things
                    currentTraceDetails.DataPullType = "remote";
                    currentTraceDetails.FromZone = span.Tags.Single(kv => kv.Key == "wf-from").VStr;
                    currentTraceDetails.ToZone = span.Tags.Single(kv => kv.Key == "wf-to").VStr;
                    currentTraceDetails.DataSize = span.Tags.Single(kv => kv.Key == "wf-ds").VInt64;
                    currentTraceDetails.DataPeerPullCall = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "DataMasterService/SignalDataChunkAvailable")
                {
                    currentTraceDetails.DataMasterPushCall = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }
            }
        }
    }
}