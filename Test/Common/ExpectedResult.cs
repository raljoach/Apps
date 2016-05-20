using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Common
{
    public class ExpectedResult
    {
        public Match OutputMatch;
        public Match ErrorMatch = new FullMatch("");

        public ExpectedResult(Match outputMatch)
        {
            this.OutputMatch = outputMatch;
        }

        public ExpectedResult(Match outputMatch, Match errorMatch) : this(outputMatch)
        {
            this.ErrorMatch = errorMatch;
        }

        public override bool Equals(object obj)
        {
            var er = obj as ExpectedResult;
            if (er != null) { return base.Equals(er); }
            var r = obj as Result;
            if(r==null) { return false; }
            return HandleEquals(r);
        }

        internal static ExpectedResult Create(params TableField[] fields)
        {
            throw new NotImplementedException();
        }

        private bool HandleEquals(Result other)
        {
            if (other == null) { return false; }
            return HandleEquals(this.OutputMatch, other.Output) && HandleEquals(this.ErrorMatch, other.Error);
        }

        private bool HandleEquals(Match match, string output)
        {
            if (match == null && output == null) return true;
            if (match == null || output == null) return false;
            return match.Matches(output);
        }

        /*private static bool HandleEquals(string s1, string s2)
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
        }*/

    }
}
