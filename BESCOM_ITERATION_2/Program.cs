using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OSII.ConversionToolkit;
using OSII.DatabaseConversionToolkit;
using OSII.DatabaseToolkit.Dat;
//using Common;
//using Common.Databases;

namespace AESES_Databases
{
    class Program
    {
        private static string inputDir;


        static void Main(string[] args)
        {
            inputDir = null;
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-f":
                        if (i < args.Length - 1)
                        {
                            inputDir = args[i + 1];
                            i++;
                        }
                        break;
                    case "--help":
                        PrintUsage();
                        break;

                    default:
                        Console.WriteLine("Invalid parameter '{0}'", args[i]);
                        return;
                }
            }

           
            ConversionSettings.Start("BESCOM_NEW");
            Logger.OpenXMLLog("BESCOM_db");

           

            BescomParser parser = new BescomParser(inputDir);            

            BescomConverter converter = new BescomConverter();

            Database[] dbs = new Database[] { converter.ScadaDB, converter.StatesDB , converter.FepDB, converter.OpenCalcDB, converter.ICCPDB};
            foreach (Database db in dbs)
            {
                db.Write(ConversionSettings.FullDataPath);
            }

            Logger.CloseXMLLog();

            //ArbiterSettings.End(true, "", "");
            DatabaseSettings.End(false);
            ConversionSettings.End(true, "", "");
        }
        public static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine("Usage:  ");
            Console.WriteLine("FPWC_Databases_new.exe -f inputDir [options]");
            Console.WriteLine();
            Console.WriteLine("Program will convert ACS databases from inputDir and generate .DAT files with their information");
            Console.WriteLine("<options>");
            Console.WriteLine("--help\t\tPrint usage information");
        }
    }
}
