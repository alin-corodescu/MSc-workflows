using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Definitions.Adapters;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;
using Workflows.StorageAdapters.Definitions;

namespace Definitions.Transports
{
    public class GrpcDataInjectionService : DataInjectionService.DataInjectionServiceBase
    {
        private readonly IDataMasterClient _dataMasterClient;
        private readonly IDictionary<string, int> _localFiles;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GrpcDataInjectionService> _logger;
        private string permStoragePath;
        private DataLocalization _localization;

        public GrpcDataInjectionService(IDataMasterClient dataMasterClient,IDictionary<string, int> localFiles, IConfiguration configuration, ILogger<GrpcDataInjectionService> logger)
        {
            _dataMasterClient = dataMasterClient;
            _localFiles = localFiles;
            _configuration = configuration;
            _logger = logger;

            permStoragePath = configuration["StorageAdapter:PermStoragePath"];
            this._localization = LocalFileSystemStorageAdapter.ExtractLocalization(configuration);
        }
        
        public override async Task<DataInjectionReply> InjectData(DataInjectionRequest request, ServerCallContext context)
        {
            var results = new List<MetadataEvent>(request.Count);
            
            _logger.LogInformation("Received request to inject data");
            for (int i = 0; i < request.Count; i++)
            {
                var fileName = Guid.NewGuid().ToString();
                await using (
                    var file = File.OpenWrite($"{permStoragePath}/{fileName}"))
                {
                    await using (
                        var writer = new BinaryWriter(file))
                    {
                        // write 1 KB at a time on the file
                        for (int j = 0; j < request.ContentSize / 1024; j++)
                        {
                            var content = new byte[1024];
                            new Random().NextBytes(content);
                            
                            writer.Write(content);
                            writer.Flush();
                        }
                    }
                }
                
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