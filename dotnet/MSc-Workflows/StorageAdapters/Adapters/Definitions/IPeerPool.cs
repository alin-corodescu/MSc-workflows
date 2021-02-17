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
        public IPeerDataNodeService GetServiceForPeer(string addr);
    }
}