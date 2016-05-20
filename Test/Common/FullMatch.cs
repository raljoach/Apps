using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    public class FullMatch : Match
    {
        public string Pattern;
        public FullMatch(string pattern)
        {
            this.Pattern = pattern;
        }

        public override bool Matches(string output)
        {
            return HandleEquals(output, Pattern);
        }

        public override string ToString()
        {
            return this.Pattern;
        }

        private static bool HandleEquals(string s1, string s2)
        {
            var comp = false;
            if (s1 == null && s2 == null)
            {
                comp = true;
            }
            else if (s1 == null || s2 == null)
            {
                comp = false;
            }
            else
            {
                comp = s1.ToLower().Equals(s2.ToLower());
            }

            return comp;
        }
    }
}
