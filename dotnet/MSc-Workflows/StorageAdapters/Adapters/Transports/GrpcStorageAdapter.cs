using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;
using Workflows.StorageAdapters.Definitions;

namespace Definitions.Transports
{
    public class GrpcStorageAdapter : StorageAdapter.StorageAdapterBase
    {
        private IStorageAdapter implementation;
        private readonly ILogger<GrpcStorageAdapter> _logger;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="implementation"></param>
        public GrpcStorageAdapter(IStorageAdapter implementation, ILogger<GrpcStorageAdapter> logger)
        {
            this.implementation = implementation;
            _logger = logger;
        }

        public override async Task<PushDataReply> PushData(PushDataRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Received request to push data. Forwarding to implementation. {request}");
            var result = await this.implementation.PushDataToStorage(request.SourceFilePath);

            var reply = new PushDataReply
            {
                GeneratedMetadata = result
            };
            
            return reply;
        }

        public override async Task<PullDataReply> PullData(PullDataRequest request, ServerCallContext context)
        {
            _logger.LogInformation($"Received request to pull data: {request}");
            await this.implementation.PullDataFromStorage(request.Metadata, request.TargetPath);
            return new PullDataReply
            {
                IsSuccess = true
            };
        }
    }
}