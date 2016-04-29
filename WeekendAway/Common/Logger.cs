using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Logger
    {
        public static void Debug(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }

        public static void DebugWrite(string message, params object[] args)
        {
            Console.Write(message, args);
        }
        public static void Debug()
        {
            Console.WriteLine();
        }

        public static void Debug(Dictionary<string, string> table)
        {
            foreach (var key in table.Keys)
            {
                Debug("{0} => {1}", key, table[key]);
            }
        }

        public static void Border()
        {
            Border('-');
        }

        public static void Border(char ch)
        {
            Debug("".PadLeft(Console.WindowWidth, ch));
        }
    }
}
