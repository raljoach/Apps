using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewsLib
{
    public class Index
    {
        public Dictionary<string, List<Question>> CompanyIndex { get; set; }
        public Dictionary<string, List<Question>> KeywordIndex { get; set; }
        public Dictionary<string, List<Question>> TopicIndex { get; set; }
        public Dictionary<string, List<Question>> TagIndex { get; set; }
    }
}
