using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace Test.Common
{
    public abstract class Driver
    {
        public abstract void SetUp(Input input);

        public abstract Result Run();
    }
}
