using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSII.ConversionToolkit.Generic;
using OSII.ConversionToolkit;


namespace AESES_Databases
{ 
    public class BescomParser
    {
        public static readonly string Closing = "|";

        public static BescomSet stpSet;
        public static BescomSet stscSet;
        public static BescomSet stsdSet;
        public static BescomSet stsiSet;
        public static BescomSet stsrSet;
        public static BescomSet ctlSet;
        public static BescomSet tlmaSet;
        public static BescomSet tlmrSet;
        public static BescomSet tlmdSet;
        public static BescomSet tlmpSet;
        public static BescomSet tlmcSet;
        public static BescomSet scfSet;
        public static BescomSet stspSet;
        public static BescomSet mastercfgSet;
        public static BescomSet FormulaSet;
        public static BescomSet sstsr;
        public static BescomSet sctl;
        public static BescomSet stlmr;
        public static GenericTable mastercfg_filteredTbl { get; set; }  // Table for Station data
        public static GenericTable dnpnetcfgdevice_Tbl { get; set; }  // Table for RTU data
        public static GenericTable dnpnetcfgport_Tbl { get; set; }  // Table for Channel data
        public static GenericTable dnpnetcfgctrlpt_Tbl { get; set; }  // Table for RTU_Control
        public static GenericTable Substation_names { get; set; }
        public Dictionary<string, int> UniqueStates { get; set; }

        public static GenericTable StateRec_Tbl { get; set; }
        public static GenericTable StateRec_Tbl123 { get; set; }
        public BescomParser(string inputDir)
        {
            stpSet = ParseSet(Path.Combine(inputDir, "STATES"));
            //stscSet = ParseSet(Path.Combine(inputDir, "STSC")); //No files available
            stsdSet = ParseSet(Path.Combine(inputDir, "STSD")); //No files available
            stsrSet = ParseSet(Path.Combine(inputDir, "STSR"));
            //stsiSet = ParseSet(Path.Combine(inputDir, "STSI"));
            // stsrSet = ParseSet(Path.Combine(inputDir, "STATUS"));
            stspSet = ParseSet(Path.Combine(inputDir, "STSP"));
            ctlSet = ParseSet(Path.Combine(inputDir, "CONTROL"));
            tlmaSet = ParseSet(Path.Combine(inputDir, "TLMA")); //No files available
           tlmdSet = ParseSet(Path.Combine(inputDir, "TLMD"));
           tlmpSet = ParseSet(Path.Combine(inputDir, "TLMP"));
            tlmrSet = ParseSet(Path.Combine(inputDir, "TELEMETRY"));
            tlmcSet = ParseSet(Path.Combine(inputDir, "TLMC"));
            scfSet = ParseSet(Path.Combine(inputDir, "SCF"));
            FormulaSet = ParseSet(Path.Combine(inputDir, "Formula"));
            mastercfgSet = ParseSet(Path.Combine(inputDir, "MASTERCFG"));
            sstsr = ParseSet(Path.Combine(inputDir, "SSTR"));
            sctl = ParseSet(Path.Combine(inputDir, "SCTL"));
            stlmr = ParseSet(Path.Combine(inputDir, "STLMR"));
            Substation_names = new GenericTable(Path.Combine(inputDir, "Substation_names.csv"), "Display_no.", ',', '\"');
            mastercfg_filteredTbl = new GenericTable(Path.Combine(inputDir, "MASTERCFG_description.csv"), "station_number", ',', '\"');
            dnpnetcfgdevice_Tbl = new GenericTable(Path.Combine(inputDir, "dnpnetconfig_device.csv"), "name", ',', '\"'); 
            dnpnetcfgport_Tbl = new GenericTable(Path.Combine(inputDir, "dnpnetconfig_port.csv"), "name", ',', '\"'); 
            dnpnetcfgctrlpt_Tbl = new GenericTable(Path.Combine(inputDir, "dnpnetconfig_ControlPoint.csv"), "device", ',', '\"'); 
            StateRec_Tbl = new GenericTable(Path.Combine(inputDir, "StateRectbl.csv"), "RECORD1", ',', '\"');
            StateRec_Tbl123 = new GenericTable(Path.Combine(inputDir, "StateRectbl123.csv"), "RECORD3", ',', '\"');//RM 11072023:STATE Requirement.
            UniqueStates = new Dictionary<string, int>();
            BescomParser.dnpnetcfgdevice_Tbl.Sort("station");
            BescomParser.dnpnetcfgport_Tbl.Sort("ip_address");
        }      

        public BescomSet ParseSet(string inputDir)
        {
            BescomSet set = new BescomSet
            {
                Files = new Dictionary<string, BescomFile>()
            };
            if (Directory.Exists(inputDir))
            {
                set.Files = (TryParseACSTxts(inputDir));
                Logger.Info("Finish parsing " + inputDir.Split('\\').Last());
              
            }
                

            return set;
        }

        public Dictionary<string, BescomFile> TryParseACSTxts(string path)
        {
            Dictionary<string, BescomFile> files = new Dictionary<string, BescomFile>();

            foreach (string file in Directory.GetFiles(path))
            {
                if (Path.GetExtension(file).Equals(".TXT"))
                {
                    int rec = 0;
                    string value;
                    string key;

                    BescomFile acsFile = new BescomFile
                    {
                        Name = Path.GetFileNameWithoutExtension(file),

                    };

                    BescomRecord acsRecord = new BescomRecord();

                    List<string> lines = new List<string>();

                    lines.AddRange(File.ReadLines(file).ToList());

                    foreach (string s in lines)
                    {
                        string line = s.Trim();

                        if (line.StartsWith("# REC. NO.:"))
                        {
                            acsRecord = new BescomRecord();

                            rec = Convert.ToInt32(line.Split(':')[1].Trim());

                            acsRecord.InfoDict = new Dictionary<string, string>();
                        }
                        else if (line.Contains("="))
                        {
                            key = line.Split('=')[0].Trim();

                            value = line.Split('=')[1].Trim();
                            
                            if (value.Contains("\""))
                            {
                                value = value.Trim('\"');
                            }

                            acsRecord.InfoDict.Add(key, value);

                            if (!acsRecord.Record.Equals(rec))
                            {
                                acsRecord.Record = rec;
                            }
                        }
                        else if (line.StartsWith(";"))
                        {
                            // End of Record

                            if (acsFile.ACSRecords == null)
                            {
                                acsFile.ACSRecords = new List<BescomRecord>
                                {
                                    acsRecord
                                };
                            }
                            else
                            {
                                acsFile.ACSRecords.Add(acsRecord);
                            }

                        }
                    }
                    files.Add(acsFile.Name, acsFile);
                }
            }
            return files;
        }
    }
}
