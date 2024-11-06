using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESES_Databases
{
    public class BescomFile
    {
        public BescomFile() { }

        public BescomFile(string name, List<BescomRecord> aCSRecords)
        {
            Name = name;
            ACSRecords = aCSRecords;
            
        }

        public string Name { get; set; }
        public List<BescomRecord> ACSRecords { get; set; }  
    }
}
