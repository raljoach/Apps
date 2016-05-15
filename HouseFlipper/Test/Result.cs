using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Result
    {
        public string Output;
        private string Error;

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
            string s1 = this.Output, s2 = other.Output;
            bool comp = Compare(s1, s2);

            return comp;
            //return  && this.Error.ToLower().Equals(other.Error.ToLower());
        }

        private static bool Compare(string s1, string s2)
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
