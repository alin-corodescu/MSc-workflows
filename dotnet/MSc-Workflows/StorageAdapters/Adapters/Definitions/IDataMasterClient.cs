using System;
using System.Collections;
using System.Threading.Tasks;
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
        /// <param name="fileGuid">The guid identifying the data</param>
        /// <returns>The ip address of the host/ pod hosting the data</returns>
        public Task<string> GetAddressForFile(Guid fileGuid);

        /// <summary>
        /// Lets the data master know the piece of data identified by the guid passed as parameter is available
        /// on the node making the call.
        /// </summary>
        /// <param name="fileGuid">The identifier for the data chunk available on this node</param>
        /// <returns></returns>
        public Task PublishFile(Guid fileGuid);
    }

    class DataMasterClient : IDataMasterClient
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataMasterClient> _logger;
        private DataMasterService.DataMasterServiceClient _client;

        public DataMasterClient(IConfiguration configuration, ILogger<DataMasterClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            // take the address of the service via environment varaibles (look for the data master service)

            var grpcChannel = GrpcChannel.ForAddress("");

            _client = new DataMasterService.DataMasterServiceClient(grpcChannel);
        }

        public async Task<string> GetAddressForFile(Guid fileGuid)
        {
            var addressRequest = new AddressRequest
            {
                Metadata = new LocalFileSystemMetadata
                {
                    FileNameGuidBytes = ByteString.CopyFrom(fileGuid.ToByteArray())
                }
            };

            var result =  await _client.GetAddrForDataChunkAsync(addressRequest).ResponseAsync;

            return result.Address;
        }

        public async Task PublishFile(Guid fileGuid)
        {
            var request = new DataChunkAvailableRequest
            {
                Metadata = new LocalFileSystemMetadata
                {
                    FileNameGuidBytes = ByteString.CopyFrom(fileGuid.ToByteArray())
                }
            };
            await _client.SignalDataChunkAvailableAsync(request);
        }
    }
}