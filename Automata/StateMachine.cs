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

        public StateMachine(State[] states/*, Driver driver*/)
        {
            State startState = null;
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


        public virtual State Current { get { return this.state; } set { this.state = value; } }
    }
    /*
    public class States : IEnumerable
    {
        private State[] _states;
        public States(State[] stateArray)
        {
            _states = new State[stateArray.Length];

            for (int i = 0; i < stateArray.Length; i++)
            {
                _states[i] = stateArray[i];
            }
        }

        // Implementation for the GetEnumerator method.
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public StateMachine GetEnumerator()
        {
            return new StateMachine(_states);
        }
    }
    */

}
