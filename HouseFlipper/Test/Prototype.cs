using Common;
using Common.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace HouseFlipper.Test
{
    class Prototype
    {
        private string dataFolderPath;
        private string houseFlipperExe;
        public Prototype(string dataFolderPath)
        {
            this.dataFolderPath = dataFolderPath;
            this.houseFlipperExe = Path.Combine(DirectoryIO.GetProjectDir(), @"..\..\HouseFlipper\bin\debug\HouseFlipper.exe");
            if(!File.Exists(houseFlipperExe))
            {
                throw new InvalidOperationException(string.Format("Error: Path does not exist '{0}'", houseFlipperExe));
            }
        }

        public TimeSpan Timeout { get; set; }

        public Result Run()
        {
            Process p = new Process();
            var args = CreateArgs(this.dataFolderPath);
            p.StartInfo = new ProcessStartInfo(houseFlipperExe, args);
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.StartInfo.UseShellExecute = false;
            var started = p.Start();
            if(!started)
            {
                throw new InvalidOperationException(string.Format("Process did not start '{0}'", houseFlipperExe));
            }
            p.WaitForExit((int)this.Timeout.TotalMilliseconds);
            string error = null;
            string output = null;
            if (!p.HasExited)
            {
                //error = "Program stuck runnning!\r\n";                
                p.Kill();
                error += FileIO.ReadAll(p.StandardError);
                output = FileIO.ReadAll(p.StandardOutput);
            }
            else
            {
                error = FileIO.ReadAll(p.StandardError);
                output = FileIO.ReadAll(p.StandardOutput);
            }
            return new Result(output,error);
        }

        private string CreateArgs(string dataFolderPath)
        {
            return dataFolderPath;
        }
    }
}
