using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseFlipper.Test
{
    class Grid
    {
        private List<Column> cols;

        public Grid(List<Column> cols)
        {
            this.cols = cols;
        }

        public override string ToString()
        {
            var output = string.Empty;
            WriteHeaders(ref output);
            WriteBorder(ref output);
            WriteValues(ref output);
            return output;
        }

        private void WriteValues(ref string output)
        {
            int maxValCount = MaxValCount(cols);

            for (var rowNum = 0; rowNum < maxValCount; rowNum++)
            {
                output = WriteRow(output, rowNum);
            }
        }

        private string WriteRow(string output, int rowNum)
        {
            foreach (var c in cols)
            {
                output = WriteField(output, c, rowNum);

            }
            output += "\r\n";
            return output;
        }

        private int MaxValCount(List<Column> cols)
        {
            int max = -1;
            foreach (var c in cols)
            {
                if (c.Values.Count > max)
                {
                    max = c.Values.Count;
                }
            }
            return max;
        }

        private static string WriteField(string output, Column c, int index)
        {
            string v = null;
            if (index < c.Values.Count)
            {
                v = c.Values[index];
                ///v = v.PadRight(c.MaxLength-v.Length);
            }
            else
            {
                v = "".PadRight(c.MaxLength);
            }
            output += v + " ";
            return output;
        }

        private void WriteBorder(ref string output)
        {
            foreach (var c in cols)
            {
                var length = c.MaxLength;
                var b = "".PadRight(length, '-');
                output += b + " ";
            }
            output += "\r\n";
            //return output;
        }

        private void WriteHeaders(ref string output)
        {
            foreach (var c in cols)
            {
                WriteHeader(ref output, c);
            }
            output += "\r\n";
            //return output;
        }

        private static void WriteHeader(ref string output, Column c)
        {
            var length = c.MaxLength;
            var hFormat = "{0" + ",-" + length + "}";
            var header = string.Format(hFormat, c.Header);
            output += header + " ";
            //return output;
        }

        private string Write(string v)
        {
            throw new NotImplementedException();
        }
    }
}
