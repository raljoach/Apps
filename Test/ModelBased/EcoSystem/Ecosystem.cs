using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.ModelBased.System
{
    public class Ecosystem
    {
        private List<Participant> _participants;
        public Ecosystem(Participant[] participants)
        {
            if(participants==null && participants.Length==0)
            {
                throw new ArgumentException();
            }
            _participants = new List<Participant>();
            _participants.AddRange(participants);
        }
    }
}
