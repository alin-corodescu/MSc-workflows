using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GrpcService
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly IConfiguration _config;
        private byte[] content;


        public GreeterService(ILogger<GreeterService> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
            var random = new Random();
            this.content = new byte[int.Parse(config["DataSize"])];
            random.NextBytes(this.content);
        }

        public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return await Task.FromResult(new HelloReply
            {
                Message = ByteString.CopyFrom(this.content)
            });
        }
    }
}