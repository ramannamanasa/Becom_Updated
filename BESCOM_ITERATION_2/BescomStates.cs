using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AESES_Databases;
using OSII.DatabaseConversionToolkit;
using OSII.ConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using System.Data;
using System.Net.NetworkInformation;
using System.Net;
using System.Xml.Linq;

namespace AESES_Databases
{
    internal class BescomStates
    {
        private static Dictionary<string, int> _uniqueStates; //declaration 
        private static List<KeyValuePair<string,int>> mydupstate = new List<KeyValuePair<string,int>>();

        private static int statesRec;
        private static BescomParser _parser;
        private static object statusNew1;
        public STATES StatesDB = new STATES();//BD
		public static Dictionary<string,int> _newparam;
		

        /// <summary>
        /// Converts states and sets them into the states database
        /// </summary>
        /// <param name="StatesDB" type="STATES"> States database from Converter </param>
        public static void ConvertStates2(STATES StatesDB)
        {
            //_parser = parser;
            _uniqueStates = new Dictionary<string, int>(); // AESESParser.UniqueStates;// _parser.UniqueStates; //initiLIZATION
            statesRec = 200;
            Logger.OpenXMLLog("ConvertStates");

            foreach (BescomFile stspFile in BescomParser.stspSet.Files.Values)
            {
                foreach (BescomRecord stspRecord in stspFile.ACSRecords)
                {
                    SetStates(StatesDB, stspRecord);
                }
            }

            foreach (BescomFile stsrFile in BescomParser.stsrSet.Files.Values)
            {
                if (stsrFile.ACSRecords == null)
                {
                    Logger.Log("Empty File", LoggerLevel.INFO, $"No Match for file# found for RTU_Address in the master file for the input file : {stsrFile.Name}"); //BD
                    continue;
                }
                foreach (BescomRecord stsrRecord in stsrFile.ACSRecords)
                {
                    SetStates(StatesDB, stsrRecord);
                }


            }
            Logger.CloseXMLLog();
        }

