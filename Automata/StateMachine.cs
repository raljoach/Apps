using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata
{
    public class StateMachine : Automata
    {
        public event Action<State> StateChanged;
        private State state;
        private State startState;

        public StateMachine(State[] states)
        {
            foreach (var s in states)
            {
                s.StateMachine = this;

                if (s.IsStart)
                {
                    startState = s;
                }
            }
            if (startState == null) { throw new ArgumentException(); }
            this.Current = startState;
        }


        public State Current
        {
            get { return this.state; }
            set
            {
                this.state = value;
                StateChanged?.Invoke(this.Current);
            }
        }

        public override void Run()
        {
            Transition trans = null;
            while ((trans = state.Next()) != null)
            {
                state.Execute(trans);
            }
        }

        public void Reset()
        {
            Current = startState;
        }
    }
}
