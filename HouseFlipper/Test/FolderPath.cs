using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseFlipper.Test
{
    public class FolderPath
    {
        public string Raw;
        public string Quoted;
        public FolderPath(string raw, string quoted)
        {
            this.Raw = raw;
            this.Quoted = quoted;
        }
    }
}
