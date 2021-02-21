using System.Collections.Generic;

namespace DataMaster
{
    public interface IDataChunkLedger
    {
        public string GetAddressForFileName(string fileName);
        
        public void StoreAddressForFileName(string fileName, string address);
    }

    class DataChunkLedger : IDataChunkLedger
    {
        private Dictionary<string, string> dataChunkLedger = new Dictionary<string, string>();

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