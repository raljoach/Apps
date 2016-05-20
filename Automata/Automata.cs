using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Automata
{
    public abstract class Automata
    {
        protected State state;
        public virtual void Run() { }
    }
}
