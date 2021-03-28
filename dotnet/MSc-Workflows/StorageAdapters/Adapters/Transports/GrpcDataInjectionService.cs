using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Definitions.Adapters;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Workflows.Models.DataEvents;
using Workflows.StorageAdapters.Definitions;

namespace Definitions.Transports
{
    public class GrpcDataInjectionService : DataInjectionService.DataInjectionServiceBase
    {
        private readonly IDataMasterClient _dataMasterClient;
        private readonly IDictionary<string, int> _localFiles;
        private readonly IConfiguration _configuration;
        private string permStoragePath;
        private DataLocalization _localization;

        public GrpcDataInjectionService(IDataMasterClient dataMasterClient,IDictionary<string, int> localFiles, IConfiguration configuration)
        {
            _dataMasterClient = dataMasterClient;
            _localFiles = localFiles;
            _configuration = configuration;

            permStoragePath = configuration["StorageAdapter:PermStoragePath"];
            this._localization = LocalFileSystemStorageAdapter.ExtractLocalization(configuration);
        }
        
        public override async Task<DataInjectionReply> InjectData(DataInjectionRequest request, ServerCallContext context)
        {
            var results = new List<MetadataEvent>(request.Count);
            
            for (int i = 0; i < request.Count; i++)
            {
                var content = new byte[request.ContentSize];
                new Random().NextBytes(content);

                var fileName = Guid.NewGuid().ToString();
                
                await File.WriteAllBytesAsync($"{permStoragePath}/{fileName}", content);

                _localFiles[fileName] = 1;
                
                var localFileSystemMetadata = new LocalFileSystemMetadata
                {
                    FileName =  fileName
                };

                var metadata = Any.Pack(localFileSystemMetadata);
                var @event = new MetadataEvent
                {
                    Metadata = metadata,
                    DataLocalization = this._localization
                };
                await _dataMasterClient.PublishFile(fileName);

                results.Add(@event);
            }
            
            var ret = new DataInjectionReply();
            ret.Events.AddRange(results);
            return ret;
        }
    }
}