        /// <summary>
        /// Function used to set the states from the input files. Name0Value determines which file it is take from.
        /// </summary>
        /// <param name="StatesDB" type="STATES"> States Database </param>
        /// <param name="acsRecord" type="ACSRecord"> Current record from input file </param>
        public static void SetStates(STATES StatesDB, BescomRecord acsRecord)
        {
            DbObject statesObj = StatesDB.GetDbObject("STATE");
            BescomFile stpFileDefault = BescomParser.stpSet.Files.FirstOrDefault(x => x.Key.Equals("STP00000")).Value;
            BescomFile stpFileCustom = BescomParser.stpSet.Files.FirstOrDefault(x => x.Key.Equals("STP00001")).Value;
            int name0Value = 0;
            int name1Value = 0;
            int name2Value = 0;
            int name3Value = 0;

            foreach (DataRow stateRow in BescomParser.StateRec_Tbl.Rows)
            {
                string rec1 = stateRow["RECORD1"].ToString();
                string rec2 = stateRow["RECORD2"].ToString();

                name0Value = Convert.ToInt32(rec1);
                name1Value = Convert.ToInt32(rec2);
                string tempNamePair;
                //statesObj.CurrentRecordNo = ++statesRec;
                tempNamePair = $"{stpFileDefault.ACSRecords[name0Value].InfoDict["message"].Trim()}/{stpFileDefault.ACSRecords[name1Value].InfoDict["message"].Trim()}";
                //if (mydupstate.Contains(tempNamePair))
                if (!_uniqueStates.ContainsKey(tempNamePair))
                {
                    statesObj.CurrentRecordNo = ++statesRec;
                    if(statesRec==23)
                    {
                        int i = 0;
                    }
                    statesObj.SetValue("names_0", stpFileDefault.ACSRecords[name0Value].InfoDict["message"].Trim());
                    statesObj.SetValue("names_1", stpFileDefault.ACSRecords[name1Value].InfoDict["message"].Trim());
                    statesObj.SetValue("description", tempNamePair);
                    _uniqueStates.Add(tempNamePair, statesRec);
                    //mydupstate.Add(tempNamePair, statesRec);
                }

            }
           // statesObj.CurrentRecordNo = ++statesRec;
            foreach (DataRow stateRow in BescomParser.StateRec_Tbl123.Rows)
            {

                
                string rec3 = stateRow["RECORD3"].ToString();
                string rec4 = stateRow["RECORD4"].ToString();

              
                int rec5 = 15;
                int rec6 = 16;


                name2Value = Convert.ToInt32(rec3);
                name3Value = Convert.ToInt32(rec4);
                if(name2Value==23)
                {
                    int i = 0;
                }

                string tempNamePair;

                tempNamePair = $"{stpFileCustom.ACSRecords[name2Value].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[name3Value].InfoDict["message"].Trim()}";
                //statesObj.CurrentRecordNo = ++statesRec;
                if (name2Value != 13)
                {
                    if (!_uniqueStates.ContainsKey(tempNamePair))
                    {

                        statesObj.CurrentRecordNo = ++statesRec;
                        statesObj.SetValue("names_0", stpFileCustom.ACSRecords[name2Value].InfoDict["message"].Trim());
                        statesObj.SetValue("names_1", stpFileCustom.ACSRecords[name3Value].InfoDict["message"].Trim());
                        statesObj.SetValue("description", tempNamePair);
                        _uniqueStates.Add(tempNamePair, statesRec);
                    }
                }

                if (name2Value == 13)
                {
                    tempNamePair = $"{stpFileCustom.ACSRecords[name2Value].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[name3Value].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[rec5].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[rec6].InfoDict["message"].Trim()}";


                    if (!_uniqueStates.ContainsKey(tempNamePair))
                    {
                        statesObj.CurrentRecordNo = ++statesRec;
                        statesObj.SetValue("names_0", stpFileCustom.ACSRecords[name2Value].InfoDict["message"].Trim());
                        statesObj.SetValue("names_1", stpFileCustom.ACSRecords[name3Value].InfoDict["message"].Trim());
                        statesObj.SetValue("names_2", stpFileCustom.ACSRecords[rec5].InfoDict["message"].Trim());
                        statesObj.SetValue("names_3", stpFileCustom.ACSRecords[rec6].InfoDict["message"].Trim());
                        statesObj.SetValue("description", tempNamePair);
                        _uniqueStates.Add(tempNamePair, statesRec);
                    }

                }
               
            }

        }
        public static int SetStates2(BescomRecord acsRecord)
        {
            
            if (null == acsRecord)
            {
                throw new ArgumentNullException(nameof(acsRecord));
            }
            int name0Value1 = Convert.ToInt32(acsRecord.InfoDict["status_pair_code"]);
            int name1Value2 = Convert.ToInt32(acsRecord.InfoDict["status_pair_code"]) + 1;
            BescomFile stpFileDefault = BescomParser.stpSet.Files.FirstOrDefault(x => x.Key.Equals("STP00000")).Value;
            BescomFile stpFileCustom = BescomParser.stpSet.Files.FirstOrDefault(x => x.Key.Equals("STP00001")).Value;
            string tempNamePair;
            int pState = 0;
            int rec5 = 215;
            int rec6 = 216;

            if (name0Value1 < 200)
            {
                tempNamePair = $"{stpFileDefault.ACSRecords[name0Value1].InfoDict["message"].Trim()}/{stpFileDefault.ACSRecords[name1Value2].InfoDict["message"].Trim()}";
                pState = _uniqueStates[tempNamePair];

                return pState;

            }
            if ((name0Value1 >= 200)&&(name0Value1 !=213))
            {
                tempNamePair = $"{stpFileCustom.ACSRecords[name0Value1-200].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[name1Value2-200].InfoDict["message"].Trim()}";
                pState = _uniqueStates[tempNamePair];
                if(pState==327)
                {
                    pState = 317;
                }
                return pState;

            }
            if (name0Value1 == 213)
            {
                tempNamePair = $"{stpFileCustom.ACSRecords[name0Value1-200].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[name1Value2-200].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[rec5-200].InfoDict["message"].Trim()}/{stpFileCustom.ACSRecords[rec6-200].InfoDict["message"].Trim()}";

                pState = _uniqueStates[tempNamePair];

                return pState;

            }

            else return 1;
            
        }
    }
}


