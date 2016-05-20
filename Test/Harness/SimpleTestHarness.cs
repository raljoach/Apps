using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Test.Common;

namespace Test.Harness
{
    public abstract class SimpleTestHarness : TestHarness
    {
        private Driver Driver;
        public SimpleTestHarness(Driver driver)
        {
            Driver = driver;
        }

        protected abstract TestTable DefineTests();

        public void EndlessRun()
        {
            while(true)
            {
                HandleRun(false, true, TimeSpan.FromSeconds(0));                
            }
        }

        public bool Run()
        {
            return HandleRun(true,true, TimeSpan.FromSeconds(0));
        }

        private bool HandleRun(bool pause, bool exitOnFail, TimeSpan delayBetweenRuns)
        {
            Logger.Debug("Executing tests....");
            Logger.Border();
            var result = TestLoop(pause,exitOnFail, delayBetweenRuns);
            Stopwatch stopWatch = result.Stopwatch;
            double numTestRuns = result.NumTestRuns;
            bool passed = result.Passed;
            Logger.Border();
            Logger.Debug("TOTAL TEST TIME: {0}h:{1}m:{2}s:{3}ms", stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds);
            TimeSpan avgRun = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds / numTestRuns);
            Logger.Debug("PER TEST RUN AVERAGE: {0}h:{1}m:{2}s:{3}ms", avgRun.Hours, avgRun.Minutes, avgRun.Seconds, avgRun.Milliseconds);
            Logger.Debug("TOTAL RUNS: {0}", numTestRuns);

            Logger.Border();
            Logger.Debug("Program has ended. Hit any key to exit.");
            Console.ReadKey();
            return passed;
        }

        private TestLoopResult TestLoop(bool pause, bool exitOnFail, TimeSpan delayBetweenRuns)
        {
            var numTestRuns = 0;
            bool passed = false;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            while (true)
            {
                ++numTestRuns;
                passed = !TestCases(numTestRuns);
                if(exitOnFail)
                {
                    if(!passed)
                    {
                        break;
                    }
                }
                
                Logger.Debug("TEST RUN #{0} COMPLETED", numTestRuns);
                Logger.Border();
                Logger.Debug();
                if (pause)
                {
                    Logger.Debug("Would you like to rerun the tests? [y|n] (Default:y)");
                    var entry = Console.ReadKey();
                    Logger.Debug();
                    if (entry.Key == ConsoleKey.Enter)
                    {
                        continue;
                    }
                    var ch = entry.KeyChar.ToString().ToLower();
                    if (ch == "n")
                    {
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(delayBetweenRuns);
                }
            }

            stopWatch.Stop();
            if (!passed)
            {
                ++numTestRuns;
                Logger.Debug("TEST RUN #{0} COMPLETED", numTestRuns);
                Logger.Border();
                Logger.Debug();
            }

            return new TestLoopResult(numTestRuns,passed,stopWatch);
        }

        private bool TestCases(int testRunNum)
        {
            Logger.Debug("Retrieving test cases.....");
            Logger.Border();
            var tests = DefineTests();
            Logger.Debug("TEST RUN #{0} STARTED", testRunNum);
            Logger.Border();
            int runCount = 0, failCount = 0, total = tests.Keys.Count;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var input in tests.Keys)
            {
                ++runCount;
                var e = tests[input];
                Logger.DebugWrite("{0}) \r\nINPUT: {1}", runCount, input);
                if (e.OutputMatch != null)
                {
                    Logger.Debug("\r\nEXPECTED OUTPUT: \r\n{0}", e.OutputMatch);
                }
                if (e.ErrorMatch != null)
                {
                    Logger.Debug("EXPECTED ERROR: \r\n{0}", e.ErrorMatch);
                }
                Result a = Execute(input);
                string result = "PASSED";
                if (!e.Equals(a))
                {
                    ++failCount;
                    result = "FAILED";
                    Logger.Debug("TEST RESULT: ({0})\r\n", result);

                    Logger.Debug("ACTUAL OUTPUT: \r\n{0}", a.Output);
                    Logger.Debug("ACTUAL ERROR: \r\n{0}", a.Error);
                    break;
                }
                Logger.Debug("TEST RESULT: ({0})", result);
                Logger.Border();
            }
            stopWatch.Stop();
            double passCount = runCount - failCount;
            double skipCount = total - runCount;
            Logger.Debug("Total: {0}", total);
            Logger.Border();
            Logger.Debug("Skipped: {0}", skipCount);
            Logger.Border();
            Logger.Debug("Executed: {0}", runCount);
            Logger.Border();
            Logger.Debug("Failed: {0}", failCount);
            Logger.Debug("Passed: {0}", passCount);
            Logger.Border();
            Logger.Debug("% PASS: {0}%", 100 * passCount / total);
            Logger.Debug("% FAIL: {0}%", 100 * failCount / total);
            Logger.Debug("% SKIPPED: {0}%", 100 * skipCount / total);
            Logger.Border();
            Logger.Debug("TEST EXECUTION: {0}h:{1}m:{2}s:{3}ms", stopWatch.Elapsed.Hours, stopWatch.Elapsed.Minutes, stopWatch.Elapsed.Seconds, stopWatch.Elapsed.Milliseconds);
            TimeSpan avgRun = TimeSpan.FromMilliseconds(stopWatch.ElapsedMilliseconds / runCount);
            Logger.Debug("PER TEST AVERAGE: {0}h:{1}m:{2}s:{3}ms", avgRun.Hours, avgRun.Minutes, avgRun.Seconds, avgRun.Milliseconds);
            Logger.Border();
            return failCount > 0;
        }

        private Result Execute(Input input)
        {
            Driver.SetUp(input);
            Result a = Driver.Run();
            return a;
        }
    }
}
