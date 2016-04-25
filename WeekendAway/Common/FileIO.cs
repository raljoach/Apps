using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class FileIO
    {
        public static IEnumerable<string> ReadFrom(string file)
        {
            using (var sw = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.ReadWrite)))
            {
                string line;
                while ((line = sw.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static string ReadAll(string file)
        {
            using (var sw = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.ReadWrite)))
            {
                return sw.ReadToEnd();
            }
        }
    }
}
