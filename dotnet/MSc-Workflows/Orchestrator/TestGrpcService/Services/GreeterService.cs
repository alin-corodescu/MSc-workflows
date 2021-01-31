
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Workflows.Models.DataEvents;

namespace TestGrpcService
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }

    public class Testss : TestService.TestServiceBase
    {
        public override Task<HelloReply> SayTest(MetadataEvent request, ServerCallContext context)
        {
            return base.SayTest(request, context);
        }
    }
}