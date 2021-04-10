using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace StorageAdapters
{
    public class DataServerConnectionHandler : ConnectionHandler
    {
        private readonly ILogger<DataServerConnectionHandler> _logger;
        private readonly IConfiguration _configuration;

        public DataServerConnectionHandler(ILogger<DataServerConnectionHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        
        public override async Task OnConnectedAsync(ConnectionContext connection)
        {
            var inputStream = connection.Transport.Input.AsStream();

            var lengthBytes = new byte[4];
            await inputStream.ReadAsync(lengthBytes.AsMemory(0, 4));
            var len = BitConverter.ToInt32(lengthBytes, 0);
            
            _logger.LogInformation($"Received TCP request with length {len}");

            var fileNameBytes = new byte[len];
            await inputStream.ReadAsync(fileNameBytes.AsMemory(0, len));

            var fileName = Encoding.UTF8.GetString(fileNameBytes);

            var permStorage = _configuration["StorageAdapter:PermStoragePath"];

            var filePath = $"{permStorage}/{fileName}";

            var file = File.OpenRead(filePath);

            await file.CopyToAsync(connection.Transport.Output.AsStream());
        }
    }
}