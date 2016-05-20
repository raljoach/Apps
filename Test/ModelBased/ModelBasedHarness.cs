using Common;
using Automata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace Test.ModelBased
{
    /*
    public abstract class ModelBasedHarness : StateMachine
    {
        private Strategy Strategy;
        public ModelBasedHarness(Strategy strategy=null) : base()
        {
            this.Strategy = strategy;
        }

        
        protected override void Initialize()
        {
            Logger.DebugWrite("PHASE INIT STARTED: Initializing model.....");
            //TODO: Put metrics on how many states, transitions are in model graph
            // Defining model
            // States: 48
            // Transitions: 128
            // Paths: 10
            HandleInitialize();
            Logger.DebugWrite("PHASE INIT COMPLETED");
            //Logger.Debug("Generating test model.....");
            Logger.Border();
            //throw new NotImplementedException();
        }
        

        protected abstract void HandleInitialize();
        
        protected override void SetStart()
        {
            Logger.Debug("PHASE SET START STATE COMMENCED: Setting start state.....");
            // TODO: Put what the start state is
            // START STATE: NULL STATE <ID: 00000> (HAS BEEN SET)
            // THEN PUT THE MACHINE IN THAT STATE
            HandleSetStartState();
            Logger.DebugWrite("PHASE SET START STATE COMPLETED");
            Logger.Border();
            //throw new NotImplementedException();
        }

        protected abstract void HandleSetStartState();

        protected abstract IEnumerable<object> Next();

        
        protected override bool Execute()
        {
            Logger.Debug("PHASE EXECUTION STARTED: Executing tests.....");
            Logger.Border();
            //Logger.Debug("Retrieving test cases.....");
            //Logger.Border();
            //var tests = DefineTests();
            Logger.Debug("TEST CASES:");
            Logger.Border();
            int runCount = 0, failCount = 0, total = 0; // tests.Keys.Count;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            foreach (var step in Next())
            {
                ++runCount;
                ++total;
                //var e = tests[transition];
                Logger.DebugWrite("{0}) {1}", runCount, step);
                //if (e.OutputMatch != null)
                //{
                //    Logger.Debug("\r\nEXPECTED OUTPUT: \r\n{0}", e.OutputMatch);
                //}
                //if (e.ErrorMatch != null)
                //{
                //    Logger.Debug("EXPECTED ERROR: \r\n{0}", e.ErrorMatch);
                //}
                Result a = StepExecute(step);
                string result = "PASSED";
                if (!Validate(a))
                {
                    ++failCount;
                    result = "FAILED";
                    Logger.Debug("TEST RESULT: ({0})\r\n", result);

                    //Logger.Debug("ACTUAL OUTPUT: \r\n{0}", a.Output);
                    //Logger.Debug("ACTUAL ERROR: \r\n{0}", a.Error);
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
            Logger.Debug("PHASE EXECUTION COMPLETED");
            Logger.Border();
            return failCount > 0;
        }
                
        protected override void CleanUp()
        {
            Logger.Debug("PHASE CLEAN-UP STARTED: Cleaning up state.....");
            Logger.Border();
            HandleCleanUp();
            Logger.Debug("PHASE CLEAN-UP COMPLETED");
            Logger.Border();
            throw new NotImplementedException();
        }

        protected virtual void HandleCleanUp() { }

        protected abstract Result StepExecute(object step);

        protected abstract bool Validate(Result actual);
    }*/
}
