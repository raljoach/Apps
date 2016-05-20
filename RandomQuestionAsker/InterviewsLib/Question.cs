using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InterviewsLib
{
    public class Question
    {
        public Question(string qText, string url, List<string> tags)
        {
            this.Text = qText;
            this.Url = url;
            this.Tags = tags;
        }

        public Question(string name, string qText, string url, List<string> tags) : this(qText,url,tags)
        {
            this.Name = name;
        }

        public Question() { }

        public string Company { get; set; }
        public List<string> Tags { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }
        public List<string> Keywords { get; set; }
        public string Topic { get; set; }
        public string Name { get; set; }
    }
}
