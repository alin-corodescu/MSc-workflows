using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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
        public IPeerDataNodeServiceClient GetPeerClient(string addr);
    }

    class PeerPool : IPeerPool
    {
        private readonly IConfiguration _configuration;
        private readonly ActivitySource _activitySource;
        private ConcurrentDictionary<string, IPeerDataNodeServiceClient> _peerPool;

        public PeerPool(IConfiguration configuration, ActivitySource activitySource)
        {
            _configuration = configuration;
            _activitySource = activitySource;
            _peerPool = new ConcurrentDictionary<string, IPeerDataNodeServiceClient>();
        }

        public IPeerDataNodeServiceClient GetPeerClient(string addr)
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
                
                var newClient = new PeerDataNodeServiceClient(grpcChannel, _activitySource);

                _peerPool[addr] = newClient;

                return newClient;
            }
        }
    }
}