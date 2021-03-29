using System.Collections.Concurrent;
using System.Collections.Generic;
using Workflows.Models.DataEvents;

namespace DataMaster
{
    public class LedgerValue
    {
        public string Address { get; set; }
        
        public DataLocalization Localization { get; set; }
    }
    
    /// <summary>
    /// Interface for classes that can keep track of the data that flows through the system.
    /// </summary>
    public interface IDataChunkLedger
    {
        public LedgerValue GetAddressForFileName(string fileName);
        
        public void StoreAddressForFileName(string fileName, LedgerValue ledgerValue);
    }

    class DataChunkLedger : IDataChunkLedger
    {
        private ConcurrentDictionary<string, LedgerValue> dataChunkLedger = new();

        public LedgerValue GetAddressForFileName(string fileName)
        {
            if (dataChunkLedger.ContainsKey(fileName))
                return dataChunkLedger[fileName];
            return new LedgerValue();
        }

        public void StoreAddressForFileName(string fileName, LedgerValue ledgerValue)
        {
            dataChunkLedger[fileName] = ledgerValue;
        }
    }
}