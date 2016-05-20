using Automata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.ModelBased.System
{
    internal class Software : Automata.Automata
    {
        private ExecutionModel executionModel;
        internal Software(Action main)
        {
            this.executionModel = new ExecutionModel(new Program(main));
        }

        internal Software(ExecutionModel executionModel)
        {
            this.executionModel = executionModel;
        }
    }

    public class ExecutionModel //: StateMachine
    {
        private Program  program;
        public ExecutionModel(Action main)
        {
            this.program = new Program(main);
        }
        internal ExecutionModel(Program program)
        {
            this.program = program;
        }

        internal ExecutionModel(Program[] programs)
        {

        }
    }

    public class Program
    {
        public Action Main;
        public Program(Action main)
        {
            this.Main = main;
        }
    }
}
