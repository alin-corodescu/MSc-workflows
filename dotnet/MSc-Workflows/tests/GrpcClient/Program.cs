using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Compression;
using GrpcService;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5000");
            
            var client = new Greeter.GreeterClient(channel);
            using var serverStreamingCall =  client.SayHello(new HelloRequest());

            await foreach (var reply in serverStreamingCall.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(reply.Message);
            }
        }
    }
}