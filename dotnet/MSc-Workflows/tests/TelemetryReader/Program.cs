

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TelemetryReader
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
            await new Worker(config).ExecuteAsync(CancellationToken.None);
        }
    }
}