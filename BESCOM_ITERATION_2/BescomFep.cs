using Microsoft.SqlServer.Server;
using OSII.ClientNET.AppManager;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Extensions;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using static OSII.ClientNET.AppManager.SystemMaintenanceManager;

namespace AESES_Databases
{
    class BescomFep
    {
        private static int controlRec = 0;
        private readonly FEP _fepDb;  
        private readonly GenericTable _scadaToFepXref;  
        private static Dictionary<string, int> RtuDataDict = new Dictionary<string, int>();
        private static Dictionary<string, int> ChannelgrpDataDict = new Dictionary<string, int>();// Local reference to the RTU_DATA Dictionary.
        public BescomFep(FEP fepDb, GenericTable scadaToFepXref)
        {
            this._fepDb = fepDb;
            this._scadaToFepXref = scadaToFepXref;
            this._scadaToFepXref.Sort("pRTU");
            //this._rtuDataDict = RTUDataDict;
        }
        #region DictRTUChannel
        public static void DictRTUChannelChannelGroup()
        {
            foreach (DataRow rtuRow in BescomParser.mastercfg_filteredTbl.Rows)
            {

                string fullName = rtuRow["SIETID"].ToString();
                int rtuadrs = Convert.ToInt32(rtuRow["rtu_address"].ToString());
                if (!RtuDataDict.ContainsKey(fullName))
                {
                    RtuDataDict.Add(fullName, rtuadrs);
                }
            }

            foreach (DataRow chnlgrpRow in BescomParser.dnpnetcfgport_Tbl.Rows)
            {
                string name = chnlgrpRow["name"].ToString();
                int Records = chnlgrpRow["REC"].ToInt();
                if (!ChannelgrpDataDict.ContainsKey(name))
                {
                    ChannelgrpDataDict.Add(name, Records);
                }
            }
        }
        #endregion
        /// <summary>
        /// Function to convert channel groups
        /// </summary>
        /// <param name="FepDb"type="FEP"></param>
        #region CHANNEL_GROUP
        //public static void ConvertChannelGroup(FEP FepDb)
        //{
        //    Logger.OpenXMLLog();

        //    DbObject channelGroupObj = FepDb.GetDbObject("CHANNEL_GROUP");
        //    int ChlGrpRec = 0; int id = 0; int ChlRec = 0;
           
        //    foreach (DataRow chnlgrpRow in BescomParser.dnpnetcfgdevice_Tbl.Rows)
        //    {

        //        string name = chnlgrpRow["name"].ToString().Trim();
        //        string IpPortSplit = chnlgrpRow["port"].ToString();
        //        string[] IpDetails = IpPortSplit.Split(':');
        //        string HostName = IpDetails[0];
        //        string HostPort = IpDetails[1];
        //        if (string.IsNullOrEmpty(name)) continue;

        //        if (name.Length > 32) Logger.Log("Chl_Grp_Truncation", LoggerLevel.INFO, $"Name exceeded 32 characters, truncated : {name }");

        //        id = GetpRTU(HostName, HostPort);
        //        channelGroupObj.CurrentRecordNo = ++ChlGrpRec;
               
        //        channelGroupObj.SetValue("Name", name);
        //        channelGroupObj.SetValue("Type", 5);
              
        //        channelGroupObj.SetValue("Protocol", 0, (int)FepProtocol.DNP);
        //        channelGroupObj.SetValue("Parameters", 0, 0);//Check
        //        //channelGroupObj.SetValue("Parameters", 3, 0);//Check
        //        channelGroupObj.SetValue("Parameters", 3, 23);
        //        channelGroupObj.SetValue("Parameters", 8, 1);//Check
        //        //channelGroupObj.SetValue("Parameters", 9, 0);//Check
        //        channelGroupObj.SetValue("Parameters", 9, 2);

                
                
        //        #region AReff
        //        //id = GetpRTU(chnlgrpRow["ip_address"].ToString() , chnlgrpRow["port_number"].ToString());
        //        //DataRow pRTU = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { name });
        //        //foreach (DataRow rtuRow in AESESParser.dnpnetcfgport_Tbl.Rows)
        //        //foreach (DataRow chnlgrpRow in BescomParser.dnpnetcfgport_Tbl.Rows)
        //        // channelGroupObj.SetValue("pRTU", pRTUID);
        //        //channelGroupObj.SetValue("pCHANNEL", 0, pChannelID);
        //        // channelGroupObj.SetValue("pChannel", 0, ++ChlRec);

        //        //channelGroupObj.SetValue("Parameters", 0, 0);
        //        //channelGroupObj.SetValue("Parameters", 3, 0);
        //        //channelGroupObj.SetValue("Parameters", 8, 1);
        //        //channelGroupObj.SetValue("Parameters", 9, 0);

        //        #endregion
        //    }
        //    Logger.CloseXMLLog();
        //}

        #endregion
        /// <summary>
        /// Function to convert RTUs
        /// </summary>
        /// <param name="FepDb"type="FEP"></param>
        /// 

