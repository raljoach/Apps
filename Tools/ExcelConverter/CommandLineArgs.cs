//-----------------------------------------------------------------------
// <copyright file="CommandLineArgs.cs" company="MyCompany">
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
    /// Used to process command line arguments of executable.
    /// </summary>
    public class CommandLineArgs
    {
        /// <summary>
        /// Files commandline parameter flag.
        /// </summary>
        public const string FILES = "-files";

        /// <summary>
        /// Input directory commandline parameter flag.
        /// </summary>
        public const string INPUT_DIRECTORY = "-dir";

        /// <summary>
        /// Output directory commandline parameter flag
        /// </summary>
        public const string OUTPUT_DIRECTORY = "-out";

        /// <summary>
        /// Contains all valid commandline parameters flags.
        /// </summary>
        public static List<string> parameters = new List<string>();

        /// <summary>
        /// Initializes the <see cref="CommandLineArgs"/> class.
        /// </summary>
        static CommandLineArgs()
        {
            parameters = new List<string>()
            {
                FILES, INPUT_DIRECTORY, OUTPUT_DIRECTORY
            };
        }

        /// <summary>
        /// Determines whether the specified string is parameter.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>true if it is a parameter, otherwise false.</returns>
        public static bool IsParameter(string str)
        {
            if(string.IsNullOrEmpty(str))
            {
                return false;
            }
            return parameters.Contains(str.ToLower());
        }

        /// <summary>
        /// Extracts the files and out directory parameter values.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="files">The files.</param>
        /// <param name="outDir">The out dir.</param>
        /// <returns>true if the commandline arguments are valid, otherwise false</returns>
        public static bool Extract(string[] args, out List<string> files, out string outDir)
        {
            outDir = ".";
            files = new List<string>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == CommandLineArgs.INPUT_DIRECTORY)
                {
                    if ((i + 1) >= args.Length)
                    {
                        Console.WriteLine(CommandLineArgs.INPUT_DIRECTORY + " option requires a directory path.\n");
                        return false;
                    }
                    var dir = args[++i];
                    if (CommandLineArgs.IsParameter(dir))
                    {
                        Console.WriteLine(CommandLineArgs.INPUT_DIRECTORY + " option requires a directory path.\n");
                        return false;
                    }

                    if (!Directory.Exists(dir))
                    {
                        Console.WriteLine("'" + dir + "' does not exist!");
                        return false;
                    }

                    outDir = dir;
                    files.AddRange(Directory.GetFiles(dir));
                }
                else if (args[i].ToLower() == CommandLineArgs.OUTPUT_DIRECTORY)
                {
                    if ((i + 1) >= args.Length)
                    {
                        Console.WriteLine(CommandLineArgs.OUTPUT_DIRECTORY + " option requires a directory path.\n");
                        return false;
                    }
                    var dir = args[++i];
                    if (CommandLineArgs.IsParameter(dir))
                    {
                        Console.WriteLine(CommandLineArgs.OUTPUT_DIRECTORY + " option requires a directory path.\n");
                        return false;
                    }

                    if (!Directory.Exists(dir))
                    {
                        Console.WriteLine("'" + dir + "' does not exist!");
                        return false;
                    }

                    outDir = dir;
                }
                else if (args[i].ToLower() == CommandLineArgs.FILES)
                {
                    if ((i + 1) >= args.Length)
                    {
                        Console.WriteLine(CommandLineArgs.FILES + " option requires one or more file paths.\n");
                        return false;
                    }
                    ++i;
                    while (i < args.Length && !CommandLineArgs.IsParameter(args[i]))
                    {
                        files.Add(args[i]);
                        ++i;
                    }
                }
            }
            return files.Count>0;
        }

        /// <summary>
        /// Prints the usage statement for running this program.
        /// </summary>
        public static void PrintUsage()
        {
            Console.WriteLine("DESCRIPTION: Converts one or more Excel files into CSV files.");            
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine("excelconverter [ " + FILES + " <excelfilenames> | " + INPUT_DIRECTORY  + " <inputdir>] [ " + OUTPUT_DIRECTORY + " <outdir>]");
            Console.WriteLine("<excelfilenames>: One or more Excel file paths to be converted to CSV format.");
            Console.WriteLine("<inputdir>: Directory path containing ONLY Excel files to be converted to CSV format.");
            Console.WriteLine("<outdir>: Output directory where converted CSV files will be written to.");
            Console.WriteLine();
            Console.WriteLine("Example: excelconverter " + FILES + " sample.xlsx");
            Console.WriteLine("Example: excelconverter " + FILES + " sample.xlsx sample2.xlsx " + OUTPUT_DIRECTORY + " .");
            Console.WriteLine("Example: excelconverter " + INPUT_DIRECTORY + " .\\input " + OUTPUT_DIRECTORY + " .\\output");
            Console.WriteLine();
            Console.WriteLine("NOTE: The output location of the converted files will be local to where this executable is ran OR will be in the same directory as -dir location.");
            Console.WriteLine("NOTE: If two input files have the same name in 2 different directories, one of the files converted CSV output will overwrite the other");
        }
    }
}
