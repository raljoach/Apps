using Automata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace Test.ModelBased
{
    public class TestMachine : StateMachine
    {
        public TestMachine(TestState[] states, Driver driver) : base(states)
        {
            foreach (var s in states)
            {
                s.Driver = driver;
            }
        }
    }

    public class TestState : State
    {
        public TestState(State state) : base(state)
        {
        }

        public Driver Driver { get; internal set; }
    }
}
