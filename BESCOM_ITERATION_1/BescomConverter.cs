using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using OSII.DatabaseConversionToolkit;
using OSII.ConversionToolkit;
using OSII.ConversionToolkit.Generic;
using OSII.DatabaseToolkit.Dat;
using System.Drawing;
using OSII.ConversionToolkit.Extensions;
using System.Diagnostics;


namespace AESES_Databases
{
    class BescomConverter
    {
        public SCADA ScadaDB = new SCADA();
        public STATES StatesDB = new STATES();
        public FEP FepDB = new FEP();
        public ICCP ICCPDB = new ICCP();
        public OpenCalc OpenCalcDB = new OpenCalc();


        static readonly string[] AlarmXrefFields = { "OSI Record", "AlarmKey", "Alarm Class", "Alarm Priority", "Bell Pattern" };
        static GenericTable AlarmXref = new GenericTable(AlarmXrefFields, "OSI Record");

        static readonly string[] AorXrefFields = { "OSI Record", "BitKey", "Bit0", "Bit1", "Bit2", "Bit60" };
        static GenericTable AorXref = new GenericTable(AorXrefFields, "OSI Record");

        static readonly string[] XrefFields = { "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record", "pRTU", "Feptype", "StsrRecord", "CtlRecord", "AESRec", "SCADA Name","station" ,"pState"};
        static GenericTable Xref = new GenericTable(XrefFields, "OSI Record");

        private static Dictionary<string, int> _uniqueAORs;
        private static Dictionary<string, int> _uniqueAlarms;
        private static Dictionary<string, string> _StationDict=new Dictionary<string, string>();
        public static Dictionary<float, int> _scaleDict = new Dictionary<float, int>();
        public static List<string> _nameList = new List<string>();

        

