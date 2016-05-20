using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KazmonWatcher.Test.Common;
using System.Diagnostics;
using Common.IO;
using Test.Common;

namespace KazmonWatcher.Test
{
    public class Prototype
    {
        private string ExecutablePath;
        private StringDictionary ArgumentsDictionary;

        private string Arguments
        {
            get
            {
                if(ArgumentsDictionary==null || ArgumentsDictionary.Keys.Count==0)
                {
                    return string.Empty;
                }
                var argStr = string.Empty;
                var isFirst = true;
                foreach(string p in ArgumentsDictionary.Keys)
                {
                    if (isFirst) { isFirst = false; }
                    else
                    {
                        argStr += " ";
                    }
                    var v = ArgumentsDictionary[p];
                    argStr+=string.Format("{0} {1}", p, v);
                }
                return argStr;
            }
        }

        private TimeSpan DefaultTimeout = TimeSpan.FromSeconds(10);
        private StringDictionary arguments;

        public Prototype(string executablePath, StringDictionary argsDictionary)
        {
            this.ExecutablePath = executablePath;
            this.ArgumentsDictionary = argsDictionary;
            Validate();
        }

        private void Validate()
        {
            if(string.IsNullOrWhiteSpace(this.ExecutablePath))
            {
                throw new InvalidOperationException("Error: ExectuablePath must not be null or empty!");
            }
        }

        public Result Run()
        {
            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(ExecutablePath, Arguments);
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            var started = p.Start();
            if(!started)
            {
                throw new InvalidOperationException(string.Format("ERROR: Could not run '{0}'", ExecutablePath));
            }
            p.WaitForExit((int)this.DefaultTimeout.TotalMilliseconds);
            if(!p.HasExited)
            {

            }
            var r = new Result(FileIO.ReadAll(p.StandardOutput), FileIO.ReadAll(p.StandardError));
            return r;
        }
    }
}
