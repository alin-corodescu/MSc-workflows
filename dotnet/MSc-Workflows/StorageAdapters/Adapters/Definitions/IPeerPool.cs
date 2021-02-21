using System.Collections.Generic;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;

namespace Workflows.StorageAdapters.Definitions
{
    /// <summary>
    /// Interface for a peer connection pool
    /// </summary>
    public interface IPeerPool
    {
        /// <summary>
        /// Gets the service for a peer found at the specified addr
        /// </summary>
        /// <param name="addr">the address at which the peer is reachable</param>
        /// <returns></returns>
        public IPeerDataNodeServiceClient GetServiceForPeer(string addr);
    }

    class PeerPool : IPeerPool
    {
        private readonly IConfiguration _configuration;
        private Dictionary<string, IPeerDataNodeServiceClient> _peerPool;

        public PeerPool(IConfiguration configuration)
        {
            _configuration = configuration;
            _peerPool = new Dictionary<string, IPeerDataNodeServiceClient>();
        }

        public IPeerDataNodeServiceClient GetServiceForPeer(string addr)
        {
            if (_peerPool.ContainsKey(addr))
            {
                return _peerPool[addr];
            }
            else
            {
                // 5001 is the standard port for the data service
                var grpcAddr = $"http://{addr}:5001";
                var grpcChannel = GrpcChannel.ForAddress(grpcAddr);
                
                var newClient = new PeerDataNodeServiceClient(grpcChannel);

                _peerPool[addr] = newClient;

                return newClient;
            }
        }
    }
}