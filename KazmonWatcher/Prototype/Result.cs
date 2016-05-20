using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KazmonWatcher.Prototype
{
    public class Result
    {
        public Result(double sum, double sqSum, double avg, double count, double? val)
        {
            this.Sum = sum;
            this.SqSum = sqSum;
            this.Average = avg;
            this.Count = count;
            this.SumOfSquares = val;
        }

        public double Sum { get; set; }
        public double SqSum { get; set; }
        public double Average { get; set; }
        public double Count { get; set; }
        public double? SumOfSquares { get; set; }

        // Sample standard deviation, using n-1
        public double StandardDeviation { get; set; }
    }
}
