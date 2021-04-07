using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Jaeger.ApiV2;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
            var runInfo = new RunInfo {LastRunTime = (DateTimeOffset.UtcNow - TimeSpan.FromHours(1)).UtcTicks};

            if (File.Exists("runInfo.json"))
            {
                using var r = new StreamReader("runInfo.json");
                var json = await r.ReadToEndAsync();
                runInfo = JsonConvert.DeserializeObject<RunInfo>(json);
            }
            
            var jaegerUrl = $"http://{_configuration["JaegerUrl"]}";

            var channel = GrpcChannel.ForAddress(jaegerUrl);

            var client = new QueryService.QueryServiceClient(channel);

            var findTracesRequest = new FindTracesRequest
            {
                Query = new TraceQueryParameters
                {
                    OperationName = "ProcessDataEvent",
                    ServiceName = "Orchestrator",
                    StartTimeMin = Timestamp.FromDateTimeOffset(new DateTimeOffset(runInfo.LastRunTime, TimeSpan.Zero)),
                    StartTimeMax = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow),
                    SearchDepth = 1000
                }
            };
            // I could store the time of the last run and look for traces since then...
            // I could also keep track of all the traces ever looked up, and ignore those.
            var streamResult = client.FindTraces(findTracesRequest, cancellationToken: stoppingToken).ResponseStream;

            List<ByteString> traceIds = new List<ByteString>();
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
                , "TriggerStepClientDuration"
                , "TriggerStepServerDuration"
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
                    if (runInfo.UsedTraceIds.Contains(span.TraceId.ToBase64()))
                    {
                        Console.WriteLine("Found an old trace, ignoring");
                        continue;
                    }
                    
                    if (span.TraceId.ToBase64() != currentTraceId)
                    {
                        
                        await textWriter.FlushAsync();
                        // new trace is starting.
                        if (currentTraceDetails != null)
                        {
                            await textWriter.WriteLineAsync(currentTraceDetails.ToString());
                        }
                        currentTraceDetails = new TraceDetails();
                        currentTraceId = span.TraceId.ToBase64();
                        currentTraceDetails.TraceId = currentTraceId;
                        traceIds.Add(span.TraceId);
                    }

                    AddInfoToCurrentTraceDetails(span, currentTraceDetails);
                }
            }

            if (currentTraceDetails != null)
            {
                await textWriter.WriteLineAsync(currentTraceDetails.ToString());
            }


            foreach (var traceId in traceIds)
            {
                runInfo.UsedTraceIds.Add(traceId.ToBase64());
            }

            runInfo.LastRunTime = DateTimeOffset.UtcNow.Ticks;

            await File.WriteAllTextAsync("runInfo.json", JsonConvert.SerializeObject(runInfo));
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
                    currentTraceDetails.TriggerStepClientDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;

                    if (currentTraceDetails.DataPullType == "none")
                    {
                        currentTraceDetails.DataPullType = "local";
                    }
                }
            }

            if (span.Process.ServiceName == "Sidecar")
            {
                if (span.OperationName == "SidecarService/TriggerStep")
                {
                    
                    currentTraceDetails.TriggerStepServerDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }
                
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

                if (span.OperationName == "ComputeStepService/TriggerCompute-Single")
                {
                    // 9
                    currentTraceDetails.ComputeDuration = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }
            }

            if (span.Process.ServiceName == "DataAdapter")
            {
                if (span.OperationName == "DataMasterService/GetAddrForDataChunk")
                {
                    
                    currentTraceDetails.DataPullType = "remote";
                    currentTraceDetails.DataMasterPullCall = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "StorageAdapter/PullData")
                {
                    // Here I need to extract a few things
                    var from = span.Tags.SingleOrDefault(kv => kv.Key == "wf-from");
                    var to = span.Tags.SingleOrDefault(kv => kv.Key == "wf-to");
                    var ds = span.Tags.SingleOrDefault(kv => kv.Key == "wf-ds");
                    currentTraceDetails.FromZone = from == null ? "null" : from.VStr.Split("-")[1];  
                    currentTraceDetails.ToZone = to == null ? "null" : to.VStr.Split("-")[1];
                    currentTraceDetails.DataSize = ds?.VInt64 ?? 0;
                    
                    currentTraceDetails.DataPeerPullCall = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }

                if (span.OperationName == "DataMasterService/SignalDataChunkAvailable")
                {
                    currentTraceDetails.DataMasterPushCall = (int) span.Duration.ToTimeSpan().TotalMilliseconds;
                }
            }
        }
    }

    public class RunInfo
    {
        public HashSet<string> UsedTraceIds { get; set; } = new();
        
        /// <summary>
        /// Date time offset as UtcTicks
        /// </summary>
        public long LastRunTime { get; set; }
    }
}