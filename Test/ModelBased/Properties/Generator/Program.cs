using Common;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator
{    
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger.Debug("Getting parameters....");
            Logger.Border();
            var signature = "Call(MlsNumber,Status,PostalCode,Address,City,CloseDate)";
            var details =
                @"
                - MlsNumber: (String) Null, Blank, NonBlank
                - Status: (String) Null, ACT, SLD
                - PostalCode: (String) Null, Blank, NonBlank
                - Address: (String) Null, Blank, NonBlank
                - City: (String) Null, Blank, NonBlank
                - CloseDate: (DateTime) Null, Blank, NonBlank
                ";

            var index = signature.IndexOf('(');
            var paramGroup = signature.Substring(index + 1).Replace(")", string.Empty);
            var parameters = paramGroup.Split(',');
            StringDictionary lookup = CreateLookup(details);
            var list = new List<Parameter>();
            var file = "pict_input.txt";
            var specBuilder = new StringBuilder();
            foreach (var p in parameters)
            {
                var paramName = p;
                var paramValues = lookup[paramName];
                index = paramValues.IndexOf(')');
                var paramType = paramValues.Substring(0, index).Replace("(", string.Empty);
                paramValues = paramValues.Substring(index + 1).Trim();
                list.Add(new Parameter(paramName, paramType, paramValues));

                var paramSpec = string.Format("{0}: {1}", paramName, paramValues);
                Logger.Debug(paramSpec);
                specBuilder.AppendLine(paramSpec);
            }
            Logger.Border();
            
            var spec = specBuilder.ToString();
            FileIO.Write(file, spec);
            PictResult result = RunPict(file);
            var output = result.Output;
            var inputs = new List<string>();
            var inputsStr = string.Empty;

            var isFirst = true;
            foreach(var l in output.Split('\n'))
            {
                if (isFirst) { isFirst = false; continue; }
                if (string.IsNullOrWhiteSpace(l)) { continue; }
                var line = l.Replace("\t", ",").Replace("\r", string.Empty);
                var input = "- INPUT: {" + line + "} " + signature + " => null ";
                inputs.Add(input);
                Logger.Explicit(input);
                inputsStr += input + "\n";
            }

            file = "oo_details.txt";
            FileIO.Write(file, inputsStr);

            Logger.Border();
            Logger.Debug("Program has ended. Hit any key to exit!");
            Console.ReadKey();
        }

        private static PictResult RunPict(string file)
        {
            Logger.Debug("Running pict...");
            Logger.Border();
            var pictExe = @"C:\Program Files (x86)\PICT\pict.exe";
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(pictExe, file);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var started = p.Start();
            if(!started)
            {
                throw new InvalidOperationException(string.Format("Error: '{0}' did not start!",pictExe));
            }
            p.WaitForExit((int)TimeSpan.FromSeconds(10).TotalMilliseconds);
            if(!p.HasExited)
            {
                throw new InvalidOperationException(string.Format("Error: '{0}' is still running!", pictExe));
            }
            var output = FileIO.ReadAll(p.StandardOutput);
            var error = FileIO.ReadAll(p.StandardError);
            Logger.Debug("Generated pict output:");
            Logger.Debug();
            Logger.Debug(output);
            Logger.Border();
            return new PictResult(output, error);
        }

        public class PictResult
        {
            public string Output;
            public string Error;

            public PictResult(string output, string error)
            {
                Output = output;
                Error = error;
            }
        }

        private static StringDictionary CreateLookup(string details)
        {
            var dict = new StringDictionary();
            var lines = details.Split('\n');
            foreach (var l in lines)
            {
                if (string.IsNullOrWhiteSpace(l)) { continue; }
                var line = l.Replace("-", string.Empty).Trim();
                var index = line.IndexOf(':');
                var paramName = line.Substring(0, index);
                var paramValues = line.Substring(index + 1).Trim();
                dict.Add(paramName, paramValues);
            }
            return dict;
        }
    }
}
