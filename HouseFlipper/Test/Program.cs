using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{        
    class Program
    {
        static void Main(string[] args)
        {
            Logger.Debug("Executing tests....");
            while(!TestCases())
            {
                Logger.Debug("Would you like to rerun the tests? [y|n] (Default:y)");
                var entry = Console.ReadKey();
                if(entry.Key == ConsoleKey.Enter)
                {
                    continue;
                }
                var ch = entry.KeyChar.ToString().ToLower();
                if(ch=="n")
                {
                    break;
                }
            }
            Console.WriteLine("Program has ended. Hit any key to exit");
            Console.ReadKey();
        }

        private static bool TestCases()
        {
            var tests = new Dictionary<Input, Result>()
            {
                { new Input(DataFolder.NotExistent), new Result("Missing data folder") }
            };

            Logger.Border();
            Logger.Debug("TEST CASES:");
            Logger.Border();
            int count = 0, failCount = 0, total = tests.Keys.Count;
            foreach(var input in tests.Keys)
            {
                ++count;
                Logger.DebugWrite("{0}) {1}", count, input);
                var e = tests[input];                
                Demo demo = new Demo(input);
                Result a = demo.Run();
                string result = "CORRECT";
                if(a!=e)
                {
                    ++failCount;
                    result = "INCORRECT";
                    break;
                }
                Logger.Debug(" ({0})", result);
            }
            Logger.Border();
            Logger.Debug("Total: {0}", total);
            Logger.Border();
            Logger.Debug("Skipped: {0}", total - count);
            Logger.Border();
            Logger.Debug("Executed: {0}", count);
            Logger.Border();
            Logger.Debug("Failed: {0}", failCount);
            Logger.Debug("Passed: {0}", count - failCount);
            Logger.Border();
            return failCount>0;
        }
    }
}
