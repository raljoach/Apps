using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    public class PartialMatch : Match
    {
        private List<string> Patterns = new List<string>();
        public PartialMatch(params string[] patterns)
        {
            if(patterns == null || patterns.Length==0)
            {
                throw new InvalidOperationException();
            }
            this.Patterns.AddRange(patterns);
        }

        public override bool Matches(string output)
        {
            foreach (var p in Patterns)
            {
                if (!HandlePartialEquals(output, p))
                {
                    return false;
                }
            }
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach(var p in Patterns)
            {
                sb.AppendLine(p);
            }
            return sb.ToString();
        }

        private static bool HandlePartialEquals(string s1, string s2)
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
                comp = s1.ToLower().Contains(s2.ToLower());
            }

            return comp;
        }
    }
}
