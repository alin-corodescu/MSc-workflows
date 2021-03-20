using System;
using System.IO;
using System.Threading;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Workflows.Models;
using Workflows.Models.DataEvents;

namespace ManualTestingProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new LocalFileSystemMetadata
            {
                FileName = "test"
            };
            var evt = new MetadataEvent
            {
                Metadata = Any.Pack(test),
                DataLocalization = new DataLocalization
                {
                    Region = "region1",
                    HostIdentifier = "Host1"
                }
            };
            
            
            var orchestrationChannel = GrpcChannel.ForAddress("http://localhost:9000");
            var orchestrationClient = new OrchestratorService.OrchestratorServiceClient(orchestrationChannel);

            var localFSMetadata = new LocalFileSystemMetadata
            {
                FileName = "25bf71dc-0230-4ef9-9a4b-7b9f596ee9f2"
            };
            
            var pushResponse = new PushDataReply
            {
                GeneratedMetadata = new MetadataEvent
                {
                    Metadata = Any.Pack(localFSMetadata)
                }
            };

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