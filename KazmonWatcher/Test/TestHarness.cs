using Common;
using KazmonWatcher.Test.Common;
using KazmonWatcher.Test.Common.Parameters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;
using Test.Harness;

namespace KazmonWatcher.Test
{
    class TestHarness : TestHarnessBase
    {
        static void Main(string[] args)
        {
            new TestHarness().Run();            
        }

        public TestHarness():base(new Demo()) { }
      
        protected override Dictionary<global::Test.Common.Input, ExpectedResult> DefineTests()
        {
            return new Dictionary<global::Test.Common.Input, ExpectedResult>()
            {
                { new TestInput(KazmonUrl.UnusedParameter), new ExpectedResult(new PartialMatch("Using Kazmon prod site:")) }
            };
        }        
    }
}
