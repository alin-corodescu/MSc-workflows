using System;
using System.IO;
using System.Threading;
using Grpc.Net.Client;
using Workflows.Models;
using Workflows.Models.DataEvents;

namespace ManualTestingProject
{
    class Program
    {
        static void Main(string[] args)
        {

            var storageChannel = GrpcChannel.ForAddress("http://localhost:5001");
            var storageClient = new StorageAdapter.StorageAdapterClient(storageChannel);

            var orchestrationChannel = GrpcChannel.ForAddress("http://localhost:5002");
            var orchestrationClient = new OrchestratorService.OrchestratorServiceClient(orchestrationChannel);
            
            File.WriteAllText("/tmp/workflows/input.txt", "Hello World!");

            var pushResponse = storageClient.PushData(new PushDataRequest
            {
                SourceFilePath = "/tmp/workflows/input.txt"
            });

            var stream = orchestrationClient.NotifyDataAvailable();
            var dataEventRequest = new DataEventRequest
            {
                Metadata = pushResponse.GeneratedMetadata
            };
            
            stream.RequestStream.WriteAsync(dataEventRequest).Wait();

            stream.ResponseStream.MoveNext(new CancellationToken()).Wait();
            Console.WriteLine(stream.ResponseStream.Current.IsSuccess);

            stream.RequestStream.CompleteAsync().Wait();
            
            Console.ReadKey();
        }
    }
}