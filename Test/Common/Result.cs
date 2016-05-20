using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Test.Common;

namespace Test.Common
{
    public class Result
    {
        public string Output;
        public string Error;

        public Result(string output)
        {
            this.Output = output;
        }

        public Result(string output, string error) : this(output)
        {
            this.Error = error;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Result;
            if (other == null) { return false; }
            return HandleEquals(this.Output, other.Output) && HandleEquals(this.Error, other.Error);
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
