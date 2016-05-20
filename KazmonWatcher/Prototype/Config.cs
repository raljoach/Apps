using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KazmonWatcher.Prototype
{
    public class Config
    {
        public List<string> Environments { get; set; }
        public List<string> Counters { get; set; }

        public Creds Creds { get; set; }
    }
}
