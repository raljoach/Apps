using Common;
using InterviewsLib;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
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
