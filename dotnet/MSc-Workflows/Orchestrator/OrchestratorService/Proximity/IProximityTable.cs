using System.Collections.Generic;
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
        private const int SameHostDistance = 0;

        private const int SameZoneDistance = 5;

        private int[][] InterZoneDistanceMatrix = {
            new[] { SameZoneDistance, 20, 100 }, 
            new[] { 20, SameZoneDistance, 30}, 
            new[] { 100 , 30, SameZoneDistance },
        };

        private Dictionary<string, int> ZoneIndex = new Dictionary<string, int>
        {
            {"edge1", 0},
            {"edge2", 1},
            {"cloud1", 2},
        };
        
        public int GetDistance(DataLocalization @from, DataLocalization to)
        {
            if (from.Host == to.Host)
            {
                return SameHostDistance;
            }
            var fromZone = ZoneIndex[from.Zone];
            var toZone = ZoneIndex[to.Zone];

            return InterZoneDistanceMatrix[fromZone][toZone];
        }
    }
}