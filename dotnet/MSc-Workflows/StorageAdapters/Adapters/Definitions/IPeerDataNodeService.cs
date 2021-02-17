using System;
using System.Threading.Tasks;

namespace Workflows.StorageAdapters.Definitions
{
    /// <summary>
    /// Interface definining the communication standard between data storage peers
    /// </summary>
    public interface IPeerDataNodeService
    {
        /// <summary>
        /// Downloads the data from a peer node
        /// </summary>
        /// <param name="remoteFileIdentifier">The identifier of the data chunk on the remote node</param>
        /// <param name="targetLocalPath">The local path at which to download the data chunk</param>
        /// <returns></returns>
        public Task DownloadDataFromPeer(Guid remoteFileIdentifier, string targetLocalPath);
    }
}