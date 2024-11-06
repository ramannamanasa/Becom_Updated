using System.Linq;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
using OSII.ConversionToolkit;
using AESES_Databases;
using System.Data;
using System;
using OSII.ConversionToolkit.Generic;
using System.Collections.Generic;

namespace AESES_Databases
{
    internal class BescomOpenCalc
    {
        private static int controlRec = 0;


        /// <summary>
        /// Function to convert channel groups
        /// </summary>
        /// <param name="FepDb"type="FEP"></param>
        public static void ConvertFormTemplate(OpenCalc OpenCalcDB)
        {
            Logger.OpenXMLLog();
            DbObject FormTempObj = OpenCalcDB.GetDbObject("FORMULA_TEMPLATE");
            int FormtempRec = 0; int id = 0;
            foreach (BescomFile FormulaFile in BescomParser.FormulaSet.Files.Values)
            {
                foreach (BescomRecord FormulaRecord in FormulaFile.ACSRecords)
                {
                    if (FormulaRecord.Record.Equals(0))
                    {
                        if (!String.IsNullOrEmpty("Formula "+ FormulaRecord.Record)) Logger.Log("REC#0 Containing Information", LoggerLevel.INFO, $"Record#0 in Formula contains information. Point Description: {"Formula " + FormulaRecord.Record}");
                        continue;
                    }
                   
                    FormTempObj.CurrentRecordNo = ++FormtempRec;
                    if(FormtempRec==135)
                    {
                        int i = 0;
                    }
                    FormTempObj.SetValue("Name", "Formula" +"_"+ FormulaRecord.Record);
                    FormTempObj.SetValue("FunctionIdentifier", "Formula" +"_"+ FormulaRecord.Record);
                    FormTempObj.SetValue("Description", FormulaRecord.InfoDict["formula_string"]);
                    //InputDataType {100}
                    int incount = 0;
                    if (FormulaRecord.InfoDict["formula_string"].Contains("V6")) incount = 6;
                    else if (FormulaRecord.InfoDict["formula_string"].Contains("V5")) incount = 5;
                    else if (FormulaRecord.InfoDict["formula_string"].Contains("V4")) incount = 4;
                    else if (FormulaRecord.InfoDict["formula_string"].Contains("V3")) incount = 3;
                    else if (FormulaRecord.InfoDict["formula_string"].Contains("V2")) incount = 2;
                    else if (FormulaRecord.InfoDict["formula_string"].Contains("V1")) incount = 1;
                    for(int i= 0 ;i < incount; i++)
                    {
                        FormTempObj.SetValue("InputDataType",i, 1);
                    }
                    //OutputDataType {10}
                    FormTempObj.SetValue("OutputDataType", 1);
                }
            }

             
            Logger.CloseXMLLog();
        }
        public static void ConvertFormula(OpenCalc OpenCalcDB, GenericTable Xref)
        {
            Logger.OpenXMLLog();
            DbObject FormObj = OpenCalcDB.GetDbObject("FORMULA");
            int FormRec = 0; int id = 0;
            
            Xref.Sort("filename,Object,AESRec");
            foreach (BescomFile FormulaFile in BescomParser.tlmcSet.Files.Values)
            {
                int recodsCount = 0;
                recodsCount = FormulaFile.ACSRecords.Count() / 2;

                for(int i = 1;i < recodsCount; i++)
                {
                    List<BescomRecord> TLMCRecs = new  List<BescomRecord>();
                    TLMCRecs = FormulaFile.ACSRecords.Where(r => r.Record == i).ToList();
                    if (TLMCRecs.Count != 2 || TLMCRecs.Count <= 1 || TLMCRecs.Count <= 0)
                    {
                        Logger.Log("OpenCalc_Error", LoggerLevel.INFO, $" Either of Telemetry or Calcualted part is missing: Filename: {FormulaFile.Name } , RecNo: {i}, Count:{TLMCRecs.Count}"); //BD
                        continue;
                    }
                    string name = "";
                    if (TLMCRecs[0].InfoDict.ContainsKey("point_description")) 
                        name = TLMCRecs[0].InfoDict["point_description"];
                    if (name == "Voltaje b-n (Calc)")
                    {
                        int t = 0;
                    }

                   

                    if (! TLMCRecs[0].InfoDict.ContainsKey("point_description"))
                    {
                        Logger.Log("OpenCalc_Error", LoggerLevel.INFO, $"Telemetry part is absent Filename: {FormulaFile.Name } , RecNo: {i}"); //BD

                        continue;
                    }
                    if (string.IsNullOrEmpty(name) )
                    {
                        Logger.Log("OpenCalc_Error", LoggerLevel.INFO, $"Name is Empty Filename: {FormulaFile.Name } , RecNo: {i}"); //BD

                        continue;
                    }

                    string PformtempRec = TLMCRecs[1].InfoDict["formula_index"];
                    int pformulatremprec = 0;
                    int.TryParse(PformtempRec, out pformulatremprec);
                    if (pformulatremprec < 0)
                    {
                        pformulatremprec = (-1 * pformulatremprec) + 103;
                    }
                    FormObj.CurrentRecordNo = ++FormRec;
                    FormObj.SetValue("Name", name);
                    FormObj.SetValue("pTemplateRecord", pformulatremprec);
                    FormObj.SetValue("pExecutionGroup", 3);
                    FormObj.SetValue("InputType", 1);
                    FormObj.SetValue("OutputType", 1);
                    //FormObj.SetValue("pTemplateRecord", pformulatremprec);

                    if (TLMCRecs[1].InfoDict["formula_index"] == "0" || TLMCRecs[1].InfoDict["formula_index"].Contains("-"))
                    {
                        Logger.Log("OpenCalc_Error", LoggerLevel.INFO, $" Formula Index is {TLMCRecs[1].InfoDict["formula_index"]} in Filename: {FormulaFile.Name } , RecNo: {i}"); //BD

                        continue;
                    }
                    //Added by RM 01062023 for pTemplateRecord Requirement for negative formula index
                    //string PformtempRec = TLMCRecs[1].InfoDict["formula_index"];
                    //int pformulatremprec = 0;
                    //int.TryParse(PformtempRec, out pformulatremprec);
                    //if (pformulatremprec < 0)
                    //{
                    //    pformulatremprec = (-1 * pformulatremprec) + 103;
                    //}
                    
                    int incount =1 ;
                    bool entered = false;
                    while(TLMCRecs[1].InfoDict.ContainsKey("station_" + incount.ToString()))
                    {
                        if (TLMCRecs[1].InfoDict["category_" + incount.ToString()] == "D")
                        {
                            Logger.Log("OpenCalc_Error", LoggerLevel.INFO, $"Category D found in Filename: {FormulaFile.Name } , RecNo: {i},Name: {TLMCRecs[0].InfoDict["point_description"]}"); //BD
                            incount++;
                            continue;
                        }
                        
                        string inkey = "";
                        string outkey = "";
                        string filename = "TLMC" + incount.ToString().PadLeft(5, '0');
                        //string fileName = "CTL" + deviceno; FormulaFile.Name;
                        
                        if (TLMCRecs[1].InfoDict["point_" + incount.ToString()] == "0" || TLMCRecs[1].InfoDict["station_" + incount.ToString()] =="0" )
                        {
                            incount++;
                        }
                        else
                        {
                            entered = true;

                            if (TLMCRecs[1].InfoDict["category_" + incount.ToString()] == "R")
                            {
                                filename = filename.Replace("TLMC", "TLMR");
                            }
                            string tlmrrec = TLMCRecs[1].InfoDict["point_" + incount.ToString()];
                            Xref.TryGetRow(new[] { filename, "ANALOG", tlmrrec }, out DataRow xrefRow);
                            if (xrefRow != null)
                            {
                                inkey = xrefRow["SCADA Key"].ToString();
                                FormObj.SetValue("InputKey", incount - 1, inkey);
                                

                            }
                            Xref.TryGetRow(new[] { filename.Replace("TLMR", "TLMC"), "ANALOG", i.ToString() }, out DataRow xrefRow2);
                            if (xrefRow2 != null)
                            {
                                outkey = xrefRow2["SCADA Key"].ToString();
                                FormObj.SetValue("OutputKey", outkey);
                            }
                            
                            
                            incount++;
                        }
                        
                        
                    }
                }

               
            }


            Logger.CloseXMLLog();
        }
        
