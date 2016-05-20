using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KazmonWatcher.Test.Common.Parameters;
using Test.Common;

namespace Test.Common
{
    public class TestInput : Input
    {
        public KazmonUrl KazmonUrl;

        public TestInput(KazmonUrl kazmonUrl = KazmonUrl.UnusedParameter)
        {
            KazmonUrl = kazmonUrl;
        }
    }
}
