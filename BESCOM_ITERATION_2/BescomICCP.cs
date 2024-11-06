using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static OSII.ClientNET.AppManager.ErrorInfo;
using static System.Collections.Specialized.BitVector32;

namespace AESES_Databases
{
    class BescomICCP
    {
        private readonly ICCP _iccpDB;
        private readonly GenericTable _scadaToFepXref;
        public static Dictionary<string, int> importset =  new Dictionary<string, int>();
       
        public BescomICCP(ICCP iccpdb, GenericTable scadaToFepXref)
        {
            this._iccpDB = iccpdb;
            this._scadaToFepXref = scadaToFepXref;
            this._scadaToFepXref.Sort("pRTU");
            
        }
        public static void ConvertControlCenterInfo(ICCP Iccpdb )
        {
            int ctrlRec = 0;
            Logger.OpenXMLLog("ConvertICCP");
            DbObject controlcenterobj = Iccpdb.GetDbObject("CONTROL_CENTER_INFO");
            #region ControlCenter
            ctrlRec++;
            controlcenterobj.CurrentRecordNo = ctrlRec;
            controlcenterobj.SetValue("Name", "Kaicp01");
            controlcenterobj.SetValue("IPAddress1", "172.16.100.7");
            controlcenterobj.SetValue("Hostname1","Host1");
            controlcenterobj.SetValue("PSEL","00 00 00 01");
            controlcenterobj.SetValue("SSEL","00 01");
            controlcenterobj.SetValue("TSEL", "00 01");
            controlcenterobj.SetValue("AE_Qualifier", "1001");
            controlcenterobj.SetValue("AP_Title", "1 3 9999 1001");

            ctrlRec++;
            controlcenterobj.CurrentRecordNo = ctrlRec;
            controlcenterobj.SetValue("Name", "Kaicp02");
            controlcenterobj.SetValue("IPAddress1", "172.16.100.8");
            controlcenterobj.SetValue("Hostname1", "Host2");
            controlcenterobj.SetValue("PSEL", "00 00 00 01");
            controlcenterobj.SetValue("SSEL", "00 01");
            controlcenterobj.SetValue("TSEL", "00 01");
            controlcenterobj.SetValue("AE_Qualifier", "1002");
            controlcenterobj.SetValue("AP_Title", "1 3 9999 1002");

            ctrlRec++;
            controlcenterobj.CurrentRecordNo = ctrlRec;
            controlcenterobj.SetValue("Name", "Kaicp11");
            controlcenterobj.SetValue("IPAddress1", "172.16.101.7");
            controlcenterobj.SetValue("Hostname1", "Host3");
            controlcenterobj.SetValue("PSEL", "00 00 00 01");
            controlcenterobj.SetValue("SSEL", "00 01");
            controlcenterobj.SetValue("TSEL", "00 01");
            controlcenterobj.SetValue("AE_Qualifier", "1003");
            controlcenterobj.SetValue("AP_Title", "1 3 9999 1003");

            ctrlRec++;
            controlcenterobj.CurrentRecordNo = ctrlRec;
            controlcenterobj.SetValue("Name", "Kaicp12");
            controlcenterobj.SetValue("IPAddress1", "172.16.101.7");
            controlcenterobj.SetValue("Hostname1", "Host4");
            controlcenterobj.SetValue("PSEL", "00 00 00 01");
            controlcenterobj.SetValue("SSEL", "00 01");
            controlcenterobj.SetValue("TSEL", "00 01");
            controlcenterobj.SetValue("AE_Qualifier", "1004");
            controlcenterobj.SetValue("AP_Title", "1 3 9999 1004");

            #endregion

            Logger.CloseXMLLog();
        }

