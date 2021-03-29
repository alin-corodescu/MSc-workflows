using System;
using System.Collections;
using System.Threading.Tasks;
using Definitions.Adapters;
using Google.Protobuf;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace Workflows.StorageAdapters.Definitions
{
    /// <summary>
    /// Service responsible for communicating with the master node of the data solution.
    /// </summary>
    public interface IDataMasterClient
    {
        /// <summary>
        /// Returns the ip address of the host/ pod hosting the data identified by the guid passed as parameter
        /// </summary>
        /// <param name="fileName">The name of the file</param>
        /// <returns>The ip address of the host/ pod hosting the data</returns>
        public Task<AddressReply> GetAddressForFile(string fileName);

        /// <summary>
        /// Lets the data master know the piece of data identified by the guid passed as parameter is available
        /// on the node making the call.
        /// </summary>
        /// <param name="fileGuid">The identifier for the data chunk available on this node</param>
        /// <returns></returns>
        public Task PublishFile(string fileName);
    }

    class DataMasterClient : IDataMasterClient
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataMasterClient> _logger;
        private DataMasterService.DataMasterServiceClient _client;
        private string _addr;
        private DataLocalization _localization;

        public DataMasterClient(IConfiguration configuration, ILogger<DataMasterClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var orchestratorServiceAddr = configuration["DATAMASTER_SERVICE_HOST"];
            var port = configuration["DATAMASTER_SERVICE_PORT"];
            _addr = configuration["NODE_IP"];
            var grpcChannel = GrpcChannel.ForAddress($"http://{orchestratorServiceAddr}:{port}");

            _client = new DataMasterService.DataMasterServiceClient(grpcChannel);

            _localization = LocalFileSystemStorageAdapter.ExtractLocalization(configuration);
        }

        public async Task<AddressReply> GetAddressForFile(string name)
        {
            var addressRequest = new AddressRequest
            {
                Metadata = new LocalFileSystemMetadata
                {
                    FileName = name
                }
            };

            var result =  await _client.GetAddrForDataChunkAsync(addressRequest).ResponseAsync;

            return result;
        }

        public async Task PublishFile(string fileName)
        {
            var request = new DataChunkAvailableRequest
            {
                Metadata = new LocalFileSystemMetadata
                {
                    FileName = fileName
                },
                Address = _addr,
                Localization = _localization
            };
            
            await _client.SignalDataChunkAvailableAsync(request);
        }
    }
}