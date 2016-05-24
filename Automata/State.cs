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
        Random _random = new Random();
        public State(IParameter[] parameters, Transition[] transitions)
        {
            _parameters = parameters;
            _transitions = transitions;
            foreach(var t in _transitions)
            {
                t.Start = this;
            }
        }

        public State(State state) : this(state.Parameters,state.Transitions)
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

        public StateMachine StateMachine { get; set; }

        public Transition[] Transitions
        {
            get { return _transitions; }
            set { _transitions = value; }
        }

        public Transition Next()
        {
            var list = EnabledTransitions();
            var pos = _random.Next(0, list.Count);
            return list[pos];
        }

        public virtual void Execute(Transition transition)
        {
            StateMachine.Current = transition.End;
            //EventFire();
        }

        private List<Transition> EnabledTransitions()
        {
            throw new NotImplementedException();
        }
    }

    public interface IParameter
    {

    }

    public interface IResponse
    {

    }


    public class Transition
    {
        public State Start { get; set; }
        public State End { get; set; }
        public Action Action { get; set; }
        //public List<IParameter> ActionArguments { get; set; }
        //public IResponse Response { get; set; }
    }
}