        public static void ImportDataSet(ICCP Iccpdb)
        {

            int ImportDSRec = 0;
            Logger.OpenXMLLog("ConvertICCPImport");
            DbObject controlcenterobj = Iccpdb.GetDbObject("ICCP_IMPORT_DS");
            #region importset
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name","ANALOG");
            controlcenterobj.SetValue("Type",0);
            controlcenterobj.SetValue("Interval",5);
            string ds = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if(!importset.ContainsKey(ds))
            {
                importset.Add(ds,ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG2");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds1 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds1))
            {
                importset.Add(ds1, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG3");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds2 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds2))
            {
                importset.Add(ds2, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG4");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds3 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds3))
            {
                importset.Add(ds3, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG5");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds4 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds4))
            {
                importset.Add(ds4, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG6");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds5 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds5))
            {
                importset.Add(ds5, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG7");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds6 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds6))
            {
                importset.Add(ds6, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG8");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds7 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds7))
            {
                importset.Add(ds7, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG9");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds8 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds8))
            {
                importset.Add(ds8, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "ANALOG10");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds9 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds9))
            {
                importset.Add(ds9, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "STATUS");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds10 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds10))
            {
                importset.Add(ds10, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "STATUS");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds11 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds11))
            {
                importset.Add(ds11, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "STATUS1");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds12 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds12))
            {
                importset.Add(ds12, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "STATUS2");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds13 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds13))
            {
                importset.Add(ds13, ImportDSRec);
            }
            ImportDSRec++;
            controlcenterobj.CurrentRecordNo = ImportDSRec;
            controlcenterobj.SetValue("Name", "STATUS3");
            controlcenterobj.SetValue("Type", 0);
            controlcenterobj.SetValue("Interval", 5);
            string ds14 = controlcenterobj.GetValue("Name", ImportDSRec, 0);
            if (!importset.ContainsKey(ds14))
            {
                importset.Add(ds14, ImportDSRec);
            }
            #endregion

        }
    public static void ImportPoints(ICCP Iccpdb, GenericTable XrefSUB)
        {
            int ImportpointRec = 0;
            Logger.OpenXMLLog("ConvertICCPImportPoint");
            DbObject controlcenterobj = Iccpdb.GetDbObject("ICCP_IMPORT_POINT");

            XrefSUB.Sort("category , OSI Station, Object, Point Address");

            foreach (DataRow IMPpoints in BescomParser.ICCPCfg_Tbl.Rows)
            {

                string fullName = IMPpoints["name"].ToString();
                string dataSet = IMPpoints["dataset[0]"].ToString();
                string category = IMPpoints["category"].ToString();
                if (dataSet == "") continue;
                string stationPoint = IMPpoints["station"].ToString();
                string Point = IMPpoints["point"].ToString();
                string typename = IMPpoints["typename"].ToString();
                string sunstng =dataSet.Substring(0, 6);
                string obj = string.Empty;
                string ical = string.Empty;
                if (XrefSUB.TryGetRow(new[] { category,stationPoint, sunstng, Point }, out DataRow xrefRow))
                {
                    ImportpointRec++;
                    controlcenterobj.CurrentRecordNo = ImportpointRec;

                    var kee = xrefRow["SCADA Key"].ToString();
                    
                    controlcenterobj.SetValue("REC_KEY", xrefRow["SCADA Key"]);
                    controlcenterobj.SetValue("Name", fullName);
                    controlcenterobj.SetValue("DB_NUM", 10);
                    if(typename== "RealQ")
                    {
                        controlcenterobj.SetValue("TYPE", 2);
                    }
                    if (typename == "StateQTimeTagExtended")
                    {
                        controlcenterobj.SetValue("TYPE", 14);
                    }
                    if (importset.TryGetValue(dataSet, out int val))
                    {
                        controlcenterobj.SetValue("pImpDS", val);
                    }
                    if(dataSet.StartsWith("A"))
                    {
                        obj = "5";
                    }
                    if(dataSet.StartsWith("S")) 
                    {
                        obj = "4";
                    }
                    controlcenterobj.SetValue("OBJ_NUM", obj);
                    controlcenterobj.SetValue("REC_NUMBER", Point);


                }

            }   
        }
    
    }
}
