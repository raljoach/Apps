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
            using (var sr = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.ReadWrite)))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static string ReadAll(string file)
        {
            var stream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite);
            return ReadAll(stream);
        }

        public static string ReadAll(Stream stream)
        {            
            using (var sr = new StreamReader(stream))
            {
                return sr.ReadToEnd();
            }
        }

        public static string ReadAll(StreamReader sr)
        {
            using (sr)
            {
                return sr.ReadToEnd();
            }
        }

        public static void Write(string file, string content)
        {
            using (var sw = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.ReadWrite)))
            {
                sw.Write(content);
                sw.Flush();
            }
        }
    }
}
