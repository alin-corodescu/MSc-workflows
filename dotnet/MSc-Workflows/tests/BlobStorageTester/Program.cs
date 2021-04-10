using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Grpc.Net.Client;
using GrpcService;
using Microsoft.Extensions.Configuration;

namespace BlobStorageTester
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            string localPath = "test.txt";
            BlobServiceClient blobServiceClient = new BlobServiceClient(config["ConnStr"]);
            var remoteFileName = config["RemoteFileName"];
            var dataSize = int.Parse(config["DataSize"]);
            
            var containerClient = blobServiceClient.GetBlobContainerClient("test");

            var blobClient = containerClient.GetBlobClient(remoteFileName);
            

            Random rand = new Random();
            
            if (config["Mode"] == "Upload")
            {
                // 100 MB of data
                var bytes = new byte[dataSize];
                rand.NextBytes(bytes);

                await File.WriteAllBytesAsync(localPath, bytes);
                // Upload the file to the 
                Stopwatch sw = Stopwatch.StartNew();
                using FileStream localFileStream = File.OpenRead("test.txt");
                // long compressedSize = CalculateCompressedSize(localFileStream);
                await blobClient.UploadAsync(localFileStream, new BlobUploadOptions
                {
                    
                    TransferOptions = new StorageTransferOptions
                    {
                        MaximumConcurrency = 1,
                        MaximumTransferSize = 10485760,
                        InitialTransferSize = 10485760
                        
                    }
                });
                Console.WriteLine($"Upload took {sw.ElapsedMilliseconds}ms");
            }

            if (config["Mode"] == "DownloadGrpc")
            {
                var channel = GrpcChannel.ForAddress(config["GrpcAddress"], new GrpcChannelOptions
                {
                    MaxReceiveMessageSize = 204857600,
                    MaxSendMessageSize = 204857600
                });

                var client = new Greeter.GreeterClient(channel);
                var sw = Stopwatch.StartNew();

                await client.SayHelloAsync(new HelloRequest());
                Console.WriteLine($"Downloading with GRPC took {sw.ElapsedMilliseconds}ms");
            }
            if (config["Mode"] == "Download")
            {
                var sw = Stopwatch.StartNew();
                var download = await blobClient.DownloadToAsync("test-downloaded.txt", transferOptions: new StorageTransferOptions
                {
                    MaximumConcurrency = 1,
                    MaximumTransferSize = 10485760,
                    InitialTransferSize = 10485760
                });
                Console.WriteLine($"Download took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static long CalculateCompressedSize(FileStream localFileStream)
        {
            using (var memStream = new MemoryStream())
            {
                using (var compressionStream = new GZipStream(memStream, CompressionLevel.Optimal))
                {
                    localFileStream.CopyTo(compressionStream);
                }

                return memStream.Length;
            }
        }
    }
}