using System.Threading.Tasks;
using Workflows.Models.DataEvents;

namespace Workflows.StorageAdapters.Definitions
{
    /// <summary>
    /// Interface for the logic used to interact with specific data stores. Decoupled from the GRPC transport.
    /// </summary>
    public interface IStorageAdapter
    {
        /// <summary>
        /// Publishes a local file to the storage
        /// </summary>
        /// <param name="filePath">The path to read the contents of the files from</param>
        /// <returns>The metadata to through the workflow system an array of bytes</returns>
        public Task<MetadataEvent> PushDataToStorage(string filePath);

        /// <summary>
        /// Pulls data from the underlying storage using the metadata provided
        /// and stores it at the destination path
        /// </summary>
        /// <param name="metadata">The metadata used to lookup the data in the storage</param>
        /// <param name="destinationPath">The path at which data needs to be stored</param>
        /// <returns></returns>
        public Task PullDataFromStorage(MetadataEvent metadata, string destinationPath);
    }
}