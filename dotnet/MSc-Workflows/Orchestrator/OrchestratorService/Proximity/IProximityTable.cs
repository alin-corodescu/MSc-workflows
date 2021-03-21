using Workflows.Models.DataEvents;

namespace OrchestratorService.Proximity
{
    /// <summary>
    /// Interface for determining the proximity between two data localization specifications
    /// </summary>
    public interface IProximityTable
    {
        int GetDistance(DataLocalization from, DataLocalization to);
    }

    public class ProximityTable : IProximityTable
    {
        public int GetDistance(DataLocalization @from, DataLocalization to)
        {
            if (@from.HostIdentifier == to.HostIdentifier)
            {
                return 0;
            }

            if (@from.Region == @to.HostIdentifier)
            {
                return 1;
            }

            return 2;
        }
    }
}