using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Input
    {
        public DataFolder DataFolder;

        public Input(DataFolder datafolder)
        {
            this.DataFolder = datafolder;
        }

        public override string ToString()
        {
            string output = "\r\n";
            output += "\r\n";
            var cols = new List<Column>();
            cols.Add(new Column(@"\data", this.DataFolder));
            var grid = new Grid(cols);
            output += grid.ToString();
            output += "\r\n";
            return output;
        }
    }
}
