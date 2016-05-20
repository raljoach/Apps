using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Harness
{
    class TestLoopResult
    {
        public TestLoopResult(int numTestRuns, bool passed, Stopwatch stopWatch)
        {
            this.NumTestRuns = numTestRuns;
            this.Passed = passed;
            this.Stopwatch = stopWatch;
        }

        public double NumTestRuns { get; private set; }
        public bool Passed { get; private set; }
        public Stopwatch Stopwatch { get; private set; }
    }
}