        public static void ConvertChannels(FEP FepDb)
        {
            Logger.OpenXMLLog("Convert_Channel&Channelgroup");
            DbObject channelObj = FepDb.GetDbObject("CHANNEL");
            DbObject channelGroupObj = FepDb.GetDbObject("CHANNEL_GROUP");
            DbObject Channelgroupdef = FepDb.GetDbObject("CHANNEL_GROUP_DEFN");
            int ChlGrpRec = 0; int id = 0; string name1 = ""; string name2 = ""; int chngrp = 0;
            int chnlgrpdef = 0;
             foreach (DataRow chnlRow in BescomParser.dnpnetcfgport_Tbl.Rows)
            //foreach (DataRow chnlRow in BescomParser.dnpnetcfgdevice_Tbl.Rows)
            {
                string name = chnlRow["name"].ToString();
                string physicAddress = chnlRow["ip_address"].ToString();
                string HostPort = chnlRow["port_number"].ToString();
                string ChannwlRespTime = "30000";

                #region CHANNEL
                ++chngrp;
                channelObj.CurrentRecordNo = chngrp;



                int channelnum = channelObj.CurrentRecordNo;

                channelObj.SetValue("Name", name);
                channelObj.SetValue("pCHANNEL_GROUP", channelnum);
                channelObj.SetValue("PhysicalPort", HostPort);
                channelObj.SetValue("Hostname", physicAddress);
                channelObj.SetValue("ChannelRespTimeoutMsec", ChannwlRespTime);

                if (name1.Length > 16) Logger.Log("Channel_Truncation", LoggerLevel.INFO, $"Name exceeded 16 characters, truncated : {name1}"); //BD
                #endregion
                #region CHANNEL_GROUP
                if (string.IsNullOrEmpty(name)) continue;
                ++ChlGrpRec;
                channelGroupObj.CurrentRecordNo = ChlGrpRec;
               
                channelGroupObj.SetValue("Name", name);
                channelGroupObj.SetValue("Type", 5);
                id = GetpRTU(physicAddress, HostPort);
                channelGroupObj.SetValue("pRTU", id);
                channelGroupObj.SetValue("pCHANNEL", 0,ChlGrpRec);
                channelGroupObj.SetValue("Protocol", 0, (int)FepProtocol.DNP);
                channelGroupObj.SetValue("Parameters", 0, 0);
                //channelGroupObj.SetValue("Parameters", 3, 0);
                channelGroupObj.SetValue("Parameters", 3, 23);
                channelGroupObj.SetValue("Parameters", 8, 1);
                //channelGroupObj.SetValue("Parameters", 9, 0);
                channelGroupObj.SetValue("Parameters", 9, 2);
                #endregion

                #region ChannlGroupDefn
                chnlgrpdef++;
                Channelgroupdef.CurrentRecordNo = chnlgrpdef;
                Channelgroupdef.SetValue("MasterAddr", 65519);
                #endregion
                if (name.Length > 32) Logger.Log("Chl_Grp_Truncation", LoggerLevel.INFO, $"Name exceeded 32 characters, truncated : {name}");
              
            }
            Logger.CloseXMLLog();
        }


        public static void ConvertRtuData(FEP FepDb)
        {
            Logger.OpenXMLLog();

            DbObject rtuDataObj = FepDb.GetDbObject("RTU_DATA");
            DbObject scandef = FepDb.GetDbObject("SCAN_DEFN");
            DbObject Demscandef = FepDb.GetDbObject("DEMAND_SCAN_DEFN");
            DbObject Initscandef = FepDb.GetDbObject("INIT_SCAN_DEFN");


            int rtuRec = 0; int id = 0; int ChlgrpRec = 0;
            int INDIC = 1;
            int SUBTYPE = 2;
            int newaddree = 0;
            int scanrec = 0;
            int demandscanrec = 0;
            int initScanDef = 0;
            foreach (DataRow rtuRow in BescomParser.mastercfg_filteredTbl.Rows)
            {
                if (string.IsNullOrEmpty(rtuRow["rtu_address"].ToString())) continue;

                string fullName = rtuRow["SIETID"].ToString();
                DataRow address = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] { fullName }); 
                if(address == null) continue;
                string addr = address["comm_address"].ToString() ;
                if (addr.ToInt() == 0) newaddree = 33;
                int rtuadrs = Convert.ToInt32(rtuRow["rtu_address"].ToString());
                int.TryParse(rtuRow["id"].ToString(), out rtuRec);
                if (rtuRec == 0) continue;
                if (string.IsNullOrEmpty(fullName)) continue;
                rtuDataObj.CurrentRecordNo = rtuRec;
                scanrec++;
                rtuDataObj.SetValue("Name", fullName);
                scandef.CurrentRecordNo=scanrec;
                scandef.SetValue("ScanType",0,1);
                scandef.SetValue("ScanType", 1, 2);
                scandef.SetValue("ScanType", 2, 3);
                scandef.SetValue("ScanType", 3, 4);
                scandef.SetValue("ScanType", 4, 5);
                scandef.SetValue("ScanType", 5, 6);
                scandef.SetValue("ScanType", 6, 7);
                //scandef.SetValue("ScanType", 7, 7);
                demandscanrec++;
                Demscandef.CurrentRecordNo = demandscanrec;
                Demscandef.SetValue("ScanType", 0, 1);
                Demscandef.SetValue("ScanType", 1, 2);
                Demscandef.SetValue("ScanType", 2, 3);
                Demscandef.SetValue("ScanType", 3, 4);
                Demscandef.SetValue("ScanType", 4, 5);
                Demscandef.SetValue("ScanType", 5, 6);
                Demscandef.SetValue("ScanType", 6, 7);
               // Demscandef.SetValue("ScanType", 7, 7);
                initScanDef++;
                Initscandef.CurrentRecordNo = initScanDef;
                Initscandef.SetValue("ScanType", 0, 1);
                Initscandef.SetValue("ScanType", 1, 2);
                Initscandef.SetValue("ScanType", 2, 3);
                Initscandef.SetValue("ScanType", 3, 4);
                Initscandef.SetValue("ScanType", 4, 5);
                Initscandef.SetValue("ScanType", 5, 6);
                Initscandef.SetValue("ScanType", 6, 7);
               // Initscandef.SetValue("ScanType", 7, 7);
                rtuDataObj.SetValue("Indic", INDIC);
                if (fullName.Length > 25) Logger.Log("RTU_Truncation", LoggerLevel.INFO, $"Name exceeded 25 characters, truncated : {fullName }"); //BD

