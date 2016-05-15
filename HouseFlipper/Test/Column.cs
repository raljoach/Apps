using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Column
    {
        public string Header;
        public List<string> Values = new List<string>();
        public int MaxLength;

        public Column(string header, params object[] values)
        {
            this.Header = header;
            MaxLength = header.Length;
            var list = new List<string>();
            foreach (var o in values)
            {
                var s = o as string;
                if (s == null)
                {
                    s = o.ToString();
                }
                Values.Add(s);
                if (MaxLength < s.Length)
                {
                    MaxLength = s.Length;
                }
            }

            Pad(ref Header, MaxLength);

            for (var j = 0; j < Values.Count; j++)
            {
                Values[j] = Pad(Values[j], MaxLength);
            }
        }

        private static string Pad(string someStr, int maxLength)
        {
            if (maxLength > someStr.Length)
            {
                someStr = someStr.PadRight(maxLength - someStr.Length);
            }
            return someStr;
        }

        private static void Pad(ref string someStr, int maxLength)
        {
            someStr = Pad(someStr, maxLength);
        }
    }
}
