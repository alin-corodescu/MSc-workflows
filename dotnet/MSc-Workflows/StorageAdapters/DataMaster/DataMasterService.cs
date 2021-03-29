using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace DataMaster
{
    public class DataMasterService : Workflows.Models.DataEvents.DataMasterService.DataMasterServiceBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataMasterService> _logger;
        private readonly IDataChunkLedger _ledger;
        

        public DataMasterService(IConfiguration configuration, ILogger<DataMasterService> logger, IDataChunkLedger ledger)
        {
            _configuration = configuration;
            _logger = logger;
            _ledger = ledger;
        }
        
        public override Task<DataChunkAvailableReply> SignalDataChunkAvailable(DataChunkAvailableRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Got a request signaling data chunk is available, from the peer: {Peer}", context.Peer);

            _ledger.StoreAddressForFileName(request.Metadata.FileName, new LedgerValue
            {
                Address = request.Address,
                Localization = request.Localization
            });

            return Task.FromResult(new DataChunkAvailableReply
            {
                IsSuccess = true
            });
        }

        public override Task<AddressReply> GetAddrForDataChunk(AddressRequest request, ServerCallContext context)
        {
            var value = _ledger.GetAddressForFileName(request.Metadata.FileName);
            var reply = new AddressReply
            {
                Address = value.Address,
                Localization = value.Localization
            };

            return Task.FromResult(reply);
        }
    }
}