using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ArgoRunner
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

            var mode = config["Mode"];
            var dataSize = config["DataSize"];
            var iterations = int.Parse(config["Iterations"]);
            if (mode == "AllAtOnce")
            {
                var file =
                    $"/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/deploy/argo/argo-all-at-once-{dataSize}.yaml";
                for (int i = 0; i < iterations; i++)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    await RunArgo(file);
                    Console.WriteLine($"Iteration {i} took {sw.ElapsedMilliseconds} ms. Sleeping 500ms");
                    
                    Thread.Sleep(500);
                }
            }

            if (mode == "Edge")
            {
                var edge1 =
                    $"/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/deploy/argo/argo-edge1-{dataSize}.yaml";
                var edge2 =
                    $"/home/alin/projects/MSc-workflows/dotnet/MSc-Workflows/deploy/argo/argo-edge2-{dataSize}.yaml";
                for (int i = 0; i < iterations; i++)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    
                    Task e1 =  RunArgo(edge1);
                    Task e2 = RunArgo(edge2);
                    
                    await Task.WhenAll(e1, e2);
                    Console.WriteLine($"Iteration {i} took {sw.ElapsedMilliseconds} ms. Sleeping 500ms");
                    
                    Thread.Sleep(500);
                }
            }
        }
        
        public static async Task RunArgo(string pathToFile)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"argo submit -n argo --wait {pathToFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();

            await process.WaitForExitAsync();
        }
    }
}