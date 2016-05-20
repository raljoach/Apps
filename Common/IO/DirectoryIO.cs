using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO
{
    public static class DirectoryIO
    {
        public static string GetProjectDir()
        {
            var exeLoc = Environment.CurrentDirectory;
            var projDir = exeLoc;
            while (!projDir.EndsWith("bin"))
            {
                projDir = Path.GetDirectoryName(exeLoc);
            }
            projDir = Path.GetDirectoryName(projDir);
            return projDir;
        }
    }
}
