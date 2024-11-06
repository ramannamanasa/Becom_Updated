using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESES_Databases
{
    public class BescomSet
    {
        public BescomSet() { }

        public BescomSet(Dictionary<string, BescomFile> files)
        {
            Files = files;
        }
        
        public Dictionary<string, BescomFile> Files { get; set; }
    }
}
