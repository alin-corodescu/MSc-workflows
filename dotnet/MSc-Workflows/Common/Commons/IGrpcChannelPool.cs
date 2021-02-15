using System;
using Grpc.Net.Client;

namespace Commons
{
    public interface IGrpcChannelPool
    {
        public GrpcChannel GetChannelForAddress(string addr);
    }
}