        public static void ConvertTimer(OpenCalc OpenCalcDB)
        {
            Logger.OpenXMLLog();
            DbObject TimerObj = OpenCalcDB.GetDbObject("TIMER");
            int TimerRec = 0; int id = 0;
            
                TimerObj.CurrentRecordNo = ++TimerRec;
                TimerObj.SetValue("Indic", 0);
                TimerObj.SetValue("Name", "Period 5 sec");
                TimerObj.SetValue("Reinitialize", 0);
                TimerObj.SetValue("Period", 5);
                TimerObj.SetValue("PeriodUnit", 1);
                TimerObj.SetValue("PeriodOffset", 0);
                TimerObj.SetValue("PeriodOffsetUnit", 0);
                TimerObj.SetValue("ProcessBits", 0);

                TimerObj.CurrentRecordNo = ++TimerRec;
                TimerObj.SetValue("Indic", 0);
                TimerObj.SetValue("Name", "Period 10 sec");
                TimerObj.SetValue("Reinitialize", 0);
                TimerObj.SetValue("Period", 10);
                TimerObj.SetValue("PeriodUnit", 1);
                TimerObj.SetValue("PeriodOffset", 0);
                TimerObj.SetValue("PeriodOffsetUnit", 0);
                TimerObj.SetValue("ProcessBits", 0);

                TimerObj.CurrentRecordNo = ++TimerRec;
                TimerObj.SetValue("Indic", 0);
                TimerObj.SetValue("Name", "Period 15 sec");
                TimerObj.SetValue("Reinitialize", 0);
                TimerObj.SetValue("Period", 15);
                TimerObj.SetValue("PeriodUnit", 1);
                TimerObj.SetValue("PeriodOffset", 0);
                TimerObj.SetValue("PeriodOffsetUnit", 0);
                TimerObj.SetValue("ProcessBits", 0);

                TimerObj.CurrentRecordNo = ++TimerRec;
                TimerObj.SetValue("Indic", 0);
                TimerObj.SetValue("Name", "Period 20 sec");
                TimerObj.SetValue("Reinitialize", 0);
                TimerObj.SetValue("Period", 20);
                TimerObj.SetValue("PeriodUnit", 1);
                TimerObj.SetValue("PeriodOffset", 0);
                TimerObj.SetValue("PeriodOffsetUnit", 0);
                TimerObj.SetValue("ProcessBits", 0);
                
            Logger.CloseXMLLog();
        }
        public static void ConvertEXxecutionGrp(OpenCalc OpenCalcDB)
        {
            Logger.OpenXMLLog();
            DbObject ExcGrpObj = OpenCalcDB.GetDbObject("EXECUTION_GROUP");
            int ExcGrpObjRec = 0; int id = 0;
            ExcGrpObj.CurrentRecordNo = ++ExcGrpObjRec;
            ExcGrpObj.SetValue("Indic", 0);
            ExcGrpObj.SetValue("Name", "EG Period 5 sec");
            ExcGrpObj.SetValue("ProcessBits", 0);
            ExcGrpObj.SetValue("ConfigBits", 1);
            ExcGrpObj.SetValue("Priority", 0);
            ExcGrpObj.SetValue("pTimerRecord", 1);
            ExcGrpObj.SetValue("LastTriggerTime", 0);
            
            ExcGrpObj.CurrentRecordNo = ++ExcGrpObjRec;
            ExcGrpObj.SetValue("Indic", 0);
            ExcGrpObj.SetValue("Name", "EG Period 10 sec");
            ExcGrpObj.SetValue("ProcessBits", 0);
            ExcGrpObj.SetValue("ConfigBits", 1);
            ExcGrpObj.SetValue("Priority", 0);
            ExcGrpObj.SetValue("pTimerRecord", 2);
            ExcGrpObj.SetValue("LastTriggerTime", 0);

            ExcGrpObj.CurrentRecordNo = ++ExcGrpObjRec;
            ExcGrpObj.SetValue("Indic", 0);
            ExcGrpObj.SetValue("Name", "EG Period 15 sec");
            ExcGrpObj.SetValue("ProcessBits", 0);
            ExcGrpObj.SetValue("ConfigBits", 1);
            ExcGrpObj.SetValue("Priority", 0);
            ExcGrpObj.SetValue("pTimerRecord", 3);
            ExcGrpObj.SetValue("LastTriggerTime", 0);

            ExcGrpObj.CurrentRecordNo = ++ExcGrpObjRec;
            ExcGrpObj.SetValue("Indic", 0);
            ExcGrpObj.SetValue("Name", "EG Period 20 sec");
            ExcGrpObj.SetValue("ProcessBits", 0);
            ExcGrpObj.SetValue("ConfigBits", 1);
            ExcGrpObj.SetValue("Priority", 0);
            ExcGrpObj.SetValue("pTimerRecord", 4);
            ExcGrpObj.SetValue("LastTriggerTime", 0);

            Logger.CloseXMLLog();
        }
    }
}
