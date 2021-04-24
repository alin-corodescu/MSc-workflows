using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

            var currentZone = config["Zone"];
            // The format is <identifier>:<filename>
            var fileToDownload = config["RemoteFileName"];

            if (fileToDownload.StartsWith("["))
            {
                var list = JsonSerializer.Deserialize<List<string>>(fileToDownload);
                fileToDownload = list[0];
            }
            
            Console.WriteLine($"RemoteFileName = {fileToDownload}");

            var zone = fileToDownload.Split(':')[0];
            var fileName = fileToDownload.Split(':')[1];

            var connStr = GetConnStrForZone(zone, config);

            var blobServiceClient = new BlobServiceClient(connStr);
            var containerClient = blobServiceClient.GetBlobContainerClient("test");
            var blobClient = containerClient.GetBlobClient(fileName);
            
            // Download the input
            await blobClient.DownloadToAsync("input.txt");
            
            // process the input
            ProcessInput("input.txt");

            // upload the output
            var uploadName = Guid.NewGuid().ToString();
            connStr = GetConnStrForZone(currentZone, config);
            
            blobServiceClient = new BlobServiceClient(connStr);
            containerClient = blobServiceClient.GetBlobContainerClient("test");
            var uploadClient = containerClient.GetBlobClient(uploadName);
            
            using FileStream localFileStream = File.OpenRead("output.txt");
            await uploadClient.UploadAsync(localFileStream);

            // Put the output where Argo expects it.
            var outputForArgo = "/mnt/out/out.txt";
            var outputValue = $"{currentZone}:{uploadName}";
            await File.WriteAllTextAsync(outputForArgo, outputValue);
        }

        private static string GetConnStrForZone(string zone, IConfiguration config)
        {
            string connStr = "";
            if (zone == "edge1")
            {
                connStr = config["ConnStrEdge1"];
            }

            if (zone == "edge2")
            {
                connStr = config["ConnStrEdge2"];
            }

            if (zone == "cloud1")
            {
                connStr = config["ConnStrCloud1"];
            }

            return connStr;
        }

        private static void ProcessInput(string path)
        {
            using var binaryReader = new BinaryReader(File.OpenRead(path));
            using var binaryWriter = new BinaryWriter(File.OpenWrite("output.txt"));

            Random random = new Random();
            while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
            {
                var readBytes = binaryReader.ReadBytes(100 * 1024);
                
                var output = readBytes.OrderBy(x => random.Next()).ToArray();
                // var output = readBytes;
                
                binaryWriter.Write(output);
            }
        }
    }
}