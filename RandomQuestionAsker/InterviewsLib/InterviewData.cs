using InterviewsLib;
using System.Collections.Generic;

namespace InterviewsLib
{
    public class InterviewData
    {
        public InterviewData(List<Question> questions)
        {
            Questions = questions;
        }
        public InterviewData() { }
        public List<Question> Questions { get; set; }
    }
}