using System;

namespace Generator
{
    internal class NounVerbDetail
    {
        public string detail;
        public string noun;
        public string verb;

        public NounVerbDetail(string noun, string verb, string detail)
        {
            this.noun = Valid(noun);
            this.verb = Valid(verb);
            this.detail = Valid(detail);
        }

        private string Valid(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException();
            }
            return str;
        }
    }
}