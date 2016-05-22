using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata
{
    public class State
    {
        IParameter[] _parameters;
        Transition[] _transitions;
        public State(IParameter[] parameters)
        {
            _parameters = parameters;//new Parameters(parameters);
        }

        public State(State state) : this(state.Parameters)
        {            
            this.IsStart = state.IsStart;
            this.Transitions = state.Transitions;
            this.StateMachine = state.StateMachine;
        }

        public bool IsStart { get; set; }

        public IParameter[] Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        public StateMachine StateMachine { get; internal set; }

        public Transition[] Transitions
        {
            get { return _transitions; }
            set { _transitions = value; }
        }
    }

    public interface IParameter
    {

    }
    /*
    public class Parameters 
    {
        private List<Parameter> _parameters;
        public Parameters(Parameter[] parameters)
        {
            if(parameters==null || parameters.Length==0)
            {
                new ArgumentException();
            }
            _parameters = new List<Parameter>();
            _parameters.AddRange(_parameters);
        }
    }*/

    public class Transition
    {

    }
    /*
    public class Transitions : IEnumerable<Transition>
    {
        private List<Transition> _transitions;
        public Transitions(Transition[] transitions)
        {
            if (transitions == null || transitions.Length == 0)
            {
                new ArgumentException();
            }
            _transitions = new List<Transition>();
            _transitions.AddRange(transitions);
        }
    }*/
}
