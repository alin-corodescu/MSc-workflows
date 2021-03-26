using System.Collections.Concurrent;
using System.Collections.Generic;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace Commons
{
    /// <summary>
    /// Class encapsulating a pool of GRPC channels
    /// </summary>
    public class GrpcChannelPool : IGrpcChannelPool
    {
        private ConcurrentDictionary<string, GrpcChannel> pool;

        private bool useConnectionPooling = true;

        public GrpcChannelPool(IConfiguration configuration)
        {
            if (configuration["UseConnectionPooling"] == "false")
            {
                useConnectionPooling = false;
            }
            
            this.pool = new ConcurrentDictionary<string, GrpcChannel>();
        }
        
        public GrpcChannel GetChannelForAddress(string addr)
        {
            if (!useConnectionPooling)
            {
                return GrpcChannel.ForAddress(addr);
            }
            
            if (pool.ContainsKey(addr))
            {
                // TODO how about failed or closed channels?
                return pool[addr];
            }

            pool[addr] = GrpcChannel.ForAddress(addr);
            return pool[addr];
        }
    }
}