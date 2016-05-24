using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata
{
    public abstract class StateMachineOld : Automata
    {
        protected State Current { get; set; }
        protected abstract void Initialize();
        protected abstract void SetStart();
        protected abstract bool Execute();
        protected abstract void CleanUp();
        public override void Run()
        {
            try
            {
                Initialize();
                SetStart();
                Execute();
            }
            finally
            {
                CleanUp();
            }
        }
    }
}
