//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="MyCompany">
//     Copyright (c) MyCompany. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelConverter
{
    /// <summary>
    /// Commandline program for converting Excel files into csv files.
    /// Outputs a status log which contains the processing status of each file.
    /// Outputs an error log which contains any exceptions encountered while processing a file.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry point of execution.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Must supply arguments to commandline.\n");
                CommandLineArgs.PrintUsage();
                return;
            }

            List<string> files;
            string outDir;
            if (!CommandLineArgs.Extract(args, out files, out outDir))
            {
                CommandLineArgs.PrintUsage();
                return;
            }

            int success = 0;
            int failed = 0;
            using (StreamWriter statusLog = new StreamWriter("excelconverter_status.log"))
            {
                using (StreamWriter errorLog = new StreamWriter("excelconverter_error.log"))
                {
                    foreach (string fileName in files)
                    {
                        string status = "Converting file: '" + fileName + "'";
                        try
                        {
                            ExcelFileConverter.ConvertToCsv(fileName, outDir);
                            success++;
                            status += " (SUCCESS)";
                            
                        }
                        catch (Exception ex)
                        {
                            failed++;
                            status += " (FAILED)";
                            var exStr = ex.ToString();
                            status += "\n" + exStr;

                            errorLog.WriteLine("Error converting " + fileName + ":");
                            errorLog.WriteLine(exStr);
                            errorLog.WriteLine();

                        }
                        Console.WriteLine(status);
                        statusLog.WriteLine(status);
                    }
                }

                StringBuilder results = new StringBuilder("Conversion of Excel files to csv files completed.").AppendLine();
                results.AppendLine(string.Format("Successfully converted: {0} files", success));
                results.AppendLine(string.Format("Failed to convert: {0} files", failed));
                var resultStr = results.ToString();
                Console.WriteLine(resultStr);
                statusLog.WriteLine(resultStr);
            }
        }                
    }
}
