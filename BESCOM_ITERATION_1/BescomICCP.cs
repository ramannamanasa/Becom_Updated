using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AESES_Databases
{
    class BescomICCP
    {
        private readonly ICCP _iccpDB;
        private readonly GenericTable _scadaToFepXref;

        public BescomICCP(ICCP iccpdb, GenericTable scadaToFepXref)
        {
            this._iccpDB = iccpdb;
            this._scadaToFepXref = scadaToFepXref;
            this._scadaToFepXref.Sort("pRTU");
            
        }
        public static void ConvertControlCenterInfo(ICCP Iccpdb )
        {
            Logger.OpenXMLLog();
            DbObject controlcenterobj = Iccpdb.GetDbObject("CONTROL_CENTER_INFO");

            Logger.CloseXMLLog();
        }
    }
}
