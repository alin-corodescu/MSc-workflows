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
            for (int i = 0; i < from.LocalizationCoordinates.Count; i++)
            {
                // return the position of the first coordinate that matches (can be host - the first one, or others).
                if (from.LocalizationCoordinates[i] == to.LocalizationCoordinates[i])
                {
                    return i;
                }
            }

            return from.LocalizationCoordinates.Count;
        }
    }
}