        public BescomConverter()
        {
            ConvertAorGroups();
            ConvertStations();
            ConvertUnts();
            ConvertScales();
            BescomStates.ConvertStates2(StatesDB);
            ConvertStates();
            BescomFep.DictRTUChannelChannelGroup();
           BescomFep.ConvertRtuData(FepDB);
           BescomFep.ConvertChannelGroup(FepDB);
            BescomFep.ConvertChannels(FepDB);
            BescomFep.ConvertRtuControl(FepDB, Xref);
            BescomFep rtuDefn = new BescomFep(FepDB, Xref);
            BescomFep.ConvertRtuDefn(FepDB, Xref);
            BescomICCP.ConvertControlCenterInfo(ICCPDB);
            
            #region FEPCHECK
            //FEPCHECK
            try
            { 

                DbUtilities.DbClear(10);
                DbUtilities.DbClear(32);
                DbUtilities.WriteAndPopulate(StatesDB);
                ScadaDB.PopulateAndScadaValidate();
                FepDB.PopulateAndFepBuild(false, true, false, true);
                          
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            #endregion
            BescomFep.ConvertScanData(FepDB, Xref);
            // AcsPrismFep.ConvertScanDefn();

            BescomOpenCalc.ConvertFormTemplate(OpenCalcDB);
            BescomOpenCalc.ConvertFormula(OpenCalcDB, Xref);
            BescomOpenCalc.ConvertTimer(OpenCalcDB);
            BescomOpenCalc.ConvertEXxecutionGrp(OpenCalcDB);
            //
            //ScadaDB.PopulateAndScadaValidate();
            //FEPCHECK
            try
            {
                FepDB.PopulateAndFepBuild(false, true, true, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            //FepDB.FinalizeFepAndCheck(ScadaDB, false);
        }

       
        public void ConvertAorGroups()
        {
            Logger.OpenXMLLog("ConvertAorGroups");
            DbObject aorObj = ScadaDB.GetDbObject("AOR_GROUP");
            int aorRec = 1;

            //aorObj.CurrentRecordNo = aorRec++;
            //aorObj.SetValue("Name", "Administrator");
            //aorObj.SetValue("AORList", 1);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "HSR");
            aorObj.SetValue("AORList", 1);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "JAYANAGAR");
            aorObj.SetValue("AORList", 2);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "KORAMANGALA");
            aorObj.SetValue("AORList", 3);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "INDIRANAGAR");
            aorObj.SetValue("AORList", 4);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "SHIVAJINAGAR");
            aorObj.SetValue("AORList", 5);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "VIDHANASOUDHA");
            aorObj.SetValue("AORList", 6);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "RAJAJINAGAR");
            aorObj.SetValue("AORList", 7);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "RAJARAJESHWARINAGARA");
            aorObj.SetValue("AORList", 8);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "KENGERI");
            aorObj.SetValue("AORList", 9);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "PEENYA");
            aorObj.SetValue("AORList", 10);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "MALLESHWARAM");
            aorObj.SetValue("AORList", 11);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "HEBBAL");
            aorObj.SetValue("AORList", 12);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "JALAHALLI");
            aorObj.SetValue("AORList", 13);

            aorObj.CurrentRecordNo = aorRec++;
            aorObj.SetValue("Name", "WHITEFIELD");
            aorObj.SetValue("AORList", 14);

            Logger.CloseXMLLog();

        }
      
        public void ConvertScales()
        {
            Logger.OpenXMLLog("ConvertScales");
            DbObject scaleObj = ScadaDB.GetDbObject("SCALE");
            int scaleRec = 0;
            foreach (BescomFile file in BescomParser.scfSet.Files.Values)
            {
                foreach (BescomRecord record in file.ACSRecords)
                {
                   
                    double divisor = 2047;
                    scaleObj.CurrentRecordNo = ++scaleRec;
                    scaleObj.SetValue("Offset", record.InfoDict["zero_scale_value"]);
                    if (record.InfoDict["divisor"] != "0") divisor = Convert.ToDouble(record.InfoDict["divisor"]);
                    double scale_factor = Convert.ToDouble(record.InfoDict["full_scale_value"]) / divisor;

                    scaleObj.SetValue("Scale", scale_factor);
                    scaleObj.SetValue("Name", (record.InfoDict["full_scale_value"]));
                }
            }
            Logger.CloseXMLLog();

        }
       
        public void ConvertUnts()
        {
            Logger.OpenXMLLog("ConvertUnits");
            DbObject unitObj = ScadaDB.GetDbObject("UNITS");
            int unitRec = 1;

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "AMP");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "VOLTS");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "KW");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "KVAR");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "ERG");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "MW");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "MVAR");

            unitObj.CurrentRecordNo = unitRec++;
            unitObj.SetValue("Name", "MWH");

            Logger.CloseXMLLog();

        }
        
       
        public void ConvertStations()
        {
            Logger.OpenXMLLog("ConvertStations");
            DbObject stationObj = ScadaDB.GetDbObject("STATION");
            int stationRec = 0;
            foreach (DataRow stationRow in BescomParser.mastercfg_filteredTbl.Rows)
            {

                string fullName = stationRow["station_name_32_characters"].ToString(); 
                string stationKey = stationRow["station_name_32_characters"].ToString();
                string Aorgroup = stationRow["AOR_Group"].ToString();
                int.TryParse(stationRow["station_number"].ToString(), out stationRec);
               
                if (stationRec == 0) continue;
                stationObj.CurrentRecordNo = stationRec;
                stationObj.SetValue("Key", stationKey);
                stationObj.SetValue("Name", fullName);
                stationObj.SetValue("pAORGroup", Aorgroup);
                if (!_StationDict.ContainsKey(fullName))
                {
                    _StationDict.Add(stationRec.ToString(), fullName);
                }
                if (stationKey.Length > 16) Logger.Log("StationKey_Truncation", LoggerLevel.INFO, $"Name exceeded 16 characters, truncated : {stationKey }"); //BD
            }
            foreach (DataRow stationRow in BescomParser.Substation_names.Rows)
            {

                string fullName = stationRow["Substation_name"].ToString();
                stationRec++;
                stationObj.CurrentRecordNo = stationRec;
                stationObj.SetValue("Key", fullName);
                stationObj.SetValue("Name", fullName);
                stationObj.SetValue("pAORGroup", 1);
                string dispnum = stationRow["Display_no."].ToString();
                if (!_StationDict.ContainsKey(fullName))
                {
                    _StationDict.Add(stationRec.ToString(), fullName);
                }
                if (fullName.Length > 16) Logger.Log("StationKey_Truncation", LoggerLevel.INFO, $"Name exceeded 16 characters, truncated : {fullName}"); //BD
            }


            Logger.CloseXMLLog();
        }



        public void ConvertStates()
        {
            Logger.OpenXMLLog("ConvertStates");
            DbObject statusObj = ScadaDB.GetDbObject("STATUS");
            DbObject statesObj = StatesDB.GetDbObject("STATE");
            int statusRec = 0;

            BescomParser.mastercfg_filteredTbl.Sort("rtu_address");

            _uniqueAlarms = new Dictionary<string, int>();
            _uniqueAORs = new Dictionary<string, int>();

            #region STSCFileSet
            //foreach (BescomFile stscFile in BescomParser.stscSet.Files.Values)
            //{
            //    int filenum = Convert.ToInt32(stscFile.Name.Split('.')[0].Replace("STSC", ""));
            //    DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
            //    if (stationRow == null)
            //    {
            //        Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stscFile.Name }"); //BD
            //        continue;

            //    }
            //    foreach (BescomRecord stscRecord in stscFile.ACSRecords)
            //    {
            //        string name = "";
            //        if (stscRecord.InfoDict.Keys.Contains("description"))
            //        {
            //            name = stscRecord.InfoDict["point description"];
            //            if (name == "") { name = "--"; }   
            //        }


            //        if (stscRecord.Record.Equals(0))
            //        {
            //            if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in STSC contains information. Point Description: {name}");
            //            continue;
            //        }
            //        int stationNumber = Convert.ToInt32(stscFile.Name.Split('.')[0].Replace("STSC", ""));

            //        if (stationNumber == 0) continue;
            //        statusObj.CurrentRecordNo = ++statusRec;
            //        statusObj.SetValue("Name", name);
            //        if(name=="--") { statusObj.SetValue("AppID", 2); }

            //        string type = "5";
            //        statusObj.SetValue("Type", type);

            //       // int pStation = Convert.ToInt32(statusObj.GetValue("pStation", 0));
            //        int z = stscRecord.Record;
            //        // int y = pStation;
            //        int y = stationNumber;
            //        statusObj.SetValue("Key", ScadaDB.GetNextKey(2, y));// KeyGen(2, y, z + 1));  BD
            //        statusObj.SetValue("pALARM_GROUP", "1");
            //        SetArchiveGroup(statusObj); 

            //        int pState = BescomStates.SetStates2(stscRecord);

            //        statusObj.SetValue("pStates", pState);
            //        statusObj.SetValue("State", 1);


            //        int stationid = Convert.ToInt32(stscFile.Name.Split('.')[0].Replace("STSC", ""));
            //        statusObj.SetValue("pAORGroup", SetAor2(stscRecord, stationid));
            //        //Station
            //        statusObj.SetValue("pStation", SetpStation2(stscRecord, stationid));
            //        SetAlarmGroups(statusObj, stscRecord); 
            //        SetArchiveGroup(statusObj);

            //        if (stscRecord.InfoDict["abnormal_list"].Equals("1") && stscRecord.InfoDict["abnormal_definition"].Equals("A")) // RM for mapping jully2023 
            //        {
            //            string abnlst = stscRecord.InfoDict["abnormal_state"];
            //            if (abnlst == "1") { abnlst = "0"; }

            //            else { abnlst = "1"; }

            //            statusObj.SetValue("ConfigNormalState", abnlst);
            //        }
            //        else statusObj.SetValue("ConfigNormalState", "-1");

            //        if (stscRecord.InfoDict["open_state_indicator"].Equals("1"))
            //            statusObj.SetValue("StateCalc", "1");
            //        else
            //            statusObj.SetValue("StateCalc", "0");
            //        string typec = statusObj.GetValue("Type", 0).ToString();
            //        string feptype = typec == "2" ? "1" : typec == "1" ? "1" : "0";
            //        object[] xrefObjects = { stscFile.Name, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), stscRecord.Record.ToString(), "N", statusObj.CurrentRecordNo.ToString(), statusObj.GetValue("pRTU", 0), feptype ,statusObj.GetValue("Name",0)};
            //        //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record", "pRTU" };//STSC
            //        Xref.AddRecordSetValues(xrefObjects);
            //    }
            //}

            #endregion
            #region STSRFileSet
            foreach (BescomFile stsrFile in BescomParser.stsrSet.Files.Values)
            {
                string name12 = "";
                int filenum = Convert.ToInt32(stsrFile.Name.Split('.')[0].Replace("STSR", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });

                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsrFile.Name }"); //BD
                    continue;

                }
                if (stsrFile.ACSRecords == null)
                {
                    Logger.Log("Empty File", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsrFile.Name }"); //BD
                    continue;
                }
                foreach (BescomRecord stsrRecord in stsrFile.ACSRecords)
                {
                   
                    string name = stsrRecord.InfoDict["point_description"];
                    int stationid = Convert.ToInt32(stsrFile.Name.Split('.')[0].Replace("STSR", ""));
                    if (stsrRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in STSR contains information. Point Description: {name}");
                        continue;
                    }
                   
                    if (stsrRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = stsrRecord.InfoDict["point_description"];
                        if (name == "") { name = "--"; }
                   
                    }
                 
                        string type = "0";
                    string scdakey = string.Empty;
                    int pRTU = GetpRTU(stsrRecord, stationid);
                    
                    bool flag = false;
                    if(name== "MECHANICAL LEVER")
                    {
                        flag = true;
                    }
                    if (name12 == name&&flag==false)
                    {
                        scdakey = ScadaDB.GetNextKey(1, pRTU);
                        continue;
                    }
                    
                    int stationNumber = Convert.ToInt32(stsrFile.Name.Split('.')[0].Replace("STSR", ""));
                    if (stationNumber == 0) continue;
                   
                    
                    statusObj.CurrentRecordNo = ++statusRec;

                    statusObj.SetValue("pRTU", pRTU);
                    statusObj.SetValue("Name", name);
                    name12 = statusObj.GetValue("Name",statusRec,0);
                    if (name == "--")
                    {
                        statusObj.SetValue("AppID", 2);
                    }
                    string station = stsrFile.Name.Split('.')[0].Replace("STSR", "CTL");
                    int StsrRecNo = stsrRecord.Record;
                   
                    statusObj.SetValue("pAORGroup", SetAor2(stsrRecord, stationid)); 
                    statusObj.SetValue("pStation", SetpStation2(stsrRecord, stationid));
                    SetAlarmGroups(statusObj, stsrRecord); 
                    int CtlRecNo = 0;
                    bool ctl = false;
                    string ctlfilename = "";
                    string ctlname = "";
                    if (BescomParser.ctlSet.Files.Keys.Contains(station))
                    {
                        foreach (BescomRecord ctlRecord in BescomParser.ctlSet.Files[station].ACSRecords)
                        {
                            if (ctlRecord.InfoDict["description"] == "FPI RESET")
                            {
                                continue;
                            }
                            if (ctlRecord.InfoDict["solicited_station"].Equals(stationNumber.ToString()) && name == ctlRecord.InfoDict["description"]
                                || ctlRecord.InfoDict["solicited_point"].Equals(stsrRecord.Record.ToString()))
                                //&&ctlRecord.InfoDict["solicited_point"].Equals(stsrRecord.Record.ToString()))
                            {
                                string PrevName= statusObj.GetValue("Name", statusRec-1, 0);
                                if(PrevName == ctlRecord.InfoDict["description"])
                                {
                                    continue;
                                }
                                if (!ctlRecord.InfoDict["solicited_station"].Equals("0"))
                                {
                                    statusObj.SetValue("Type", 2);
                                    type = "2";
                                    CtlRecNo = ctlRecord.Record;
                                    ctl = true;
                                    ctlfilename = BescomParser.ctlSet.Files[station].Name;

                                    break;
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(name)) Logger.Log("CTL_sol_stn", LoggerLevel.INFO, $"solicited_station is 0 file:{BescomParser.ctlSet.Files[station].Name},Point Description: {name} and record_number:{ctlRecord.Record.ToString() }");
                                    break;

                                }

                            }

                        }
                    }
                    if (!type.Equals("2"))
                    {
                        statusObj.SetValue("Type", "1");
                        type = "1";
                    }
                   
                    SetArchiveGroup(statusObj);

                    if (stationid != 0)
                        try
                        {
                            statusObj.SetValue("Key", ScadaDB.GetNextKey(1, pRTU));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }


                    //STATES                     
                    List<string> stateMessages = new List<string>();
                    SetArchiveGroup(statusObj); //
                    if (SetStates(stsrRecord) != null)
                    {
                        stateMessages.AddRange(SetStates(stsrRecord));
                    }
                    else
                    {
                        continue;
                    }

                    
                    int pState = BescomStates.SetStates2(stsrRecord);
                    statusObj.SetValue("pStates", pState);
                    if (pState == 327)
                    {
                        pState = 205;
                        statusObj.SetValue("pStates", pState);
                    }
                    if (stsrRecord.InfoDict["abnormal_list"].Equals("1") && stsrRecord.InfoDict["abnormal_definition"].Equals("A")) // RM for mapping jully2023 
                    {
                        string abnlst = stsrRecord.InfoDict["abnormal_state"];

                        if(abnlst=="1")
                        {
                            abnlst = "0";
                        }
                        else 
                        {
                            abnlst = "1";
                        }
                        statusObj.SetValue("ConfigNormalState", abnlst);
                    }
                    // else statusObj.SetValue("ConfigNormalState", "-1");
                    else statusObj.SetValue("ConfigNormalState", "1");

                    string pst = statusObj.GetValue("pStation",0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    
                    if (stsrRecord.InfoDict["open_state_indicator"].Equals("1"))
                        statusObj.SetValue("StateCalc", "1");
                    else
                        statusObj.SetValue("StateCalc", "0");
                    string typec = statusObj.GetValue("Type", 0).ToString();
                    string feptype = typec == "2" ? "1" : typec == "1" ? "1" : "0";
                    string filename = ctl == true ? ctlfilename : stsrFile.Name;
                    object[] xrefObjects = { filename, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), stsrRecord.Record.ToString(), "R", statusObj.CurrentRecordNo.ToString(), pRTU, feptype, StsrRecNo, CtlRecNo ,"", statusObj.GetValue("Name", 0),value, pState }; //statusObj.GetValue("Name", 0) FEP RTU_CONTROL name (from ctl files) statusObj.GetValue("pRTU", 0)
                    //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record" };//STSR
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion

            #region CTLFPIinput
            foreach (BescomFile ctlFile in BescomParser.ctlSet.Files.Values)
            {
                
                int filenum = Convert.ToInt32(ctlFile.Name.Split('.')[0].Replace("CTL", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                statusObj.CurrentRecordNo = statusRec++;
                int type = 3;
                foreach (BescomRecord ctlRecord in ctlFile.ACSRecords)
                {

                    string name = ctlRecord.InfoDict["description"];
                    
                    if(name.StartsWith("FPI"))
                    {
                       
                        statusObj.SetValue("Name", name);
                        
                        statusObj.SetValue("Type", type);
                        statusObj.SetValue("pAORGroup", SetAor2(ctlRecord, filenum));
                        statusObj.SetValue("pStation", SetpStation2(ctlRecord, filenum));
                        statusObj.SetValue("pALARM_GROUP", 2);
                        int pRTU = GetpRTU(ctlRecord, filenum);
                        
                        statusObj.SetValue("pRTU", pRTU);
                        if (filenum != 0)
                            try
                            {
                                statusObj.SetValue("Key", ScadaDB.GetNextKey(1, pRTU));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                        //STATES                     
                        List<string> stateMessages = new List<string>();
                        SetArchiveGroup(statusObj); //
                        if (SetStates(ctlRecord) != null)
                        {
                            stateMessages.AddRange(SetStates(ctlRecord));
                        }
                        else
                        {
                            continue;
                        }

                        int pState = 223;
                        //int pState = BescomStates.SetStates2(ctlRecord);
                        statusObj.SetValue("pStates", pState);
                        string pst = statusObj.GetValue("pStation", 0);
                        string value = string.Empty;
                        _StationDict.TryGetValue(pst, out value);
                        string filename = ctlFile.Name;
                        int feptype = 3;
                        object[] xrefObjects = { filename, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), ctlRecord.Record.ToString(), "R", statusObj.CurrentRecordNo.ToString(), pRTU, feptype,"","", "", statusObj.GetValue("Name", 0) , value, pState }; //statusObj.GetValue("Name", 0) FEP RTU_CONTROL name (from ctl files) statusObj.GetValue("pRTU", 0)
                                                                                                                                                                                                                                                                                                                                 //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record" };//STSR
                        Xref.AddRecordSetValues(xrefObjects);

                    }
                   
                    // int stationid = Convert.ToInt32(ctlFile.Name.Split('.')[0].Replace("CTL", ""));
                }

            }
                    #endregion

            #region STSDSet
           foreach (BescomFile stsdFile in BescomParser.stsdSet.Files.Values)
            {
                string name12 = "";
                int filenum = Convert.ToInt32(stsdFile.Name.Split('.')[0].Replace("STSD", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsdFile.Name }"); //BD
                    continue;

                }
                if (stsdFile.ACSRecords == null)
                {
                    Logger.Log("Empty File", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsdFile.Name }"); //BD
                    continue;
                }
                foreach (BescomRecord stsdRecord in stsdFile.ACSRecords)
                {

                    string name = stsdRecord.InfoDict["point_description"];
                    int stationid = Convert.ToInt32(stsdFile.Name.Split('.')[0].Replace("STSD", ""));
                    if (stsdRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in STSR contains information. Point Description: {name}");
                        continue;
                    }

                    if (stsdRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = stsdRecord.InfoDict["point_description"];
                        if (name == "") { name = "--"; }

                    }

                    string type = "0";

                    if (name12 == name) continue;
                    if (_nameList.Contains(name)) continue;
                    int stationNumber = Convert.ToInt32(stsdFile.Name.Split('.')[0].Replace("STSD", ""));
                    if (stationNumber == 0) continue;
                    statusObj.CurrentRecordNo = ++statusRec;

                    statusObj.SetValue("Name", name);
                    name12 = statusObj.GetValue("Name", statusRec, 0);
                    if (name == "--")
                    {
                        statusObj.SetValue("AppID", 2);
                    }
                    
                    statusObj.SetValue("pAORGroup", SetAor2(stsdRecord, stationid));
                    statusObj.SetValue("pStation", SetpStation2(stsdRecord, stationid));
                    SetAlarmGroups(statusObj, stsdRecord);
                    
                    statusObj.SetValue("Type", "1");
                       
                    
                    int pRTU = GetpRTU(stsdRecord, stationid);
                    statusObj.SetValue("pRTU", pRTU);

                    SetArchiveGroup(statusObj);
                    if (stationid != 0)
                        try
                        {
                            statusObj.SetValue("Key", ScadaDB.GetNextKey(1, pRTU));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    //STATES                     
                    List<string> stateMessages = new List<string>();
                    SetArchiveGroup(statusObj); //
                    if (SetStates(stsdRecord) != null)
                    {
                        stateMessages.AddRange(SetStates(stsdRecord));
                    }
                    else
                    {
                        continue;
                    }


                    int pState = BescomStates.SetStates2(stsdRecord);
                    statusObj.SetValue("pStates", pState);

                    if(pState ==327)

                    {
                        pState = 205;
                        statusObj.SetValue("pStates", pState);
                    }
                    if (stsdRecord.InfoDict["abnormal_list"].Equals("1") && stsdRecord.InfoDict["abnormal_definition"].Equals("A")) // RM for mapping jully2023 
                    {
                        string abnlst = stsdRecord.InfoDict["abnormal_state"];

                        if (abnlst == "1")
                        {
                            abnlst = "0";
                        }
                        else
                        {
                            abnlst = "1";
                        }
                        statusObj.SetValue("ConfigNormalState", abnlst);
                    }
                    else statusObj.SetValue("ConfigNormalState", "-1");

                    if (stsdRecord.InfoDict["open_state_indicator"].Equals("1"))
                        statusObj.SetValue("StateCalc", "1");
                    else
                        statusObj.SetValue("StateCalc", "0");
                    string pst = statusObj.GetValue("pStation", 0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    string typec = statusObj.GetValue("Type", 0).ToString();
                    string feptype = typec == "2" ? "1" : typec == "1" ? "1" : "0";
                    object[] xrefObjects = { stsdFile.Name, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), stsdRecord.Record.ToString(), "D", statusObj.CurrentRecordNo.ToString(), statusObj.GetValue("pRTU", 0), feptype, stsdRecord.Record , statusObj.GetValue("Name", 0),value, pState }; //statusObj.GetValue("Name", 0) FEP RTU_CONTROL name (from ctl files)
                    //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record" };//STSR
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion
            #region STSIFileSet
            //foreach (BescomFile stsiFile in BescomParser.stsiSet.Files.Values)
            //{
            //    string name12 = "";
            //    int filenum = Convert.ToInt32(stsiFile.Name.Split('.')[0].Replace("STSI", ""));
            //    DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
            //    if (stationRow == null)
            //    {
            //        Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsiFile.Name }"); //BD
            //        continue;

            //    }
            //    if (stsiFile.ACSRecords == null)
            //    {
            //        Logger.Log("Empty File", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsiFile.Name }"); //BD
            //        continue;
            //    }
            //    foreach (BescomRecord stsiRecord in stsiFile.ACSRecords)
            //    {

            //        string name = stsiRecord.InfoDict["point_description"];
            //        int stationid = Convert.ToInt32(stsiFile.Name.Split('.')[0].Replace("STSI", ""));
            //        if (stsiRecord.Record.Equals(0))
            //        {
            //            if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in STSR contains information. Point Description: {name}");
            //            continue;
            //        }

            //        if (stsiRecord.InfoDict.Keys.Contains("point_description"))
            //        {
            //            name = stsiRecord.InfoDict["point_description"];
            //            if (name == "") { name = "--"; }

            //        }

            //        string type = "0";

            //        if (name12 == name) continue;
            //        if (_nameList.Contains(name)) continue;
            //        int stationNumber = Convert.ToInt32(stsiFile.Name.Split('.')[0].Replace("STSI", ""));
            //        if (stationNumber == 0) continue;
            //        statusObj.CurrentRecordNo = ++statusRec;

            //        statusObj.SetValue("Name", name);
            //        name12 = statusObj.GetValue("Name", statusRec, 0);
            //        if (name == "--")
            //        {
            //            statusObj.SetValue("AppID", 2);
            //        }

            //        statusObj.SetValue("pAORGroup", SetAor2(stsiRecord, stationid));
            //        statusObj.SetValue("pStation", SetpStation2(stsiRecord, stationid));
            //        SetAlarmGroups(statusObj, stsiRecord);

            //        statusObj.SetValue("Type", "1");


            //        int pRTU = GetpRTU(stsiRecord, stationid);
            //        statusObj.SetValue("pRTU", pRTU);

            //        SetArchiveGroup(statusObj);
            //        if (stationid != 0)
            //            try
            //            {
            //                statusObj.SetValue("Key", ScadaDB.GetNextKey(1, pRTU));
            //            }
            //            catch (Exception ex)
            //            {
            //                Console.WriteLine(ex.Message);
            //            }

            //        //STATES                     
            //        List<string> stateMessages = new List<string>();
            //        SetArchiveGroup(statusObj); //
            //        if (SetStates(stsiRecord) != null)
            //        {
            //            stateMessages.AddRange(SetStates(stsiRecord));
            //        }
            //        else
            //        {
            //            continue;
            //        }


            //        int pState = BescomStates.SetStates2(stsiRecord);
            //        statusObj.SetValue("pStates", pState);

            //        if (stsiRecord.InfoDict["abnormal_list"].Equals("1") && stsiRecord.InfoDict["abnormal_definition"].Equals("A")) // RM for mapping jully2023 
            //        {
            //            string abnlst = stsiRecord.InfoDict["abnormal_state"];

            //            if (abnlst == "1")
            //            {
            //                abnlst = "0";
            //            }
            //            else
            //            {
            //                abnlst = "1";
            //            }
            //            statusObj.SetValue("ConfigNormalState", abnlst);
            //        }
            //        else statusObj.SetValue("ConfigNormalState", "-1");

            //        if (stsiRecord.InfoDict["open_state_indicator"].Equals("1"))
            //            statusObj.SetValue("StateCalc", "1");
            //        else
            //            statusObj.SetValue("StateCalc", "0");
            //        string typec = statusObj.GetValue("Type", 0).ToString();
            //        string feptype = typec == "2" ? "1" : typec == "1" ? "1" : "0";
            //        object[] xrefObjects = { stsiFile.Name, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), stsiRecord.Record.ToString(), "I", statusObj.CurrentRecordNo.ToString(), statusObj.GetValue("pRTU", 0), feptype, stsiRecord.Record, statusObj.GetValue("Name", 0) }; //statusObj.GetValue("Name", 0) FEP RTU_CONTROL name (from ctl files)
            //        //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record" };//STSR
            //        Xref.AddRecordSetValues(xrefObjects);
            //    }
            //}
            #endregion
            #region STSPFileSet
            foreach (BescomFile stspFile in BescomParser.stspSet.Files.Values) //M_IND
            {
                int filenum = Convert.ToInt32(stspFile.Name.Split('.')[0].Replace("STSP", ""));
                if(filenum==3224)
                {
                    int i = 0;
                }
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stspFile.Name }"); //BD
                    continue;
                }
                foreach (BescomRecord stspRecord in stspFile.ACSRecords)
                {
                    string name = "";

                    if (stspRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = stspRecord.InfoDict["point_description"];
                        if(name=="")
                        {
                            name = "--";
                        }
                    }
                    
                    if (stspRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in STSP contains information. Point Description: {name}");
                        continue;
                    }
                    int stationNumber = Convert.ToInt32(stspFile.Name.Split('.')[0].Replace("STSP", ""));
                    if (stationNumber == 0)
                        continue;

                   statusObj.CurrentRecordNo = ++statusRec;
                    statusObj.SetValue("Name", name);
                    if (name == "--")
                    {
                        statusObj.SetValue("AppID", 2);
                    }
                    string type = "8";
                    statusObj.SetValue("Type", type);
                    SetArchiveGroup(statusObj);
                    int pRTU = GetpRTU(stspRecord, filenum);
                    statusObj.SetValue("pRTU", pRTU);

                    int pState = BescomStates.SetStates2(stspRecord);

                    statusObj.SetValue("pStates", pState);
                    if(pState == 327)
                    {
                        pState = 205;
                        statusObj.SetValue("pStates", pState);
                    }
                    statusObj.SetValue("State", 1);
                                      
                    int stationid = Convert.ToInt32(stspFile.Name.Split('.')[0].Replace("STSP", ""));
                  
                    statusObj.SetValue("pAORGroup", SetAor2(stspRecord, stationid)); 
                   
                    statusObj.SetValue("pStation", SetpStation2(stspRecord, stationid)); 
                   
                    SetAlarmGroups(statusObj, stspRecord); 

                    int pStation = Convert.ToInt32(statusObj.GetValue("pStation", 0));// stscSet
                    int z = stspRecord.Record;
                    int y = pStation;// stationNumber;
                    statusObj.SetValue("Key", ScadaDB.GetNextKey(12, y));// KeyGen(8, y, z + 1));

              
                    if (stspRecord.InfoDict["abnormal_list"].Equals("1") && stspRecord.InfoDict["abnormal_definition"].Equals("A")) // RM for mapping jully2023 
                    {
                        string abnlst = stspRecord.InfoDict["abnormal_state"];
                        if (abnlst == "1")
                        {
                            abnlst = "0";
                        }
                        else 
                        {
                            abnlst = "1";
                        }
                        statusObj.SetValue("ConfigNormalState", abnlst);
                    }
                    else statusObj.SetValue("ConfigNormalState", "-1");

                    if (stspRecord.InfoDict["open_state_indicator"].Equals("1"))
                        statusObj.SetValue("StateCalc", "1");
                    else
                        statusObj.SetValue("StateCalc", "0");
                    string pst = statusObj.GetValue("pStation", 0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    string typec = statusObj.GetValue("Type", 0).ToString();
                    string feptype = typec == "2" ? "1" : typec == "1" ? "1" : "0";
                    object[] xrefObjects = { stspFile.Name, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), stspRecord.Record.ToString(), "P", statusObj.CurrentRecordNo.ToString(), statusObj.GetValue("pRTU", 0), feptype , statusObj.GetValue("Name", 0),value, pState };
                    //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record" };//stsp
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion

            //SUBSTATION STSR 

            #region SUBSTATIONSTSRFileSet
            foreach (BescomFile sstsrFile in BescomParser.sstsr.Files.Values)
            {
                string name12 = "";
                int filenum = Convert.ToInt32(sstsrFile.Name.Split('.')[0].Replace("STSR", ""));
                DataRow stationRow = BescomParser.Substation_names.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {sstsrFile.Name}"); //BD
                    continue;

                }
                if (sstsrFile.ACSRecords == null)
                {
                    Logger.Log("Empty File", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {sstsrFile.Name}"); //BD
                    continue;
                }
                foreach (BescomRecord sstsrRecord in sstsrFile.ACSRecords)
                {

                    string name = sstsrRecord.InfoDict["point_description"];
                    int stationid = Convert.ToInt32(sstsrFile.Name.Split('.')[0].Replace("STSR", ""));
                    if (sstsrRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in STSR contains information. Point Description: {name}");
                        continue;
                    }

                    if (sstsrRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = sstsrRecord.InfoDict["point_description"];
                        if (name == "") { name = "--"; }

                    }

                    string type = "0";
                    string scdakey = string.Empty;
                    int pRTU = GetpRTU(sstsrRecord, stationid);
                    if (name12 == name)
                    {
                        scdakey = ScadaDB.GetNextKey(1, stationid);
                        continue;
                    }

                    int stationNumber = Convert.ToInt32(sstsrFile.Name.Split('.')[0].Replace("STSR", ""));
                    if (stationNumber == 0) continue;


                    statusObj.CurrentRecordNo = ++statusRec;


                    statusObj.SetValue("Name", name);
                    name12 = statusObj.GetValue("Name", statusRec, 0);

                    string station = sstsrFile.Name.Split('.')[0].Replace("STSR", "CTL");
                    int StsrRecNo = sstsrRecord.Record;

                    statusObj.SetValue("pAORGroup", SetAor2(sstsrRecord, stationid));
                    statusObj.SetValue("pStation", SetpStation2(sstsrRecord, stationid));
                    SetAlarmGroups(statusObj, sstsrRecord);
                    int CtlRecNo = 0;
                    bool ctl = false;
                    string ctlfilename = "";
                    string ctlname = "";
                    if (BescomParser.sctl.Files.Keys.Contains(station))
                    {
                        foreach (BescomRecord ctlRecord in BescomParser.sctl.Files[station].ACSRecords)
                        {

                            if (ctlRecord.InfoDict["solicited_station"].Equals(stationNumber.ToString()) && name == ctlRecord.InfoDict["description"]
                                || ctlRecord.InfoDict["solicited_point"].Equals(sstsrRecord.Record.ToString()))
                            //&&ctlRecord.InfoDict["solicited_point"].Equals(stsrRecord.Record.ToString()))
                            {
                                string PrevName = statusObj.GetValue("Name", statusRec - 1, 0);
                                if (PrevName == ctlRecord.InfoDict["description"])
                                {
                                    continue;
                                }
                                if (!ctlRecord.InfoDict["solicited_station"].Equals("0"))
                                {
                                    statusObj.SetValue("Type", 2);
                                    type = "2";
                                    CtlRecNo = ctlRecord.Record;
                                    ctl = true;
                                    ctlfilename = BescomParser.sctl.Files[station].Name;

                                    break;
                                }
                                else
                                {
                                    if (!String.IsNullOrEmpty(name)) Logger.Log("CTL_sol_stn", LoggerLevel.INFO, $"solicited_station is 0 file:{BescomParser.ctlSet.Files[station].Name},Point Description: {name} and record_number:{ctlRecord.Record.ToString()}");
                                    break;

                                }

                            }

                        }
                    }
                    if (!type.Equals("2"))
                    {
                        statusObj.SetValue("Type", "1");
                        type = "1";
                    }

                    SetArchiveGroup(statusObj);

                    if (stationid != 0)
                        try
                        {
                            statusObj.SetValue("Key", ScadaDB.GetNextKey(1, stationid));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }


                    //STATES                     
                    List<string> stateMessages = new List<string>();
                    SetArchiveGroup(statusObj); //
                    if (SetStates(sstsrRecord) != null)
                    {
                        stateMessages.AddRange(SetStates(sstsrRecord));
                    }
                    else
                    {
                        continue;
                    }


                    int pState = BescomStates.SetStates2(sstsrRecord);
                    statusObj.SetValue("pStates", pState);

                    if (sstsrRecord.InfoDict["abnormal_list"].Equals("1") && sstsrRecord.InfoDict["abnormal_definition"].Equals("A")) // RM for mapping jully2023 
                    {
                        string abnlst = sstsrRecord.InfoDict["abnormal_state"];

                        if (abnlst == "1")
                        {
                            abnlst = "0";
                        }
                        else
                        {
                            abnlst = "1";
                        }
                        statusObj.SetValue("ConfigNormalState", abnlst);
                    }
                    else statusObj.SetValue("ConfigNormalState", "-1");

                    if (sstsrRecord.InfoDict["open_state_indicator"].Equals("1"))
                        statusObj.SetValue("StateCalc", "1");
                    else
                        statusObj.SetValue("StateCalc", "0");
                    string pst = statusObj.GetValue("pStation", 0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    string typec = statusObj.GetValue("Type", 0).ToString();
                    string feptype = typec == "2" ? "1" : typec == "1" ? "1" : "0";
                    string filename = ctl == true ? ctlfilename : sstsrFile.Name;
                    //object[] xrefObjects = { filename, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), sstsrRecord.Record.ToString(), "R", statusObj.CurrentRecordNo.ToString(), pRTU, feptype, StsrRecNo, CtlRecNo, "", statusObj.GetValue("Name", 0),value }; //statusObj.GetValue("Name", 0) FEP RTU_CONTROL name (from ctl files) statusObj.GetValue("pRTU", 0)
                    object[] xrefObjects = { filename, type, statusObj.GetValue("pStation", 0), statusObj.GetValue("Key", 0), "STATUS", statusObj.GetValue("pAORGroup", 0), sstsrRecord.Record.ToString(), "R", statusObj.CurrentRecordNo.ToString(), 0, feptype, StsrRecNo, CtlRecNo, "", statusObj.GetValue("Name", 0), value }; //statusObj.GetValue("Name", 0) FEP RTU_CONTROL name (from ctl files) statusObj.GetValue("pRTU", 0)

                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion


            //ANALOG
            DbObject analogObj = ScadaDB.GetDbObject("ANALOG");
            DbObject scaleObj = ScadaDB.GetDbObject("SCALE");

           
            #region TLMRFileSet
            foreach (BescomFile tlmrFile in BescomParser.tlmrSet.Files.Values)
            {
                int filenum = Convert.ToInt32(tlmrFile.Name.Split('.')[0].Replace("TLMR", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {tlmrFile.Name }"); //BD
                    continue;
                }
                foreach (BescomRecord tlmrRecord in tlmrFile.ACSRecords)
                {
                    int stationid = Convert.ToInt32(tlmrFile.Name.Split('.')[0].Replace("TLMR", ""));
                  
                    string name = "";
                    if(tlmrRecord.InfoDict["point_description"].Contains("ENERGY"))
                    {
                        continue;
                    }
                    if (tlmrRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = tlmrRecord.InfoDict["point_description"];
                        if (name == "")
                        {
                            name = "--";
                            
                        }
                    }
                  
                    if (tlmrRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMR contains information. Point Description: {name}");
                        continue;
                    }
                  
                    if (stationid == 0) continue; 
                    analogObj.CurrentRecordNo++;
                   
                    analogObj.SetValue("Name", name);
                    SetArchiveGroup(analogObj); 

                    analogObj.SetValue("pAORGroup", SetAor2(tlmrRecord, stationid)); 
                    
                    analogObj.SetValue("pStation", SetpStation2(tlmrRecord, stationid));

                    SetAlarmGroups(analogObj, tlmrRecord); 
                    string type = "1";
                    analogObj.SetValue("Type", type);

                    int stationNumber = Convert.ToInt32(tlmrFile.Name.Split('.')[0].Replace("TLMR", ""));
                    int pStation = Convert.ToInt32(analogObj.GetValue("pStation", 0));
                    int pRTU = GetpRTU(tlmrRecord, stationid);
                    analogObj.SetValue("pRTU", pRTU);
                   
                    analogObj.SetValue("Key", ScadaDB.GetNextKey(3, pRTU));// KeyGen(3, stationNumber, tlmrRecord.Record + 1)); BD

                    //SCALES                                      
                    analogObj.SetValue("pScale", SetScale(ref ScadaDB, tlmrRecord));

                    SetAnalogLimits(analogObj, tlmrRecord);
                    int pUNIT = 0;
                    analogObj.SetValue("pUNIT", 0, SetpUNIT(tlmrRecord, name, pUNIT));
                    analogObj.SetValue("RawCountFormat", 0, 5);
                    string pst = analogObj.GetValue("pStation", 0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    object[] xrefObjects = { tlmrFile.Name, type, analogObj.GetValue("pStation", 0), analogObj.GetValue("Key", 0), "ANALOG", analogObj.GetValue("pAORGroup", 0), tlmrRecord.Record.ToString(), "N", analogObj.CurrentRecordNo.ToString(), analogObj.GetValue("pRTU", 0), "4", "", "", tlmrRecord.Record,analogObj.GetValue("Name",0),value,0 };
                   
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion

            #region TLMDFileSet
            foreach (BescomFile tlmdFile in BescomParser.tlmdSet.Files.Values)
            {
                int filenum = Convert.ToInt32(tlmdFile.Name.Split('.')[0].Replace("TLMD", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {tlmdFile.Name}"); //BD
                    continue;
                }
                foreach (BescomRecord tlmdRecord in tlmdFile.ACSRecords)
                {
                    int stationid = Convert.ToInt32(tlmdFile.Name.Split('.')[0].Replace("TLMD", ""));

                    string name = "";

                    if (tlmdRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = tlmdRecord.InfoDict["point_description"];
                        if (name == "")
                        {
                            name = "--";

                        }
                    }

                    if (tlmdRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMR contains information. Point Description: {name}");
                        continue;
                    }

                    if (stationid == 0) continue;
                    analogObj.CurrentRecordNo++;

                    analogObj.SetValue("Name", name);
                    SetArchiveGroup(analogObj);

                    analogObj.SetValue("pAORGroup", SetAor2(tlmdRecord, stationid));

                    analogObj.SetValue("pStation", SetpStation2(tlmdRecord, stationid));

                    SetAlarmGroups(analogObj, tlmdRecord);
                    string type = "1";
                    analogObj.SetValue("Type", type);

                    int stationNumber = Convert.ToInt32(tlmdFile.Name.Split('.')[0].Replace("TLMD", ""));
                    int pStation = Convert.ToInt32(analogObj.GetValue("pStation", 0));
                    int pRTU = GetpRTU(tlmdRecord, stationid);
                    analogObj.SetValue("pRTU", pRTU);

                    analogObj.SetValue("Key", ScadaDB.GetNextKey(3, pRTU));// KeyGen(3, stationNumber, tlmrRecord.Record + 1)); BD

                    //SCALES                                      
                    analogObj.SetValue("pScale", SetScale(ref ScadaDB, tlmdRecord));

                    SetAnalogLimits(analogObj, tlmdRecord);
                    int pUNIT = 0;
                    analogObj.SetValue("pUNIT", 0, SetpUNIT(tlmdRecord, name, pUNIT));
                    analogObj.SetValue("RawCountFormat", 0, 5);

                    object[] xrefObjects = { tlmdFile.Name, type, analogObj.GetValue("pStation", 0), analogObj.GetValue("Key", 0), "ANALOG", analogObj.GetValue("pAORGroup", 0), tlmdRecord.Record.ToString(), "N", analogObj.CurrentRecordNo.ToString(), analogObj.GetValue("pRTU", 0), "4", "", "", tlmdRecord.Record, analogObj.GetValue("Name", 0) };

                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion
            #region TLMPFileSet
            foreach (BescomFile tlmpFile in BescomParser.tlmpSet.Files.Values)
            {
                int filenum = Convert.ToInt32(tlmpFile.Name.Split('.')[0].Replace("TLMP", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {tlmpFile.Name}"); //BD
                    continue;
                }
                foreach (BescomRecord tlmpRecord in tlmpFile.ACSRecords)
                {
                    int stationid = Convert.ToInt32(tlmpFile.Name.Split('.')[0].Replace("TLMP", ""));

                    string name = "";

                    if (tlmpRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = tlmpRecord.InfoDict["point_description"];
                        if (name == "")
                        {
                            name = "--";

                        }
                    }

                    if (tlmpRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMR contains information. Point Description: {name}");
                        continue;
                    }

                    if (stationid == 0) continue;
                    analogObj.CurrentRecordNo++;

                    analogObj.SetValue("Name", name);
                    SetArchiveGroup(analogObj);

                    analogObj.SetValue("pAORGroup", SetAor2(tlmpRecord, stationid));

                    analogObj.SetValue("pStation", SetpStation2(tlmpRecord, stationid));

                    SetAlarmGroups(analogObj, tlmpRecord);
                    string type = "1";
                    analogObj.SetValue("Type", type);

                    int stationNumber = Convert.ToInt32(tlmpFile.Name.Split('.')[0].Replace("TLMP", ""));
                    int pStation = Convert.ToInt32(analogObj.GetValue("pStation", 0));
                    int pRTU = GetpRTU(tlmpRecord, stationid);
                    analogObj.SetValue("pRTU", pRTU);

                    analogObj.SetValue("Key", ScadaDB.GetNextKey(3, pRTU));// KeyGen(3, stationNumber, tlmrRecord.Record + 1)); BD

                    //SCALES                                      
                    analogObj.SetValue("pScale", SetScale(ref ScadaDB, tlmpRecord));

                    SetAnalogLimits(analogObj, tlmpRecord);
                    int pUNIT = 0;
                    analogObj.SetValue("pUNIT", 0, SetpUNIT(tlmpRecord, name, pUNIT));
                    analogObj.SetValue("RawCountFormat", 0, 5);

                    object[] xrefObjects = { tlmpFile.Name, type, analogObj.GetValue("pStation", 0), analogObj.GetValue("Key", 0), "ANALOG", analogObj.GetValue("pAORGroup", 0), tlmpRecord.Record.ToString(), "N", analogObj.CurrentRecordNo.ToString(), analogObj.GetValue("pRTU", 0), "4", "", "", tlmpRecord.Record, analogObj.GetValue("Name", 0) };

                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion

            #region TLMCFileSet
            foreach (BescomFile tlmcFile in BescomParser.tlmcSet.Files.Values)
            {
                int filenum = Convert.ToInt32(tlmcFile.Name.Split('.')[0].Replace("TLMC", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {tlmcFile.Name }"); //BD
                    continue;
                }
                foreach (BescomRecord tlmcRecord in tlmcFile.ACSRecords)
                {
                    int stationid = Convert.ToInt32(tlmcFile.Name.Split('.')[0].Replace("TLMC", ""));
                  
                    if (stationid == 0) { continue; }
                   
                    string name = "";

                    if (tlmcRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = tlmcRecord.InfoDict["point_description"];
                        if (name == "") { name = "--"; }
                    
                    }
                    
                    if (String.IsNullOrEmpty(name)) { continue; }
                  
                    if (tlmcRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMC contains information. Point Description: {name}");
                        continue;
                    }

                  
                    analogObj.CurrentRecordNo++;
                  
                    analogObj.SetValue("Name", name);
                    SetArchiveGroup(analogObj); 
                    analogObj.SetValue("RawCountFormat", 0, 5);
                    analogObj.SetValue("pAORGroup", SetAor2(tlmcRecord, stationid));
                    analogObj.SetValue("pStation", SetpStation2(tlmcRecord, stationid));
                   
                    SetAlarmGroups(analogObj, tlmcRecord);
                    string type = "2";
                    analogObj.SetValue("Type", type);
                    int pUNIT = 0;
                    analogObj.SetValue("pUNIT", 0, SetpUNIT(tlmcRecord, name, pUNIT));

                    if (tlmcRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMC contains information. Point Description: {name}");
                        continue;
                    }

                   
                    int pStation = Convert.ToInt32(analogObj.GetValue("pStation", 0));
                    if (pStation == 0)
                        pStation = 1;
                    analogObj.SetValue("Key", ScadaDB.GetNextKey(4, pStation));// KeyGen(4, pStation, tlmcRecord.Record + 1)); BD

                    analogObj.SetValue("pScale", SetScale(ref ScadaDB, tlmcRecord));
                    double high0, high1, high2, high3, high4 = 0.00;
                    double low0, low1, low2, low3, low4 = 0.00;

                   

                    SetAnalogLimits(analogObj, tlmcRecord);
                    string pst = analogObj.GetValue("pStation", 0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    object[] xrefObjects = { tlmcFile.Name, type, analogObj.GetValue("pStation", 0), analogObj.GetValue("Key", 0), "ANALOG", analogObj.GetValue("pAORGroup", 0), tlmcRecord.Record.ToString(), "n", analogObj.CurrentRecordNo.ToString(), "0", "", "", "", tlmcRecord.Record ,analogObj.GetValue("Name",0), value ,0};
                    
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion


            //SUBSTATION ANALOG 
            #region SubstationTLMR
            foreach (BescomFile stlmrFile in BescomParser.stlmr.Files.Values)
            {
                int filenum = Convert.ToInt32(stlmrFile.Name.Split('.')[0].Replace("TLMR", ""));
                DataRow stationRow = BescomParser.Substation_names.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stlmrFile.Name}"); //BD
                    continue;
                }
                foreach (BescomRecord stlmrRecord in stlmrFile.ACSRecords)
                {
                    int stationid = Convert.ToInt32(stlmrFile.Name.Split('.')[0].Replace("TLMR", ""));

                    string name = "";

                    if (stlmrRecord.InfoDict.Keys.Contains("point_description"))
                    {
                        name = stlmrRecord.InfoDict["point_description"];
                        if (name == "")
                        {
                            name = "--";

                        }
                    }

                    if (stlmrRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMR contains information. Point Description: {name}");
                        continue;
                    }

                    if (stationid == 0) continue;
                    analogObj.CurrentRecordNo++;

                    if (analogObj.CurrentRecordNo == 183414)
                    {
                        int i = 0;
                    }

                    analogObj.SetValue("Name", name);
                    SetArchiveGroup(analogObj);

                    analogObj.SetValue("pAORGroup", SetAorSubstation(stlmrRecord, stationid));

                    analogObj.SetValue("pStation", SetpStationSubstation(stlmrRecord, stationid));

                    SetAlarmGroups(analogObj, stlmrRecord);
                    string type = "1";
                    analogObj.SetValue("Type", type);

                    int stationNumber = Convert.ToInt32(stlmrFile.Name.Split('.')[0].Replace("TLMR", ""));
                    int pStation = Convert.ToInt32(analogObj.GetValue("pStation", 0));
                    int pRTU = GetpRTU(stlmrRecord, stationid);
                    analogObj.SetValue("pRTU", pRTU);

                    analogObj.SetValue("Key", ScadaDB.GetNextKey(3, stationid));// KeyGen(3, stationNumber, tlmrRecord.Record + 1)); BD

                    //SCALES                                      
                    analogObj.SetValue("pScale", SetScale(ref ScadaDB, stlmrRecord));

                    SetAnalogLimits(analogObj, stlmrRecord);
                    int pUNIT = 0;
                    analogObj.SetValue("pUNIT", 0, SetpUNIT(stlmrRecord, name, pUNIT));
                    analogObj.SetValue("RawCountFormat", 0, 5);
                    string pst = analogObj.GetValue("pStation", 0);
                    string value = string.Empty;
                    _StationDict.TryGetValue(pst, out value);
                    //object[] xrefObjects = { stlmrFile.Name, type, analogObj.GetValue("pStation", 0), analogObj.GetValue("Key", 0), "ANALOG", analogObj.GetValue("pAORGroup", 0), stlmrRecord.Record.ToString(), "N", analogObj.CurrentRecordNo.ToString(), analogObj.GetValue("pRTU", 0), "4", "", "", stlmrRecord.Record, analogObj.GetValue("Name", 0),value };
                    object[] xrefObjects = { stlmrFile.Name, type, analogObj.GetValue("pStation", 0), analogObj.GetValue("Key", 0), "ANALOG", analogObj.GetValue("pAORGroup", 0), stlmrRecord.Record.ToString(), "N", analogObj.CurrentRecordNo.ToString(), 0, "4", "", "", stlmrRecord.Record, analogObj.GetValue("Name", 0), value };
                    //pRtu made 0 becuase subststion points reporting to iccp 
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion
            //ACCUMULATOR
            DbObject accumulatorObj = ScadaDB.GetDbObject("ACCUMULATOR");
            accumulatorObj.CurrentRecordNo = 0;
            #region TLMAFileSet
            foreach (BescomFile tlmaFile in BescomParser.tlmaSet.Files.Values)
            {
                int filenum = Convert.ToInt32(tlmaFile.Name.Split('.')[0].Replace("TLMA", ""));
                DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { filenum });
                if (stationRow == null)
                {
                    Logger.Log("No_RTU_Address_Match", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {tlmaFile.Name }"); //BD
                    continue;
                }

                foreach (BescomRecord tlmaRecord in tlmaFile.ACSRecords)
                {

                    if(tlmaRecord.InfoDict["point_description"]=="")
                    {
                        continue;
                    }
                    string name;
                    if (tlmaRecord.InfoDict["point_id"].Equals(""))
                    {
                        name = tlmaRecord.InfoDict["point_description"];
                    }
                    else
                    {
                        name = tlmaRecord.InfoDict["point_id"];
                    }
                    if (tlmaRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty(name)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in TLMA contains information. Point Description: {name}");
                        continue;
                    }
                   
                    int stationid = Convert.ToInt32(tlmaFile.Name.Split('.')[0].Replace("TLMA", ""));
                    if (stationid == 0) continue;
                    accumulatorObj.CurrentRecordNo++;

                    //if (accumulatorObj.CurrentRecordNo > 20000)
                    //{
                    //    continue;
                    //}
                    accumulatorObj.SetValue("Name", name);
                    if(name== "METER 2 TOTAL REACTIVE ENERGY (24 HOURS)")
                    {
                        int i = 0;
                    }
                    accumulatorObj.SetValue("pAORGroup", SetAor2(tlmaRecord, stationid)); 
                    
                    accumulatorObj.SetValue("pStation", SetpStation2(tlmaRecord, stationid)); 

                    SetAlarmGroups(accumulatorObj, tlmaRecord); 
                    string type = "1";
                    accumulatorObj.SetValue("Type", type);

                    int stationNumber = Convert.ToInt32(tlmaFile.Name.Split('.')[0].Replace("TLMA", ""));
                    string maxcount = tlmaRecord.InfoDict["rollover_value"].ToString();
                    if(maxcount == "0.000000")
                    {
                        maxcount = "5.000000";
                    }
                    int pStation = Convert.ToInt32(accumulatorObj.GetValue("pStation", 0));
                    accumulatorObj.SetValue("Key", ScadaDB.GetNextKey(3, stationNumber));
                    //accumulatorObj.SetValue("MaxAccumCount", tlmaRecord.InfoDict["rollover_value"]);
                    accumulatorObj.SetValue("MaxAccumCount", maxcount);

                    accumulatorObj.SetValue("pScale", SetScale(ref ScadaDB, tlmaRecord));

                    SetAlarmGroups(accumulatorObj, tlmaRecord);
                    object[] xrefObjects = { tlmaFile.Name, type, accumulatorObj.GetValue("pStation", 0), accumulatorObj.GetValue("Key", 0), "ACCUM", accumulatorObj.GetValue("pAORGroup", 0), tlmaRecord.Record.ToString(), "N", accumulatorObj.CurrentRecordNo.ToString(), accumulatorObj.GetValue("pRTU", 0), "0" };
                    //{ "filename", "Type", "OSI Station", "SCADA Key", "Object", "OSI AOR", "Point Address", "Calculated", "OSI Record" };//TLMA
                    Xref.AddRecordSetValues(xrefObjects);
                }
            }
            #endregion

            Logger.CloseXMLLog();
          
            Database[] dbs = new Database[] { ScadaDB, StatesDB };
            foreach (Database db in dbs)
            {
                db.Write(ConversionSettings.FullDataPath);
            }
            Xref.WriteToDefault("BESCOM_XREF");
            AlarmXref.WriteToDefault("ALARM_XREF");
            AorXref.WriteToDefault("AOR_XREF");
        }
        public static void SetArchiveGroup(DbObject obj)
        {
            ArchiveGroup archiveFlagGroup = ArchiveGroup.None;
            archiveFlagGroup = ArchiveGroup.Index7;
            int archiveFlag = (int)archiveFlagGroup;
            obj.SetValue("Archive_group", archiveFlag);
        }
        public static int SetAor2(BescomRecord record, int stationid)
        {
           
            DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { stationid });
            if (stationRow != null)
            {
                #region AOR
                //string aornew = record.InfoDict["responsibility"].Trim();
                //int count = 0;
                //int digit1 = 0;
                //char[] digit = aornew.ToCharArray();
                //foreach(char aor1 in digit)
                //{
                //    if(aor1.Equals('1'))
                //    {
                //         digit1++;
                //    }

                //    count++;
                //    if((digit1==1)&&(count==1))
                //    {
                //        string AOR1 = "ISR";
                //    }
                //    if ((digit1 == 1) && (count == 2))
                //    {
                //        string AOR1 = "Event";
                //    }
                //    if ((digit1 == 2) && (count == 2))
                //    {
                //        string AOR1 = "Event";
                //    }
                //    if ((digit1 == 3) && (count == 50))
                //    {
                //        string AOR1 = "WHITEFIELD";
                //    }
                //    if ((digit1 == 3) && (count == 51))
                //    {
                //        string AOR1 = "JALAHALLI";
                //    }

                //}
                #endregion
                int.TryParse(stationRow["AOR_Group"].ToString(), out int aor);
                return aor;
            }
            else
                return 1;
        }

        public static int SetAorSubstation(BescomRecord record, int stationid)
        {

            DataRow stationRow = BescomParser.Substation_names.GetInfo(new object[] { stationid });
            if (stationRow != null)
            {
                #region AOR
                //string aornew = record.InfoDict["responsibility"].Trim();
                //int count = 0;
                //int digit1 = 0;
                //char[] digit = aornew.ToCharArray();
                //foreach(char aor1 in digit)
                //{
                //    if(aor1.Equals('1'))
                //    {
                //         digit1++;
                //    }

                //    count++;
                //    if((digit1==1)&&(count==1))
                //    {
                //        string AOR1 = "ISR";
                //    }
                //    if ((digit1 == 1) && (count == 2))
                //    {
                //        string AOR1 = "Event";
                //    }
                //    if ((digit1 == 2) && (count == 2))
                //    {
                //        string AOR1 = "Event";
                //    }
                //    if ((digit1 == 3) && (count == 50))
                //    {
                //        string AOR1 = "WHITEFIELD";
                //    }
                //    if ((digit1 == 3) && (count == 51))
                //    {
                //        string AOR1 = "JALAHALLI";
                //    }

                //}
                #endregion
                int.TryParse(stationRow["AOR_Group"].ToString(), out int aor);
                return aor;
            }
            else
                return 1;
        }

        public static int SetpUNIT(BescomRecord record, string name, int pUNIT)
        {
            if (name != null)
            {
                if (name.Contains("CURRENT")|| name.Contains("_IR")|| name.Contains("_IY")|| name.Contains("_IB"))
                {
                    pUNIT = 1;
                }
                else if (name.Contains("VOLTS")|| name.Contains("_V"))
                {
                    pUNIT = 2;
                }
                else if (name.Contains("(KW)"))
                {
                    pUNIT = 3;
                }
                else if (name.Contains("(KVAR)"))
                {
                    pUNIT = 4;
                }
                else if (name.Contains("ENERGY"))
                {
                    pUNIT = 5;
                }
                else if (name.Contains("_MW"))
                {
                    pUNIT = 6;
                }
                else if (name.Contains("_MVAR"))
                {
                    pUNIT = 7;
                }
                else if (name.Contains("_MWH"))
                {
                    pUNIT = 8;
                }

                else

                    pUNIT = 1;


            }

            return pUNIT;
        }
        public static int GetpRTU(BescomRecord record, int stationid) 
        {
            
            DataRow stationRow = BescomParser.mastercfg_filteredTbl.GetInfo(new object[] { stationid });
            if (stationRow != null)
            {
                int.TryParse(stationRow["id"].ToString(), out int pRTU);
                return pRTU;
            }
            else
                return 1; 
        }
        public static int SetpStation2(BescomRecord record, int stationid) 
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
        public static int SetpStationSubstation(BescomRecord record, int stationid)
        {
            //if (stationid == 10) stationid = 1;
            DataRow stationRow = BescomParser.Substation_names.GetInfo(new object[] { stationid });
            if (stationRow != null)
            {
                int.TryParse(stationRow["Display_no."].ToString(), out int stationNum);
                return stationNum;
            }
            else
                return 1;
        }


        /// <summary>
        /// Get OSI_BYTE corresponding to baseMask. Note: baseMask needs to be an int 
        /// of length 8 with only 0's and 1's (int array representation of a byte).
        /// </summary>
        /// <param name="baseMask">int array representation of a byte</param>
        public static double GetNominalPairInactive(int[] baseMask)
        {
            Array.Reverse(baseMask);
            if (null == baseMask)
            {
                throw new ArgumentNullException(nameof(baseMask));
            }
            double nominalPairInactive = 0;

            for (int i = 0; i < baseMask.Length; i++)
            //for (int i = baseMask.Length - 1; i >= 0; i--)
            {
                if (baseMask[i].Equals(1))
                {
                    nominalPairInactive += Math.Pow(2, i);
                }
            }


            return nominalPairInactive;
        }
        public static void SetAnalogLimits(DbObject analogObj, BescomRecord acsRecord)
        {
            if (null == analogObj)
            {
                throw new ArgumentNullException(nameof(analogObj));
            }
            if (null == acsRecord)
            {
                throw new ArgumentNullException(nameof(acsRecord));
            }
            int[] nomPairInactive = { 0, 0, 0, 1, 1, 1, 1, 1 };

            double high0, high1, high2, high3, high4 = 0.00;
            double low0, low1, low2, low3, low4 = 0.00;
            high0 = Convert.ToDouble(acsRecord.InfoDict["high_operational_limit"]);
            high1 = Convert.ToDouble(acsRecord.InfoDict["high_emergency_limit"]);
            high4 = Convert.ToDouble(acsRecord.InfoDict["high_reasonability_limit"]);
            low0 = Convert.ToDouble(acsRecord.InfoDict["low_operational_limit"]);
            if(low0== 8.000000 )
            {
                int i = 0;
                low0 = -8.000000;
            }
            low1 = Convert.ToDouble(acsRecord.InfoDict["low_emergency_limit"]);
            low4 = Convert.ToDouble(acsRecord.InfoDict["low_reasonability_limit"]);
            analogObj.SetValue("NominalHiLimits", 0, high0);// acsRecord.InfoDict["high_operational_limit"]);
            analogObj.SetValue("NominalHiLimits", 1, high1);// acsRecord.InfoDict["high_emergency_limit"]);
            analogObj.SetValue("NominalLowLimits", 0, low0);// acsRecord.InfoDict["low_operational_limit"]);
            analogObj.SetValue("NominalLowLimits", 1, low1);// acsRecord.InfoDict["low_emergency_limit"]);
            string key = analogObj.GetValue("Key", 0);

            if (!acsRecord.InfoDict["high_operational_limit"].Equals("0.000000") && !acsRecord.InfoDict["low_operational_limit"].Equals("0.000000"))
            {
                nomPairInactive[6] = 0;
            }
            if (!acsRecord.InfoDict["high_emergency_limit"].Equals("0.000000") && !acsRecord.InfoDict["low_emergency_limit"].Equals("0.000000"))
            {
                nomPairInactive[5] = 0;
            }
            //if (acsRecord.InfoDict["high_operational_limit"].Equals(acsRecord.InfoDict["low_operational_limit"]))
            //{
            //    nomPairInactive[6] = 0;
            //}
            //if (acsRecord.InfoDict["high_emergency_limit"].Equals(acsRecord.InfoDict["low_emergency_limit"]))
            //{
            //    nomPairInactive[5] = 0;
            //}
            //if (high0 <=low0)
            //{
            //    nomPairInactive[6] = 0;
            //}
            //if (high1 <= low1)
            //{
            //    nomPairInactive[5] = 0;
            //}

            
            analogObj.SetValue("NominalPairInactive", GetNominalPairInactive(nomPairInactive));

            if (!acsRecord.InfoDict["high_reasonability_limit"].Equals("0.000000"))
            {
                analogObj.SetValue("NominalHiLimits", 4, acsRecord.InfoDict["high_reasonability_limit"]);
            }
            else
            {
                analogObj.SetValue("NominalHiLimits", 4, 999999);
            }
            if (!acsRecord.InfoDict["low_reasonability_limit"].Equals("0.000000"))
            {
                analogObj.SetValue("NominalLowLimits", 4, acsRecord.InfoDict["low_reasonability_limit"]);
            }
            else
            {
                analogObj.SetValue("NominalLowLimits", 4, -999999);
            }
        }
        public static int GetNominalpairinactive(double high0, double high1, double high2, double high3, double high4, double low0, double low1, double low2, double low3, double low4)
        {
            dynamic convertedVal;
            convertedVal = 1;
            var NominalPairInactive = 0x00;
            if (high0 == 0 && low0 == 0) NominalPairInactive |= 0x02;
            else NominalPairInactive &= ~0x02;
            if (high1 == 0 && low1 == 0) NominalPairInactive |= 0x04;
            else NominalPairInactive &= ~0x04;
            if (high2 == 0 && low2 == 0) NominalPairInactive |= 0x08;
            else NominalPairInactive &= ~0x08;
            if (high3 == 0 && low4 == 0) NominalPairInactive |= 0x10;
            else NominalPairInactive &= ~0x10;
            if (high4 == 0 && low4 == 0) NominalPairInactive |= 0x01;
            else NominalPairInactive &= ~0x01;
            NominalPairInactive |= 0x01;

            return NominalPairInactive;
            return convertedVal;
        }
       
        public static void SetAlarmGroups(DbObject dbObj, BescomRecord acsRecord)
        {
            if (null == dbObj)
            {
                throw new ArgumentNullException(nameof(dbObj));
            }
            if (null == acsRecord)
            {
                throw new ArgumentNullException(nameof(acsRecord));
            }
            int tempAlarmClass = Convert.ToInt32(acsRecord.InfoDict["alarm_priority"]);

            switch (tempAlarmClass)
            {
                case 0:
                    dbObj.SetValue("pALARM_GROUP", 3);
                    break;
                case 1:
                    dbObj.SetValue("pALARM_GROUP", 2);
                    break;
                case 2:
                    dbObj.SetValue("pALARM_GROUP", 1);
                    break;
                case 3:
                    dbObj.SetValue("pALARM_GROUP", 3);
                    break;
                case 4:
                    dbObj.SetValue("pALARM_GROUP", 4);
                    break;
                case 5:
                    dbObj.SetValue("pALARM_GROUP", 1);
                    break;
                default:
                    dbObj.SetValue("pALARM_GROUP", -1);
                    break;
            }
        }
        

        public static int SetScale(ref SCADA scadaDB, BescomRecord record)
        {
            DbObject scaleObj = scadaDB.GetDbObject("SCALE");

            int pScale = 0;

            foreach (BescomFile scfFile in BescomParser.scfSet.Files.Values)
            {
                foreach (BescomRecord scfRecord in scfFile.ACSRecords)
                {
                    //if (scfRecord.Record.ToString().Equals(record.InfoDict["scale_factor_index"])) //BD
                    {
                        float fsv = (float)Convert.ToDouble(scfRecord.InfoDict["full_scale_value"]);
                        float zsv = (float)Convert.ToDouble(scfRecord.InfoDict["zero_scale_value"]);
                        float divisor = (float)Convert.ToDouble(scfRecord.InfoDict["divisor"]);

                        float scale = (fsv - zsv);
                        if (!divisor.Equals(0))
                            scale = fsv / divisor;
                        else scale = fsv / 2047;

                        float offset = zsv;

                        if (_scaleDict.ContainsKey(scale))
                        {
                            pScale = _scaleDict[scale];
                            return pScale;
                        }
                        else
                        {
                            _scaleDict.Add(scale, _scaleDict.Count + 1);
                            pScale = _scaleDict[scale];
                            scaleObj.CurrentRecordNo = _scaleDict.Count;
                            scaleObj.SetValue("Scale", scale);
                            scaleObj.SetValue("Offset", offset);
                            //scaleObj.SetValue("Name", "(" + fsv.ToString() + " - " + zsv.ToString() + ")/" + divisor.ToString());
                            scaleObj.SetValue("Name", fsv.ToString());
                        }
                        return pScale;
                    }
                    
                }
            }
            return 0;
        }

        public static List<string> SetStates(BescomRecord record)
        {
            List<string> statesList2 = new List<string> { "", "" };
            return statesList2; //BD
            if (record.InfoDict.Keys.Contains("status_pair_code"))
            {
                int spc = Convert.ToInt32(record.InfoDict["status_pair_code"]);
                int spc1 = spc + 1;
                BescomFile stateFile;
                BescomFile stateFile1;
                if (spc < 200)
                {
                    stateFile = BescomParser.stpSet.Files["STP00000"];
                }
                else
                {
                    stateFile = BescomParser.stpSet.Files["STP00001"];
                    spc -= 200;
                }

                if (spc1 < 200)
                {
                    stateFile1 = BescomParser.stpSet.Files["STP00000"];
                }
                else
                {
                    stateFile1 = BescomParser.stpSet.Files["STP00001"];
                    spc1 -= 200;
                }

                string state1 = stateFile.ACSRecords[spc].InfoDict["message"].Trim(' ');

                string state2 = stateFile1.ACSRecords[spc1].InfoDict["message"].Trim(' ');

                if (state1.Equals(""))
                {
                    state1 = "<Blank>";
                }

                if (state2.Equals(""))
                {
                    state2 = "<Blank>";
                }

                if (state1 != null && state2 != null)
                {
                    List<string> statesList = new List<string> { state1, state2 };

                    return statesList;
                }

            }
            return null;
        }
        /// <summary>
        /// Function used to set the states from the uniqueStates dictionary. 
        /// </summary>
        /// <param name="acsRecord" type="ACSRecord"> Current record from input file </param>
        /// <returns> returns pState of type int </returns>


    }
}
