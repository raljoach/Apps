using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
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
            p.Start();
            string error = FileIO.ReadAll(p.StandardError);
            string output = FileIO.ReadAll(p.StandardOutput);
            return new Result(output,error);
        }

        private string CreateArgs(string dataFolderPath)
        {
            return dataFolderPath;
        }
    }
}
