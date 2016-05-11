using Common;
using InterviewsLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RandomQuestionAsker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var topics = new List<string>();
            var companies = new List<string>();
            var keywords = new List<string>();
            var tags = new List<string>();
            if(args!=null && args.Length>0)
            {
                for(int j=0; j<args.Length; j++)
                {
                    var flag = args[j].ToLower();
                    if(flag.StartsWith("-"))
                    {
                        flag = flag.Substring(1);
                    }
                    switch(flag)
                    {
                        case "topic":
                            ++j;
                            GetParamValues(args, ref j, topics);
                            break;
                        case "company":
                            ++j;
                            GetParamValues(args, ref j, companies);
                            break;
                        case "keyword":
                            ++j;
                            GetParamValues(args, ref j, keywords);
                            break;
                        case "tag":
                            ++j;
                            GetParamValues(args, ref j, tags);
                            break;
                        default:
                            throw new InvalidOperationException(string.Format("Error: Unhandled parameter flag '{0}", flag));
                    }
                }
            }
            var file = "questions.json";
            var index = BuildIndexes(file);
            AskQuestions(index, topics, companies, keywords, tags);
        }

        private static void GetParamValues(string[] args, ref int j, List<string> list)
        {
            for (; j < args.Length; j++)
            {
                var nextArg = args[j];
                if (nextArg.StartsWith("-")) { --j; break; }
                list.Add(nextArg);
            }
        }

        public enum IndexType
        {
            Topic,
            Company,
            Tag,
            Keyword
        }

        private static void AskQuestions(Index index, List<string> topics, List<string> companies, List<string> keywords, List<string> tags)
        {
            int numIndexes = 0;
            var indexMap = new Dictionary<IndexType, Dictionary<string, List<Question>>>();
            bool noRestrictions = topics.Count==0 && companies.Count==0 && keywords.Count==0 && tags.Count==0;

            if (index.TopicIndex.Count > 0)
            {
                if (noRestrictions || topics.Count > 0)
                {
                    ++numIndexes;
                    indexMap.Add(IndexType.Topic,index.TopicIndex);
                }
            }
            if (index.KeywordIndex.Count > 0)
            {
                if (noRestrictions || keywords.Count > 0)
                {
                    ++numIndexes;
                    indexMap.Add(IndexType.Keyword, index.KeywordIndex);
                }
            }
            if (index.CompanyIndex.Count > 0)
            {
                if (noRestrictions || companies.Count > 0)
                {
                    ++numIndexes;
                    indexMap.Add(IndexType.Company, index.CompanyIndex);
                }
            }
            if (index.TagIndex.Count > 0)
            {
                if (noRestrictions || tags.Count > 0)
                {
                    ++numIndexes;
                    indexMap.Add(IndexType.Tag, index.TagIndex);
                }
            }
            int minsToAnswer = 5;
            var minsToAnswerTS = TimeSpan.FromMinutes(minsToAnswer);
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            var random = new Random(seed);
            Logger.Debug("Random seed: {0}", seed);
            if (numIndexes > 0)
            {
                var choice = random.Next(numIndexes);
                var iType = indexMap.Keys.ToArray()[choice];
                var cIndex = indexMap[iType];

                Question q;
                if (noRestrictions)
                {
                    var numK = cIndex.Keys.Count;
                    choice = random.Next(numK);
                    var k = cIndex.Keys.ToArray()[choice];

                    var qList = cIndex[k];
                    var numQ = qList.Count;
                    choice = random.Next(numQ);
                    q = qList[choice];
                }
                else
                {
                    switch(iType)
                    {
                        case IndexType.Company:
                            break;
                        default:
                            throw new InvalidOperationException(string.Format("Error: Unhandled index type: {0}", iType));
                    }
                }

                while (true)
                {
                    Logger.Debug("Ready for a question? Hit enter to continue or q to quit.");
                    var key = Console.ReadKey().KeyChar;
                    if (key == 'q' || key == 'Q') { break; }
                    Logger.Border();
                    Logger.Debug("Here's your question:");
                    Logger.Border();
                    Logger.Debug(q.Text);
                    Logger.Border();

                    var timeout = DateTime.Now.Add(minsToAnswerTS);
                    var halfSecCount = 0;
                    while (DateTime.Now <= timeout)
                    {
                        Thread.Sleep(TimeSpan.FromMilliseconds(500));
                        ++halfSecCount;
                        if (halfSecCount % 2 == 0)
                        {
                            minsToAnswerTS.Subtract(TimeSpan.FromSeconds(1));
                        }
                        Logger.Debug("Time remaining: {0}:{1}", minsToAnswerTS.Minutes, minsToAnswerTS.Seconds);
                    }

                    Logger.Debug("Times up!");
                    //Buzzer sounds!   
                }
            }
        }

        private static Index BuildIndexes(string file)
        {
            var indexFile = "index.json";
            Index index;
            if (File.Exists(indexFile))
            {
                index = JsonConvert.DeserializeObject<Index>(FileIO.ReadAll(indexFile));
            }
            else
            {
                var interviewData = JsonConvert.DeserializeObject<InterviewData>(FileIO.ReadAll(file));
                var keywordIndex = new Dictionary<string, List<Question>>(StringComparer.CurrentCulture);
                var companyIndex = new Dictionary<string, List<Question>>(StringComparer.CurrentCulture);
                var topicIndex = new Dictionary<string, List<Question>>(StringComparer.CurrentCulture);
                foreach (var q in interviewData.Questions)
                {
                    AddToIndex(keywordIndex, q.Keywords, q);
                    AddToIndex(companyIndex, q.Company, q);
                    AddToIndex(topicIndex, q.Topic, q);
                }
                index = new Index();
                index.TopicIndex = topicIndex;
                index.CompanyIndex = companyIndex;
                index.KeywordIndex = keywordIndex;
                var json = JsonConvert.SerializeObject(index);

                FileIO.Write(indexFile, json);
            }
            return index;
        }

        private static void AddToIndex(Dictionary<string, List<Question>> index, string key, Question q)
        {
            if (key == null) return;
            List<Question> list;
            if(index.ContainsKey(key))
            {
                list = index[key];
            }
            else
            {
                list = new List<Question>();
                index.Add(key, list);
            }
            list.Add(q);            
        }

        private static void AddToIndex(Dictionary<string, List<Question>> index, List<string> keys, Question q)
        {
            foreach(var k in keys)
            {
                AddToIndex(index, k, q);
            }
        }
    }
}
