using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESES_Databases
{
    public class BescomRecord
    {
        public BescomRecord() { }
        

        public BescomRecord(int record, Dictionary<string, string> infoDict)
        {
            Record = record;
            InfoDict = infoDict;
        }

        public int Record { get; set; }
        public Dictionary<string, string> InfoDict { get; set; }
    }
}
