using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit
{
    class CountAverage
    {
        public CountAverage(double count, double avg)
        {
            this.Count = count;
            this.Average = avg;
        }
        public double Count { get; set; }
        public double Average { get; set; }
    }
    class Result
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
    class Program
    {
        static void Main(string[] args)
        {
            //Freq
            List<CountAverage> values = new List<CountAverage>()
            {
                new CountAverage(44641,7.65),
                new CountAverage(6557,39.7),
                //new CountAverage(3,60933),
                new CountAverage(258686,7.54),
                new CountAverage(12360,173)
            };

            var result = StandardDeviation(values);
            Logger.Debug("Sum: {0}", result.Sum);
            Logger.Debug("Count: {0}", result.Count);
            Logger.Border();
            Console.WriteLine("Average: {0}", result.Average);
            Console.WriteLine("Standard deviation: {0}", result.StandardDeviation);
            Logger.Border();
            var tmp = result.StandardDeviation * 2;
            Logger.Debug("Z: +2 => {0}", tmp + result.Average);
            Logger.Debug("Z: -2 => {0}", tmp - result.Average);
            Logger.Border();
            Logger.Debug("1.5*Avg: {0}", result.Average * 1.5);
            Logger.Border();
            Logger.Debug("Z values:");
            var above = new List<Tuple<int,double>>();
            for (int j = 0; j < values.Count; j++)
            {
                var ca = values[j];
                var z = ZScore(ca, result);
                Logger.Debug("[{0}] {1} => Z:{2}", j+1, ca.Average, z);
                if (z >= 2)
                {
                    above.Add(new Tuple<int,double>(j,z));
                }
            }
            Logger.Border();
            Logger.Debug("Values above Z=2:");
            foreach(var t in above)
            {                
                //var z = ZScore(ca, result);
                var j = t.Item1;
                var z = t.Item2;
                var ca = values[j];
                Logger.Debug("[{0}] {1} => Z:{2}", j+1, ca.Average, z);
            }
            Logger.Border();
            Console.WriteLine();
            Console.WriteLine("Program has ended. Hit any key to exit.");
            Console.ReadKey();
        }

        private static double ZScore(CountAverage ca, Result result)
        {
            var diff = ca.Average - result.Average;
            return diff / result.StandardDeviation;
        }

        private static Result StandardDeviation(List<CountAverage> values)
        {
            var result = SumOfSquares(values);
            var count = result.Count;
            var avg = result.Average;
            result.StandardDeviation = -1;
            if (result.SumOfSquares.HasValue)
            {
                var variance = (double)result.SumOfSquares / (count - 1);
                var std = Math.Sqrt(variance);
                result.StandardDeviation = std;             
            }
            return result;
        }

        private static Result SumOfSquares(List<CountAverage> values)
        {
            double sum = 0.0;
            double sqSum = 0.0;
            //double count = instances.Count;
            double count = 0.0;
            foreach (var ca in values)
            {
                sum += ca.Average*ca.Count;
                count += ca.Count;
                sqSum += Math.Pow(ca.Average, 2)*ca.Count;
            }
            double avg = sum / count;
            var ss = SumOfSquares(avg, count, sum, sqSum);
            return new Result(sum, sqSum, avg, count, ss);
        }

        private static double? SumOfSquares(double average, double count, double sum, double sqSum)
        {
            if (count > 0)
            {
                return Math.Max(sqSum - average * sum, 0);
            }
            return null;
        }
    }
}
