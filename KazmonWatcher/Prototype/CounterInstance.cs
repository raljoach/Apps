using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KazmonWatcher.Prototype
{
    public class CounterInstance
    {
        public CounterInstance(double count, double average)
        {
            Count = count;
            Average = average;
        }

        public double Count { get; set; }
        public double Average { get; set; }
    }
}
