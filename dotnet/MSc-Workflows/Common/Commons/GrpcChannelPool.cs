using System.Collections.Generic;
using Grpc.Net.Client;

namespace Commons
{
    /// <summary>
    /// Class encapsulating a pool of GRPC channels
    /// </summary>
    public class GrpcChannelPool : IGrpcChannelPool
    {
        private Dictionary<string, GrpcChannel> pool;

        public GrpcChannelPool()
        {
            this.pool = new Dictionary<string, GrpcChannel>();
        }
        
        public GrpcChannel GetChannelForAddress(string addr)
        {
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