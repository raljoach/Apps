using Common;
using InterviewsLib;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit
{
    class Program
    {
        static void Main(string[] args)
        {
            //ProcessCareerCupQuestions();
            var outFile = "programcreek.json";
            var questions = new List<Question>();
            var driver = PhantomJSExt.InitPhantomJS();
            var site = "http://www.programcreek.com/2012/11/top-10-algorithms-for-coding-interview/";

            try
            {
                GetQuestions(questions, driver, site);
                foreach (var q in questions)
                {
                    driver.Navigate().GoToUrl(q.Url);
                    var section = driver.FindElement(By.ClassName("entrybody"));
                    var p = section.FindElement(By.XPath("./p"));
                    var qText = p.Text;
                    q.Text = qText;
                }
            }
            finally
            {
                driver.Quit();
                InterviewData data = new InterviewData();
                data.Questions = questions;
                var json = JsonConvert.SerializeObject(data);
                FileIO.Write(outFile, json);
            }
        }

        private static void GetQuestions(List<Question> questions, IWebDriver driver, string site)
        {
            driver.Navigate().GoToUrl(site);
            var section = driver.FindElement(By.ClassName("entrybody"));
            var paragraphs = section.FindElements(By.TagName("p"));
            string topic = null;
            var sectionCount = 0;
            bool newTopic = false;
            for (var j = 0; j < paragraphs.Count; j++)
            {
                var p = paragraphs[j];
                var aTags = p.FindElements(By.XPath("./a"));

                var strongTags = p.FindElements(By.XPath("./strong"));
                if (newTopic && sectionCount == 3)
                {
                    //if (newTopic)
                    //{
                    if (p.Text.ToLower().StartsWith("classic problem"))
                    {
                        if (aTags != null && aTags.Count > 0)
                        {
                            newTopic = false;
                            ProcessTags(aTags, questions, topic, p);
                        }
                        /*if (strongTags != null && strongTags.Count > 0)
                        {
                            if (strongTags.Count == 1)
                            {
                                var s = strongTags[0];
                                var title = s.Text;
                                var index = title.IndexOf(' ');
                                topic = title.Substring(index + 1).Trim();
                                ++sectionCount;
                                newTopic = true;
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }*/
                        //}
                    }

                }
                else if (strongTags != null && strongTags.Count > 0)
                {
                    if (strongTags.Count == 1)
                    {
                        if (newTopic) { throw new InvalidOperationException(); }
                        var s = strongTags[0];
                        var title = s.Text;
                        var index = title.IndexOf(' ');
                        topic = title.Substring(index + 1).Trim();
                        ++sectionCount;
                        newTopic = true;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else if (sectionCount < 4 || sectionCount == 8 || (sectionCount >= 10 && sectionCount <= 12))
                {
                    if (newTopic)
                    {
                        var t = p.Text.ToLower();
                        if (t.StartsWith("classic problem") ||
                            t.StartsWith("class problem") ||
                            t.StartsWith("additional problems"))
                        {
                            newTopic = false;
                            ProcessTags(aTags, questions, topic, p);

                        }
                    }
                }
                else if (newTopic)
                {
                    //++j;
                    //var nextP = paragraphs[j];
                    /*var prefix = string.Empty;
                    if (!string.IsNullOrWhiteSpace(p.Text))
                    {
                        prefix = p.Text;
                    }*/
                    while (aTags == null || aTags.Count == 0)
                    {
                        ++j;
                        p = paragraphs[j];
                        aTags = p.FindElements(By.XPath("./a"));
                    }

                    newTopic = false;
                    ProcessTags(aTags, questions, topic, p/*, prefix*/);

                    if (sectionCount == 13)
                    {
                        var t = p.Text.ToLower();
                        while (!t.StartsWith("additional problems"))
                        {
                            ++j;
                            p = paragraphs[j];
                            t = p.Text.ToLower();
                        }
                        aTags = p.FindElements(By.XPath("./a"));
                        newTopic = false;
                        ProcessTags(aTags, questions, topic, p);
                    }
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

        private static void ProcessTags(ReadOnlyCollection<IWebElement> aTags, List<Question> questions, string topic, IWebElement p, string prefix = "")
        {
            if (aTags == null || aTags.Count == 0)
            {
                throw new InvalidOperationException();
            }
            for (var k = 0; k < aTags.Count; k++)
            {
                var a = aTags[k];
                var url = a.GetAttribute("href");
                var name = a.Text.Trim();

                var pos = 0;
                for (; pos < name.Length; pos++)
                {
                    if (!Char.IsNumber(name[pos]))
                    {
                        if (name[pos] == ')')
                        {
                            pos++;
                        }
                        break;
                    }
                }
                if (pos > 0)
                {
                    name = name.Substring(pos + 1).Trim();
                }
                else if (!string.IsNullOrEmpty(prefix))
                {
                    //if (k == 0) {
                    name = prefix + name;
                    //}
                }
                var q = new Question(name, null, url, new List<string>() { topic });
                questions.Add(q);
            }
        }

        private static void ProcessCareerCupQuestions()
        {
            var dir = @"C:\Users\ralph.joachim\Documents\Visual Studio 2015\Projects\CareerCupToolkit\CareerCupToolkit";
            var questions = new List<Question>();
            var driver = PhantomJSExt.InitPhantomJS();
            try
            {
                foreach (var fName in Directory.EnumerateFiles(dir, "*.txt"))
                {
                    var fileName = Path.GetFileName(fName);
                    var index = fileName.IndexOf('_');
                    var company = fileName.Substring(0, index);

                    Question q = null;
                    var urlFound = true;
                    bool lookForBody = false;
                    StringBuilder textSB = new StringBuilder();
                    foreach (var line in FileIO.ReadFrom(fName))
                    {
                        if (line.StartsWith("-----")) { if (urlFound) { urlFound = false; lookForBody = true; } else if (lookForBody) { lookForBody = false; q.Text = textSB.ToString(); questions.Add(q); textSB = new StringBuilder(); } continue; }
                        if (line.StartsWith("Question"))
                        {
                            q = new Question();
                            q.Company = company;
                            var i = line.IndexOf(':');
                            var url = line.Substring(i + 1).Trim();
                            q.Url = url;
                            urlFound = true;

                            //tag, concept, topic
                            //You will have to visit the url to get the tags to populate topic

                            driver.Navigate().GoToUrl(url);
                            List<string> tags = GetTags(driver);
                            q.Tags = tags;
                        }
                        else if (lookForBody)
                        {
                            textSB.AppendLine(line);
                        }

                    }
                }
            }
            finally
            {
                driver.Quit();
                InterviewData data = new InterviewData();
                data.Questions = questions;
                var json = JsonConvert.SerializeObject(data);
                FileIO.Write("questions.json", json);
            }
        }

        private static List<string> GetTags(OpenQA.Selenium.PhantomJS.PhantomJSDriver driver)
        {
            var tElem = driver.FindElement(By.ClassName("tags"));
            var tags = new List<string>();
            foreach (var a in tElem.FindElements(By.TagName("a")))
            {
                tags.Add(a.GetAttribute("text"));
            }

            return tags;
        }
    }
}
