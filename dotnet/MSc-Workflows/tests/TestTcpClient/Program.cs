using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestTcpClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var stopwatch = Stopwatch.StartNew();
            var client = new TcpClient("localhost", 6000);

            var stream = client.GetStream();
            var fileName = "ce67eeac-fd28-476c-b55f-645e849c970e";

            var fileNameLength = BitConverter.GetBytes(fileName.Length);
            await stream.WriteAsync(fileNameLength.AsMemory(0, fileNameLength.Length));

            var fnBytes = Encoding.UTF8.GetBytes(fileName);
            
            await stream.WriteAsync(fnBytes.AsMemory(0, fnBytes.Length));
            
            Console.WriteLine("Sent the request, waiting for the response");

            await using var file = File.OpenWrite("test.txt");
            await stream.CopyToAsync(file);
            
            Console.WriteLine($"Download took {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}