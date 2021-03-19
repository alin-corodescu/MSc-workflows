using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DataMaster
{
    /// <summary>
    /// Interface for classes that can keep track of the data that flows through the system.
    /// </summary>
    public interface IDataChunkLedger
    {
        public string GetAddressForFileName(string fileName);
        
        public void StoreAddressForFileName(string fileName, string address);
    }

    class DataChunkLedger : IDataChunkLedger
    {
        private ConcurrentDictionary<string, string> dataChunkLedger = new();

        public string GetAddressForFileName(string fileName)
        {
            if (dataChunkLedger.ContainsKey(fileName))
                return dataChunkLedger[fileName];
            return "";
        }

        public void StoreAddressForFileName(string fileName, string address)
        {
            dataChunkLedger[fileName] = address;
        }
    }
}