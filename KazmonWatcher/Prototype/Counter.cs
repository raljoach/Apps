using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KazmonWatcher.Prototype
{
    public class Counter
    {
        public List<CounterInstance> Instances { get; set; }
        public CounterInstance Total { get; set; }
    }
}
