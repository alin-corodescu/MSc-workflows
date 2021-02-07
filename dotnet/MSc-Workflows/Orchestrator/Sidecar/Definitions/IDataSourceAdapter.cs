using System.Threading.Tasks;
using Workflows.Models.DataEvents;

namespace TestGrpcService.Definitions
{
    /// <summary>
    /// Interface used by the sidecar to get data from a data source.
    /// </summary>
    public interface IDataSourceAdapter
    {
        Task<PullDataReply> DownloadData(PullDataRequest metadata);
    }
}