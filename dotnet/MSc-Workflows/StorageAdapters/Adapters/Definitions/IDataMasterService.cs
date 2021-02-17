using System;
using System.Threading.Tasks;

namespace Workflows.StorageAdapters.Definitions
{
    /// <summary>
    /// Service responsible for communicating with the master node of the data solution.
    /// </summary>
    public interface IDataMasterService
    {
        /// <summary>
        /// Returns the ip address of the host/ pod hosting the data identified by the guid passed as parameter
        /// </summary>
        /// <param name="fileGuid">The guid identifying the data</param>
        /// <returns>The ip address of the host/ pod hosting the data</returns>
        public Task<string> GetAddressForFile(Guid fileGuid);

        /// <summary>
        /// Lets the data master know the piece of data identified by the guid passed as parameter is available
        /// on the node making the call.
        /// </summary>
        /// <param name="fileGuid">The identifier for the data chunk available on this node</param>
        /// <returns></returns>
        public Task PublishFile(Guid fileGuid);
    }
}