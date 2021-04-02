using System.Runtime.CompilerServices;

namespace TelemetryReader
{
    public class TraceDetails
    {
        public string TraceId { get; set; }

        /// <summary>
        /// local or remote
        /// </summary>
        public string DataPullType { get; set; } = "none";

        /// <summary>
        /// Optional
        /// </summary>
        public string FromZone { get; set; } = "null";

        /// <summary>
        /// Optional.
        /// </summary>
        public string ToZone { get; set; } = "null";

        /// <summary>
        /// Optional.
        /// </summary>
        public long DataSize { get; set; } = 0;
        
        public int TotalDuration { get; set; }
        
        public int TriggerStepDuration { get; set; }
        
        public int DataPullDuration { get; set; }
        
        public int DataPushDuration { get; set; }
        
        public int ComputeDuration { get; set; }
        
        public int DataMasterPullCall { get; set; }

        public int DataPeerPullCall { get; set; }
        
        public int DataMasterPushCall { get; set; }

        public override string ToString()
        {
            return string.Join(',',
                TraceId,
                DataPullType,
                FromZone,
                ToZone,
                DataSize,
                TotalDuration,
                TriggerStepDuration,
                DataPullDuration,
                DataPushDuration,
                ComputeDuration,
                DataMasterPullCall,
                DataPeerPullCall,
                DataMasterPushCall);
        }
    }
}