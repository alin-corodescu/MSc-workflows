using System.Threading.Tasks;
using Grpc.Core;
using Workflows.Models.DataEvents;
using Workflows.StorageAdapters.Definitions;

namespace Definitions.Transports
{
    public class GrpcStorageAdapter : StorageAdapter.StorageAdapterBase
    {
        private IStorageAdapter implementation;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="implementation"></param>
        public GrpcStorageAdapter(IStorageAdapter implementation)
        {
            this.implementation = implementation;
        }

        public override async Task<PushDataReply> PushData(PushDataRequest request, ServerCallContext context)
        {
            var result = await this.implementation.PushDataToStorage(request.SourceFilePath);

            var reply = new PushDataReply
            {
                GeneratedMetadata = result
            };
            
            return reply;
        }

        public override async Task<PullDataReply> PullData(PullDataRequest request, ServerCallContext context)
        {
            await this.implementation.PullDataFromStorage(request.Metadata, request.TargetPath);
            return new PullDataReply
            {
                IsSuccess = true
            };
        }
    }
}