                rtuDataObj.SetValue("Protocol", (int)FepProtocol.DNP);
                rtuDataObj.SetValue("SubType", SUBTYPE);
                //rtuDataObj.SetValue("Address", rtuadrs);
                rtuDataObj.SetValue("Address", addr);

                int.TryParse(rtuRow["AOR_Group"].ToString(), out int pAor);
                rtuDataObj.SetValue("pAORGroup", pAor);
                
                int stnIPAd = 0;
                DataRow newrow = BescomParser.dnpnetcfgdevice_Tbl.GetInfo( new object[]{ fullName});

                DataRow devicerow = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] {fullName});
                if (devicerow != null)
                {
                    string stnIPAdnew = devicerow["port"].ToString();
                    string stnIPDnew = stnIPAdnew.Split(':')[0];
                    string stnPort = stnIPAdnew.Split(':')[1];
                    ChlgrpRec = Getpchlgrp(stnIPDnew, stnPort, rtuRec);

                }
                
                else
                {
                    Logger.Log("No_DNPSTnNum_MasterStnNumNotFound", LoggerLevel.INFO, $"No Match for for station_number in the master file found in the Station column of dnpnetconfig_device file : {stnIPAd}"); //BD

                }
                if (ChlgrpRec == 34 && rtuRec > 3384) ChlgrpRec = 35;
                if (ChlgrpRec == 35 && rtuRec > 4406) ChlgrpRec = 36;
                rtuDataObj.SetValue("pCHANNEL_GROUP", 0, ChlgrpRec);
            }

            Logger.CloseXMLLog();

        }

        public static void GlobalScanDefn(FEP FepDb)
        {
            DbObject GlobalScan = FepDb.GetDbObject("GLOBAL_SCAN_DEFN");
            int globalrec = 0;
            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "READ CLASS 0");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 92);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "READ CLASS 1");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 93);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "READ CLASS 2");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 94);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "READ CLASS 3");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 95);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "ENABLE USM");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 20);
            GlobalScan.SetValue("ScanType", 1);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);
            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "DISABLE USM");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 21);
            GlobalScan.SetValue("ScanType", 1);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "BINARY ALL VARIATIONS");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 1);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "ANALOG ALL VARIATIONS");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 55);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "COUNTER ALL VARIATIONS");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 15);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);

            globalrec++;
            GlobalScan.CurrentRecordNo = globalrec;
            GlobalScan.SetValue("name", "COUNTER FROZEN");
            GlobalScan.SetValue("Protocol", 8);
            GlobalScan.SetValue("FuncCode", 1);
            GlobalScan.SetValue("ScanType", 15);
            GlobalScan.SetValue("pGSG", 11);
            GlobalScan.SetValue("ProtoParmShort", -1);




        }


        /// <summary>
        /// Function to convert channels
        /// </summary>
        /// <param name="FepDb"type="FEP"></param>


        public static string GetRTUName(int rtu_address) 
        {
           
            DataRow devicerow = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] { rtu_address });
            if (devicerow != null)
            {
                string name = devicerow["name"].ToString();
                return name;
            }
            else
            {
                Logger.Log("No_RTUAddress_DNPStation_Match", LoggerLevel.INFO, $"No Match for for RTU_Address in the master file found in the Station column of dnpnetconfig_device file : {rtu_address }"); //BD
                return "";
            }
               
        }
        public static int GetpRTU(string ipaddress, string port_number) //BD
        {
            string channelpRTU = ipaddress +":"+ port_number; 
            BescomParser.dnpnetcfgdevice_Tbl.Sort("port");
            DataRow devicerow1 = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] { port_number });
            DataRow devicerow = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] { channelpRTU });
            if (devicerow != null)
            {
               // string name = devicerow["port"].ToString();
                string name = devicerow["name"].ToString();
                if (RtuDataDict.ContainsKey(name))
                {
                    return RtuDataDict[name];
                }
                else return 0;
            }
            else
            {
                Logger.Log("No_RTU_Match", LoggerLevel.INFO, $"No Match for for RTU in the device file found in the port column of dnpnetconfig_device file : {ipaddress }"); //BD
                return 0;
            }

        }
        public static int Getpchlgrp(string ipaddress,string port,int rec) //BD
        {
            BescomParser.dnpnetcfgport_Tbl.Sort("ip_address,port_number");
            
            DataRow portrow = BescomParser.dnpnetcfgport_Tbl.GetInfo(new object[] { ipaddress , port});
            if (portrow != null)
            {
                int recno = 0;
                int.TryParse(portrow["REC"].ToString(), out recno);
                return recno;
                
            }
            else
            {
                
                Logger.Log("No_RTU_Match", LoggerLevel.INFO, $"No Match for for RTU in the device file found in the port column of dnpnetconfig_device file : {ipaddress }"); //BD
                //if(rec>3500)
                //    return 34;
                //if (rec > 4444)
                //    return 35;
                return 34;
            }

        }
        public static string GetIPAddress(int rtu_address) //BD
        {
          
            string ipaddrs = "";
            DataRow devicerow = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] { rtu_address });
            if (devicerow != null)
            {
                ipaddrs = devicerow["ip_address"].ToString();
                return ipaddrs;
            }
            else
            {
                Logger.Log("No_RTUAddress_DNPStation_Match", LoggerLevel.INFO, $"No Match for for RTU_Address in the master file found in the Station column of dnpnetconfig_device file : {rtu_address }"); //BD
                return ipaddrs;
            }
           
        }
        public static int SetRtuName(BescomRecord record, int stationid) //BD
        {
            DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { stationid });
            if (stationRow != null)
            {
                int.TryParse(stationRow["station_number"].ToString(), out int stationNum);
                return stationNum;
            }
            else
                return 1;
        }

        #region notRequired
        /// <summary>
        /// Function to convert Control points bases on input from CTL and STSR files. Gets called from HandleScada
        /// </summary>
        /// <param name="FepDb" type="FEP"> FebDb passed in from HandleScada </param>
        /// <param name="statusFileName" type="string"> Name of the STSR file passed </param>
        /// <param name="key" type="string"> SCADA key from the status point that needs a control point </param>
        /// <param name="pStation" type="int"> pStation of the status point </param>
        /// <param name="statusRec" type="int"> record number of the current status point </param>
        //public static void ConvertRtuControl(FEP FepDb, string statusFileName, string key, int pStation, int statusRec)
        //{
        //    DbObject controlObj = FepDb.GetDbObject("RTU_CONTROL");
        //    string station = statusFileName.Split('.')[0].Replace("STSR", "CTL");
        //    for (int i = 0; i < 2; i++)
        //    {
        //        ACSRecord curRec;
        //        int type = 0;

        //        controlObj.CurrentRecordNo = ++controlRec;

        //        controlObj.SetValue("SourceKey", key);
        //        controlObj.SetValue("pRTU", pStation);

        //        if (AESESParser.ctlSet.Files.Keys.Contains(station))
        //        {
        //            foreach (ACSRecord ctlRecord in AESESParser.ctlSet.Files[station].ACSRecords)
        //            {
        //                if (!ctlRecord.InfoDict.Keys.Contains("solicited_station")) continue;
        //                if (ctlRecord.InfoDict["solicited_point"].Equals(statusRec.ToString()))
        //                {
        //                    curRec = ctlRecord;
        //                    controlObj.SetValue("point_address", curRec.InfoDict[i == 0 ? "point1" : "point2"]);
        //                    string tempType = ctlRecord.InfoDict["cmd_code"];
        //                    if (i.Equals(0))
        //                    {
        //                        switch (tempType)
        //                        {
        //                            case "224":
        //                                type = 1;
        //                                break;
        //                            default:
        //                                type = 4;
        //                                break;
        //                        }
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        switch (tempType)
        //                        {
        //                            case "224":
        //                                type = 2;
        //                                break;
        //                            default:
        //                                type = 5;
        //                                break;
        //                        }
        //                        break;
        //                    }
        //                }
        //            }
        //        }
        //        if (type.Equals(0))
        //        {
        //            Logger.Log("Missing CTL Record", LoggerLevel.INFO, $"For SCADA Point: {key} matching CTL point not found.");
        //        }
        //        controlObj.SetValue("control_type", type);

        //        // set defaults 
        //        controlObj.SetValue("def_execute_ticks", 50);
        //        controlObj.SetValue("dnp_control_code", 1);
        //        controlObj.SetValue("control_format", 1);
        //        controlObj.SetValue("execute_mult", 100);
        //    }
        //}
        #endregion
        static string ExtractDigits(string input)
        {
            Match match = Regex.Match(input, @"\d{1,4}$");
            if (match.Success)
            {
                string digits = match.Value;
                // Remove leading zeros
                string result = Convert.ToInt32(digits).ToString();
                return result;
            }
            else
            {
                return ""; // or throw exception or handle accordingly
            }
            //return match.Success ? match.Value : string.Empty;
        }
        public static void ConvertRTUNewControl(FEP FepDb, GenericTable Xref)
        {
            Logger.OpenXMLLog();
            int rtuctlrec = 0;
            DbObject controlObj = FepDb.GetDbObject("RTU_CONTROL");
            foreach (DataRow ctlRecord in BescomParser.dnpnetcfgctrlpt_Tbl.Rows)
            {
                string device = ctlRecord["device"].ToString();
                string controlpt = ctlRecord["control_mechanism"].ToString();
                string deviceAct = ExtractDigits(device);
                DataRow xrefnew1 = null;

                if (device.Contains("3W")|| device.Contains("5W")|| device.Contains("4W"))
                {
                   
                    foreach (DataRow RmuRec in BescomParser.RMU.Rows)
                    {
                        rtuctlrec++;
                        string name = RmuRec["name"].ToString(); 
                        string address = RmuRec["address"].ToString();
                        string controltype = RmuRec["Control Type"].ToString();
                        Xref.Sort("pRTU, SCADA Name, Type ");
                        controlObj.CurrentRecordNo = rtuctlrec;
                        string[] newname = name.Split(' ');
                        string name1 = newname[0].ToString();
                        string name2 = newname[1].ToString();
                        string name3 = name1 +" "+ name2;
                        if(name == "FPI RESET")
                        {
                            int i = 0;
                        }
                        if (Xref.TryGetRow(new[] { deviceAct, name3,"2"}, out DataRow xrefnew))
                        {
                            string Actname = xrefnew["SCADA Name"].ToString();
                            string ActKey = xrefnew["SCADA Key"].ToString();
                            controlObj.SetValue("Name", name);// SourceKey
                            controlObj.SetValue("SourceKey", ActKey);

                            controlObj.SetValue("pRTU", xrefnew["pRTU"]);
                            controlObj.SetValue("mode", 1);

                            controlObj.SetValue("point_address", address);
                            int cntrlfomt = ctlRecord["control_mechanism"].ToString() == "SBO" ? 1 : 2;
                            controlObj.SetValue("control_format", cntrlfomt);
                            controlObj.SetValue("control_type", controltype);
                           // controlObj.SetValue("dnp_control_code", controlPoint);
                        }
                      
                        
                        if ((Xref.TryGetRow(new[] { deviceAct, name, "2" }, out  xrefnew1))|| (Xref.TryGetRow(new[] { deviceAct, name, "3" }, out  xrefnew1)))
                        {
                            string Actname = xrefnew1["SCADA Name"].ToString();
                            string ActKey = xrefnew1["SCADA Key"].ToString();
                           
                            controlObj.SetValue("Name", name);
                            controlObj.SetValue("SourceKey", ActKey);
                            if (Actname.Contains("/"))
                            {
                               string  Removename = Actname.Replace('/', ' ');
                                controlObj.SetValue("Name", Removename);
                                
                            }

                            //controlObj.SetValue("SourceKey", ActKey);
                            controlObj.SetValue("pRTU", xrefnew1["pRTU"]);
                            controlObj.SetValue("mode", 1);

                            controlObj.SetValue("point_address", address);
                            int cntrlfomt = ctlRecord["control_mechanism"].ToString() == "SBO" ? 1 : 2;
                            controlObj.SetValue("control_format", cntrlfomt);
                            controlObj.SetValue("control_type", controltype);
                        }


                    }

                }
                if (device.Contains("LR"))
                {
                    foreach (DataRow RmuRec in BescomParser.LRC.Rows)
                    {
                        rtuctlrec++;
                        string name = RmuRec["name"].ToString();
                        string address = RmuRec["address"].ToString();
                        string controltype = RmuRec["Control Type"].ToString();
                        string[] newname = name.Split(' ');
                        string name1 = newname[0].ToString();
                        controlObj.CurrentRecordNo = rtuctlrec;
                        if ((Xref.TryGetRow(new[] { deviceAct, name, "2" }, out xrefnew1)) || (Xref.TryGetRow(new[] { deviceAct, name, "3" }, out xrefnew1)))
                        {
                            string Actname = xrefnew1["SCADA Name"].ToString();
                            string ActKey = xrefnew1["SCADA Key"].ToString();
                            if(name== "RELAY FORWARD/REVERSE STATUS")
                            {
                                name = "RELAY FORWARD REVERSE STATUS";
                            }
                            controlObj.SetValue("Name", name);// SourceKey
                            controlObj.SetValue("SourceKey", ActKey);

                            controlObj.SetValue("pRTU", xrefnew1["pRTU"]);
                            controlObj.SetValue("mode", 1);

                            controlObj.SetValue("point_address", address);
                            int cntrlfomt = ctlRecord["control_mechanism"].ToString() == "SBO" ? 1 : 2;
                            controlObj.SetValue("control_format", cntrlfomt);
                            controlObj.SetValue("control_type", controltype);
                        }
                    }

                }
                if (device.Contains("LB"))
                {
                    foreach (DataRow RmuRec in BescomParser.LBS.Rows)
                    {
                        rtuctlrec++;
                        string name = RmuRec["name"].ToString();
                        string address = RmuRec["address"].ToString();
                        string controltype = RmuRec["Control Type"].ToString();
                        string[] newname = name.Split(' ');
                        string name1 = newname[0].ToString();
                        controlObj.CurrentRecordNo = rtuctlrec;
                        if ((Xref.TryGetRow(new[] { deviceAct, name1, "2" }, out xrefnew1)) || (Xref.TryGetRow(new[] { deviceAct, name, "3" }, out xrefnew1)))
                        {
                            string Actname = xrefnew1["SCADA Name"].ToString();
                            string ActKey = xrefnew1["SCADA Key"].ToString();
                            controlObj.SetValue("Name", name);// SourceKey
                            controlObj.SetValue("SourceKey", ActKey);

                            controlObj.SetValue("pRTU", xrefnew1["pRTU"]);
                            controlObj.SetValue("mode", 1);

                            controlObj.SetValue("point_address", address);
                            int cntrlfomt = ctlRecord["control_mechanism"].ToString() == "SBO" ? 1 : 2;
                            controlObj.SetValue("control_format", cntrlfomt);
                            controlObj.SetValue("control_type", controltype);
                        }

                    }

                }

            }

            }
        public static void ConvertRtuControl(FEP FepDb, GenericTable Xref)
        {
            Logger.OpenXMLLog();
            DbObject controlObj = FepDb.GetDbObject("RTU_CONTROL");
           
            BescomParser.dnpnetcfgdevice_Tbl.Sort("name");
            Xref.Sort("filename,CtlRecord,Type");
          
            foreach (DataRow ctlRecord in BescomParser.dnpnetcfgctrlpt_Tbl.Rows)
            {
                string device = ctlRecord["device"].ToString();
                if(device=="3W2899")
                {
                    int i = 0;
                }

                string devicePtNo = ctlRecord["device_point_number"].ToString();
               
                string controlType = ctlRecord["control_function"].ToString();
                string deviceno = "";
                int controlPoint = 0;
                int controlCode = 0;
                if(controlType=="PON")
                {
                    controlPoint = 1;
                }
                else if(controlType=="POFF")
                {
                    controlPoint = 2;
                }
                else if (controlType == "LON")
                {
                    controlPoint = 3;
                }
                else if (controlType == "LOFF")
                {
                    controlPoint = 4;
                }
                else if (controlType == "CL")
                {
                    controlPoint = 65; //65 ,hex 41
                }
                else if (controlType == "TR")
                {
                    controlPoint = 129;//129 hex 81
                }
                
                if ((controlType == "PON")|| (controlType == "POFF"))
                {
                    controlCode = 0;
                }
                else if ((controlType == "LON")|| (controlType == "CL"))
                {
                    controlCode = 5;
                }
                else if ((controlType == "LOFF")|| (controlType == "TR"))
                {
                    controlCode = 4;
                }
                

                DataRow deviceRow = BescomParser.dnpnetcfgdevice_Tbl.GetInfo(new object[] { device });
                if (deviceRow != null)
                {
                    string stationNo = deviceRow["station"].ToString();
                    DataRow xrefRow = null;
                    deviceno = stationNo.PadLeft(5, '0');
                    string fileName = "CTL" + deviceno;
                    if ((Xref.TryGetRow(new[] { fileName, devicePtNo, "2" }, out xrefRow)) || (Xref.TryGetRow(new[] { fileName, devicePtNo, "3" }, out xrefRow)))
                    {
                        if (xrefRow != null)
                        {
                            string prevptr = controlObj.GetValue("pRTU", controlRec, 0);
                            string prevpt1 = controlObj.GetValue("point_address", controlRec, 0);
                            controlObj.CurrentRecordNo = ++controlRec;
                            string name = xrefRow["SCADA Name"].ToString();
                            string actname = string.Empty;
                            if(name== "RELAY 1 GROUP A/B ACTIVE")
                            {
                                int i = 0;
                            }
                            if (name.Contains("/"))
                            {
                                actname = name.Replace('/', ' ');
                                controlObj.SetValue("Name", actname);
                            }
                           
                            else
                                controlObj.SetValue("Name", name);
                            string rtu = xrefRow["pRTU"].ToString();


                            if ((devicePtNo == prevpt1) && (rtu == prevptr))
                            {
                                continue;
                            }
                            //controlObj.SetValue("Name", xrefRow["SCADA Name"]);

                            controlObj.SetValue("SourceKey", xrefRow["SCADA Key"]);
                            string key = xrefRow["SCADA Key"].ToString();
                            
                            controlObj.SetValue("pRTU", xrefRow["pRTU"]);
                            controlObj.SetValue("mode", 1);

                            controlObj.SetValue("point_address", devicePtNo);
                            int cntrlfomt = ctlRecord["control_mechanism"].ToString() == "SBO" ? 1 : 2;
                            controlObj.SetValue("control_format", cntrlfomt);
                            controlObj.SetValue("control_type", controlCode);
                            controlObj.SetValue("dnp_control_code", controlPoint);

                        }
                    }
                   

                }
                else
                {
                    Logger.Log("Ctrl_No_Match", LoggerLevel.INFO, $"No Match for device in ctrl point in device tab : {device} and ptNo: {devicePtNo}"); //BD

                }
            }
            Logger.CloseXMLLog();


        }
        public void CountPointTypeGroups(int pRtu, ref ConcurrentDictionary<string, List<DataRow>> fepPointGroups, ref Dictionary<string, int> feptypecount)
        {
            Dictionary<int, List<DataRow>> mapRTUDdataRow = new Dictionary<int, List<DataRow>>();
       
            DataView dataView = this._scadaToFepXref.DataView;
            //dataView.Sort("pRtu");
            var drs = dataView.FindRows(pRtu);
            if (drs.Length > 0)
            {
                var enumerator = drs.GetEnumerator();
                
                while (enumerator.MoveNext())
                {
                    var dataRow = ((System.Data.DataRowView)enumerator.Current).Row;


                    int RTURec = dataRow["pRtu"].ToInt();
                    List<DataRow> DataRowlist = new List<DataRow>();
                    if (mapRTUDdataRow.ContainsKey(RTURec))
                        DataRowlist = mapRTUDdataRow[RTURec];

                    DataRowlist.Add(dataRow);
                    mapRTUDdataRow[RTURec] = DataRowlist;
                }
                int i = 0;
            }
           
            //for (int i = 0; i < dataView.Count; i++)
            //{
            //    DataRow dataRow = dataView[i].Row;
            //    int RTURec = dataRow["pRtu"].ToInt();
            //    if (RTURec.Equals(pRtu))
            //    {
            //        List<DataRow> DataRowlist = new List<DataRow>();
            //        if (mapRTUDdataRow.ContainsKey(RTURec))
            //            DataRowlist = mapRTUDdataRow[RTURec];

            //        DataRowlist.Add(dataRow);
            //        mapRTUDdataRow[RTURec] = DataRowlist;
            //    }
            //}



            if (mapRTUDdataRow.ContainsKey(pRtu))
            {
                int prevIOA = 0;
                int singles = 0;
                int doubles = 0;
                int meas = 0;
                int ints = 0;
                int BinOutcount = 0;
                int BinIncount = 0;
                int AnalogINcount = 0;
                int BinanalogOUTcount = 0;
                int CNTFrozen = 0;
                if(pRtu==4999)
                {
                    int i = 0;
                }
                fepPointGroups = new ConcurrentDictionary<string, List<DataRow>>();
                feptypecount = new Dictionary<string, int>();

                var DataRowlist = mapRTUDdataRow[pRtu];
               
                for (int i = 0; i < DataRowlist.Count; i++)
                {
                    if(i==126)
                    {
                        int k = 0;
                    }
                    DataRow dataRow = DataRowlist[i];
                    if (dataRow["pRtu"].ToInt().Equals(pRtu))
                    {
                        switch (dataRow["Type"].ToString())
                        {
                            case "1":
                                if(dataRow["Object"].ToString() == "STATUS")
                                {
                                   
                                    BinIncount = BinIncount + 1;
                                }
                                    
                                if(dataRow["Object"].ToString() == "ANALOG")
                                {
                                   
                                    AnalogINcount = AnalogINcount + 1;
                                }
                                if (dataRow["Object"].ToString() == "SETPOINT")
                                {

                                    BinanalogOUTcount = BinanalogOUTcount + 1;
                                }
                                if (dataRow["Object"].ToString() == "ACCUM")
                                {

                                    CNTFrozen = CNTFrozen + 1;
                                }

                                break;
                            case "2":
                                if (dataRow["Object"].ToString() == "STATUS")
                                {
                                    
                                    BinOutcount = BinOutcount + 1; 
                                                                   
                                    BinIncount = BinIncount + 1;
                                }
                                break;
                            case "3":
                                if (dataRow["Object"].ToString() == "STATUS")
                                {

                                    BinOutcount = BinOutcount + 1;

                                    BinIncount = BinIncount + 1;
                                }
                                if (dataRow["Object"].ToString() == "SETPOINT")
                                {

                                    BinanalogOUTcount = BinanalogOUTcount + 1;
                                }
                                break;

                        }
                    }

                }
                feptypecount.Add("BIN_IN", BinIncount);
                feptypecount.Add("BIN_OUT", BinOutcount);
                feptypecount.Add("ANALG_IN", AnalogINcount);
                feptypecount.Add("ANALG_OUT", BinanalogOUTcount);
                feptypecount.Add("CNTFROZEN", CNTFrozen);

            }

        }
        public void AddToDict(ConcurrentDictionary<string, List<DataRow>> fepPointGroups, DataRow dataRow, string mainGroup, ref int subGroup, ref int prevIOA)
        {
            if (dataRow["IOA"].TryParseInt(out int ioa) && (ioa.Equals(prevIOA + 1) || prevIOA.Equals(0)))
            {
                if (fepPointGroups.ContainsKey($"{mainGroup}{subGroup}"))
                {
                    fepPointGroups[$"{mainGroup}{subGroup}"].Add(dataRow);
                }
                else
                {
                    List<DataRow> temp = new List<DataRow>
                    {
                        dataRow
                    };
                    fepPointGroups.TryAdd($"{mainGroup}{subGroup}", temp);
                 
                }
                prevIOA = ioa;
            }
            else
            {
                ++subGroup;
                List<DataRow> temp = new List<DataRow>
                {
                    dataRow
                };
                
                fepPointGroups.TryAdd($"{mainGroup}{subGroup}", temp);
                prevIOA = ioa;
            }
        }
        /// <summary>
        /// Function to convert RTU definitions
        /// </summary>
        public static void ConvertRtuDefn(FEP FepDB, GenericTable Xref)
        {
            Logger.OpenXMLLog();
            DbObject rtuDefnObj = FepDB.GetDbObject("RTU_DEFN");
            DbObject rtuDataObj = FepDB.GetDbObject("RTU_DATA");
            BescomParser.dnpnetcfgdevice_Tbl.Sort("station");
            int dataRec = rtuDataObj.RecordCount;
            int rtuDefnRec = 0;

            // Constant values from mapping.
            const int BINARY_INPUT = 1;
            const int BINARY_OUT = 2;
            const int ANALOG_IN = 4;
            const int ANALOG_OUT = 6;
            const int CNT_INT = 8;
            const int INT_TOTAL = 5;
            
            foreach (DataRow rtuRow in BescomParser.mastercfg_filteredTbl.Rows)
            {
                if (string.IsNullOrEmpty(rtuRow["rtu_address"].ToString())) continue;
                string fullName = rtuRow["station_name_32_characters"].ToString();
                int rtuadrs = Convert.ToInt32(rtuRow["rtu_address"].ToString());
                string name = GetRTUName(rtuadrs);
                
                int.TryParse(rtuRow["id"].ToString(), out int rtuRec);
                if (rtuRec == 0) continue;
                
                //if (string.IsNullOrEmpty(name)) continue;

                rtuDefnObj.CurrentRecordNo = rtuRec;// ++rtuDefnRec;

                ConcurrentDictionary<string, List<DataRow>> fepPointGroups = new ConcurrentDictionary<string, List<DataRow>>();
                Dictionary<string, int> feptypecount = new Dictionary<string, int>();
                if(rtuRec == 2)
                {
                    int i = 0;
                }
                var acsfep = new BescomFep(FepDB, Xref);
                acsfep.CountPointTypeGroups(rtuRec, ref fepPointGroups, ref feptypecount);
                int index = 0;
                int prevCount = 0;
                int nextAddr = 0;
                int checkcount = 0;
                int singleaddress = 0;
                int doubleaddress = 0;
                int measaddress = 0;

                
                rtuDefnObj.SetValue("PointType", index, BINARY_INPUT);
                rtuDefnObj.SetValue("Start", index, 1);
                feptypecount.TryGetValue("BIN_IN", out int binin);
                rtuDefnObj.SetValue("Count", index, binin);
                

                ++index;
               
                rtuDefnObj.SetValue("PointType", index, ANALOG_IN);
                rtuDefnObj.SetValue("Start", index, 1);
                feptypecount.TryGetValue("ANALG_IN", out int analogin);
                rtuDefnObj.SetValue("Count", index, analogin);


                ++index;

                rtuDefnObj.SetValue("PointType", index, BINARY_OUT);
                rtuDefnObj.SetValue("Start", index, 1);
                feptypecount.TryGetValue("BIN_OUT", out int binout);
                rtuDefnObj.SetValue("Count", index, binout);
                ++index;

                rtuDefnObj.SetValue("PointType", index, ANALOG_OUT);
                rtuDefnObj.SetValue("Start", index, 1);
                feptypecount.TryGetValue("ANALG_OUT", out int analogout);
                rtuDefnObj.SetValue("Count", index, analogout);

                ++index;

                rtuDefnObj.SetValue("PointType", index, CNT_INT);
                rtuDefnObj.SetValue("Start", index, 1);
                feptypecount.TryGetValue("CNTFROZEN", out int counter);
                rtuDefnObj.SetValue("Count", index, counter);

               



            }
            Logger.CloseXMLLog();
        }
            
            /// <summary>
            /// Function to convert SCANs
            /// </summary>
        public static void ConvertScanData(FEP FepDB, GenericTable Xref)
        {
            Logger.OpenXMLLog();
            Dictionary<string, string> ioakey = new Dictionary<string, string>();
            List<string> ioa = new List<string>();

            DbObject scanDataObj = FepDB.GetDbObject("SCAN_DATA");


            int newcount = scanDataObj.RecordCount;
            int scanDataRec;

            Xref.Sort("pRtu, Feptype, Point Address");


            for (scanDataRec = 1; scanDataRec <= scanDataObj.RecordCount; scanDataRec++)
           
            {
                
                scanDataObj.CurrentRecordNo = scanDataRec;
                
                
                string pRtu = scanDataObj.GetValue("pRTU", 0);
                string key = scanDataObj.GetValue("Key", 0);
                
                string protocolType = scanDataObj.GetValue("ProtocolType", 0);
                string addr = scanDataObj.GetValue("PointAddress", 0);
                string statetype_act = scanDataObj.GetValue("StateType", scanDataRec - 1, 0);
                string statetypeCurrent = scanDataObj.GetValue("StateType", 0);
                string destkey=scanDataObj.GetValue("DestinationKey",scanDataRec - 1, 0);   
                string compare = pRtu + "," + protocolType + "," + addr;
                if(key== "01001001")
                {
                    int i = 0;
                }
               if(statetype_act=="1")
                {
                    scanDataObj.SetValue("StateType", 0, 1);
                    scanDataObj.SetValue("StateOfs", 0, 1);
                    scanDataObj.SetValue("DestinationKey",0, destkey);
                }
                string objecttype = "STATUS";
                if (protocolType == "1" || protocolType == "2") objecttype = "STATUS";
                //if (protocolType == "4" || protocolType == "4") objecttype = "STATUS";
                else if (protocolType == "4") objecttype = "ANALOG";
                if (Xref.TryGetRow(new[] { pRtu, protocolType,  addr }, out DataRow xrefRow))
                {
                    string new1 = xrefRow["filename"].ToString();
                    if(new1.StartsWith("STSD")|| new1.StartsWith("STSP")|| new1.StartsWith("TLMP"))
                    {
                        continue;
                    }
                    scanDataObj.SetValue("DestinationKey", xrefRow["SCADA Key"]);
                    var kee = xrefRow["SCADA Key"].ToString();
                    if(key!=kee)
                    {
                        continue;
                    }
                    var pState = xrefRow["pState"].ToString();
                    if (kee == "010RY063")
                    {
                        int i = 0;
                    }
                    bool flag = false;  
                    if (pState == "406")
                    {
                        scanDataObj.SetValue("StateType", 0, 1);
                        scanDataObj.SetValue("StateOfs", 0, 0);
                       
                        flag = true;
                    }
                    else
                    {
                        scanDataObj.SetValue("StateType", 0, 0);
                        scanDataObj.SetValue("StateOfs", 0, 0);
                    }

                    scanDataObj.SetValue("DestinationRecordHint", xrefRow["OSI Record"]);
                    var rec = xrefRow["OSI Record"];
                 
                    scanDataObj.SetValue("DestinationDatabase", 0, "10");
                   
                    string feptype = xrefRow["Object"].ToString() == "STATUS" ? "1" : xrefRow["Object"].ToString() == "ANALOG" ? "3" : xrefRow["Object"].ToString() == "Accum" ? "5" : "1";
                    scanDataObj.SetValue("fepType", 0, feptype);
                    string obj = xrefRow["Object"].ToString() == "STATUS" ? "4" : xrefRow["Object"].ToString() == "ANALOG" ? "5" : xrefRow["Object"].ToString() == "Accum" ? "6" : "4";
                    scanDataObj.SetValue("DestinationObject", 0, obj);
                    string statetype = protocolType == "2" ? "1" : "0";
                
                }
                else
                {
                    Logger.Log("MISSING SCAN DATA LINK", LoggerLevel.INFO, $"Could not find scada point in xref with pRtu: {pRtu}\t protocolType: {protocolType}\t IOA: {addr}");
                }
            }
            Logger.CloseXMLLog();
        }

       
    }
}
