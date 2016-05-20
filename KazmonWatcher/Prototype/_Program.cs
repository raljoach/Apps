using Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KazmonWatcher.Prototype
{                   
    public static class _Program
    {
        private static string kazmonUrl = "https://kazmon.docusign.net:17005/Dashboard/";

        public static CounterInstance Total { get; private set; }

        static _Program()
        {
            var file = "config.json";
            var json = FileIO.ReadAll(file);
            var config = JsonConvert.DeserializeObject<Config>(json);
            var driver = new ChromeDriver();//InitFirefoxDriver();/PhantomJSExt.InitPhantomJS();
            //var driver = new RemoteWebDriver(new Uri(kazmonUrl), DesiredCapabilities.Chrome());
            try
            {
                driver.Navigate().GoToUrl(kazmonUrl);
                EnterCreds(driver, config);
                Console.WriteLine("Please enter google authenticator code into browser. Then hit enter in console window.");//id='softTokenAttempt.passcode'
                Console.ReadKey();
                Console.WriteLine("OK. Continuing...");
                var autoRef = driver.FindElement(By.Id("metricAutoRefresh"));
                if (autoRef.Selected)
                {
                    autoRef.Click();
                }
                //Prototype(config, driver);

                RealCode(config, driver);

            }
            finally
            {
                driver.Quit();
            }
            Console.WriteLine("Program has ended. Hit any key to exit.");
            Console.ReadKey();
        }

        private static IWebDriver InitFirefoxDriver()
        {
            var driver = new FirefoxDriver(new FirefoxBinary(@"C:\Program Files (x86)\Mozilla Firefox\Firefox.exe"), new FirefoxProfile(), TimeSpan.FromMinutes(10));
            return driver;
        }

        private static void RealCode(Config config, IWebDriver driver)
        {
            // TODO: Set the time to 1 day
            var periodBox = driver.FindElement(By.Id("metricPeriod"));
            periodBox.Clear();
            periodBox.SendKeys("1.0:00");

            // TODO: Set reference point to Now
            SetEnvironments(config, driver);
            var counters = SetCounters(config, driver);
            Thread.Sleep(TimeSpan.FromSeconds(15));
            // Find instances that are anomalies
            // 1: avg, std 
            //     => avg should be the one on the Total line
            //     => std should use each of the instances
            ExpandTables(driver);
            foreach (var name in counters)
            {                
                var counter = FindCounter(driver, name);
                var total = FindTotal(counter);
                var instances = FindInstances(counter);
                ZTest(instances);
            }

            // Find environments with anomalies
        }

        private static void Refresh(IWebDriver driver)
        {
            //Thread.Sleep(TimeSpan.FromSeconds(5));
            var refBtn = driver.FindElement(By.Id("metricRefresh"));
            refBtn.Click();
            //Thread.Sleep(TimeSpan.FromSeconds(15));
        }

        private static void ZTest(List<CounterInstance> instances)
        {
            var result =  StandardDeviation(instances);
            var count = result.Count;
            var avg = result.Average;
            if (result.SumOfSquares.HasValue)
            {
                var variance = (double)result.SumOfSquares / (count - 1);
                var std = Math.Sqrt(variance);

                // Calculate z-score of each instance
                Logger.Debug("Sum: {0}", result.Sum);
                Logger.Debug("Count: {0}", result.Count);
                Logger.Border();
                Console.WriteLine("Average: {0}", result.Average);
                Console.WriteLine("Standard deviation: {0}", result.StandardDeviation);
                Logger.Border();
                var tmp = result.StandardDeviation * 2;
                Logger.Debug("Z: +2 => {0}", tmp + result.Average);
                Logger.Debug("Z: -2 => {0}", tmp - result.Average);
                Logger.Border();
                Logger.Debug("1.5*Avg: {0}", result.Average * 1.5);
                Logger.Border();
                Logger.Debug("Z values:");
                var above = new List<Tuple<int, double>>();
                for (var j=0; j<instances.Count; j++)
                {
                    var inst = instances[j];
                    var z = ZScore(inst, result);
                    Logger.Debug("[{0}] {1} => Z:{2}", j + 1, inst.Average, z);
                    if (z >= 2)
                    {
                        above.Add(new Tuple<int, double>(j, z));
                    }
                }
                Logger.Border();
                Logger.Debug("Values above Z=2:");
                foreach (var t in above)
                {                    
                    //var z = ZScore(ca, result);
                    var j = t.Item1;
                    var z = t.Item2;
                    var ca = instances[j];
                    Logger.Debug("[{0}] {1} => Z:{2}", j + 1, ca.Average, z);
                }
            }
        }

        private static Result StandardDeviation(List<CounterInstance> instances)
        {
            var result = SumOfSquares(instances);
            var count = result.Count;
            var avg = result.Average;
            result.StandardDeviation = -1;
            if (result.SumOfSquares.HasValue)
            {
                var variance = (double)result.SumOfSquares / (count - 1);
                var std = Math.Sqrt(variance);
                result.StandardDeviation = std;
            }
            return result;
        }

        private static double ZScore(CounterInstance ca, Result result)
        {
            var diff = ca.Average - result.Average;
            return diff / result.StandardDeviation;
        }

        private static Result SumOfSquares(List<CounterInstance> instances)
        {
            double sum = 0.0;
            double sqSum = 0.0;
            double count = 0.0;
            foreach (var ca in instances)
            {
                sum += ca.Average * ca.Count;
                count += ca.Count;
                sqSum += Math.Pow(ca.Average, 2) * ca.Count;
            }
            double avg = sum / count;
            var val = SumOfSquares(avg, count, sum, sqSum);
            return new Result(sum, sqSum, avg, count, val);
        }

        private static double? SumOfSquares(double average, double count, double sum, double sqSum)
        {
            if (count > 0)
            {
                return Math.Max(sqSum - average * sum, 0);
            }
            return null;
        }

        private static CounterInstance FindTotal(Counter counter)
        {
            return counter.Total;
        }

        private static List<CounterInstance> FindInstances(Counter counter)
        {
            return counter.Instances;
        }

        private static void ExpandTables(IWebDriver driver)
        {
            //var counterSearch = counter.ToLower();
            var section = driver.FindElement(By.Id("metricCharts"));
            var pause = section.FindElements(By.XPath("//img[@src='Content/images/loader.gif']"));
            var loadCount = 0;
            foreach(var p in pause)
            {
                if(p.Displayed)
                {
                    ++loadCount;
                }
            }
            while (loadCount > 0)
            {
                Refresh(driver);
                Thread.Sleep(TimeSpan.FromSeconds(15));
                pause = section.FindElements(By.XPath("//img[@src='Content/images/loader.gif']"));
                loadCount = 0;
                foreach (var p in pause)
                {
                    if (p.Displayed)
                    {
                        ++loadCount;
                    }
                }
            }

            var dataTables = driver.FindElements(By.XPath("//table[@class='data-table']"));
            foreach(var dt in dataTables)
            {
                var btn = dt.FindElement(By.TagName("button"));
                btn.Click();
            }
        }

        private static Counter FindCounter(IWebDriver driver, string counter)
        {
            var counterSearch = counter.ToLower();
            var section = driver.FindElement(By.Id("metricCharts"));/*
            var loading = section.FindElements(By.XPath("//img[@src='Content/images/loader.gif']"));
            if(loading!=null && loading.Count>0)
            {
                Refresh(driver);
                Thread.Sleep(TimeSpan.FromSeconds(15));
                loading = section.FindElements(By.XPath("//img[@src='Content/images/loader.gif']"));
            }*/
            var tds = section.FindElements(By.ClassName("data-table-caption"));
            var list = new List<CounterInstance>();
            CounterInstance total = null;
            var found = false;
            foreach(var td in tds)
            {
                var spans = td.FindElements(By.TagName("span"));
                foreach(var span in spans)
                {
                    var text = span.Text.ToLower();
                    if(text.StartsWith(counterSearch))
                    {
                        Logger.Border();
                        Logger.Debug("Counter {0} found", counter);
                        Logger.Border();
                        //var table = span.FindElement(By.XPath("//parent::tbody"));                        
                        //var dataTable = table.FindElement(By.XPath("//tr/td/table[@class='data-table']"));

                        //var parentTR = span.FindElement(By.XPath("//parent::tr"));
                        //var dataTables = parentTR.FindElements(By.XPath("//td/table[@class='data-table']"));

                        
                        var parent = span.FindElement(By.XPath(".."));
                        while(parent.TagName!="tbody")
                        {
                            parent = parent.FindElement(By.XPath(".."));
                        }

                        //var nexTR = parent.FindElement(By.XPath("//following-sibling::tr"));
                        var others = parent.FindElements(By.XPath("./tr"));
                        if(others.Count!=2)
                        {
                            throw new InvalidOperationException();
                        }
                        var nextTR = others[1];
                        var container = nextTR.FindElement(By.XPath("./td[@class='container']"));
                        var dataTable = container.FindElement(By.XPath("./table[@class='data-table']"));
                        /*
                        if (dataTables != null)
                        {
                            if (dataTables.Count == 1)
                            {
                                var dataTable = dataTables[0];*/
                        //var btn = dataTable.FindElement(By.TagName("button"));
                        //        btn.Click();
                        //dataTable = table.FindElement(By.XPath("//tr/td/table[@class='data-table']"));
                                var rows = dataTable.FindElements(By.TagName("tr"));
                                //var rows = dataTable.FindElements(By.ClassName("data-row"));
                                //var rows = dataTable.FindElements(By.XPath("//tbody/tr[@class='data-row']"));
                                for (int j = 1; j < rows.Count; j++)
                                {
                                    var row = rows[j];
                                    var cells = row.FindElements(By.TagName("td"));
                                    var count = double.Parse(cells[2].Text);
                                    var average = double.Parse(cells[4].Text);
                                    var inst = new CounterInstance(count, average);
                                    if (j < rows.Count - 1)
                                    {
                                        list.Add(inst);
                                    }
                                    else
                                    {
                                        total = inst;
                                    }
                                }

                        /*}
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("Error: Can't access data grid for counter {0}", counter));
                    }*/
                        found = true;
                        break;
                    }
                }
                if(found)
                {
                    break;
                }
            }
            if(!found)
            {
                throw new InvalidOperationException(string.Format("Error: Unable to find counter '{0}'", counter));
            }
            return new Counter() { Instances = list, Total = total };
        }

        private static void Prototype(Config config, IWebDriver driver)
        {
            SetEnvironments(config, driver);
            SetCounters(config, driver);
        }

        private static void EnterCreds(IWebDriver driver, Config config)
        {
            var userId = "cred_userid_inputtext";
            var passId = "cred_password_inputtext";
            var userBox = driver.FindElement(By.Id(userId));
            userBox.SendKeys(config.Creds.UserName);


            var passBox = driver.FindElement(By.Id(passId));
            passBox.Click();//passBox.SendKeys(config.Creds.Password);
            /*
            var btn = driver.FindElement(By.Id("cred_sign_in_button"));
            btn.Click();*/
        }

        private static List<string> SetCounters(Config config, IWebDriver driver)
        {
            Console.WriteLine("Setting counters:");
            var counters = config.Counters;
            var section = driver.FindElement(By.Id("metricCounters"));
            SetCheckBoxes(config, driver, section, counters);
            return counters;
        }

        private static void SetEnvironments(Config config, IWebDriver driver)
        {
            Console.WriteLine("Setting environments:");
            var section = driver.FindElement(By.Id("metricEnvironments"));
            SetCheckBoxes(config, driver, section, config.Environments);
        }

        private static void SetCheckBoxes(Config config, IWebDriver driver, IWebElement section, List<string> selections)
        {
            var matchHash = new HashSet<string>();
            var choices = section.FindElements(By.TagName("label"));
            var count = choices.Count;
            if (count <= 10)
            {
                foreach (var c in choices)
                {
                    var envOp = c.Text;
                    if (!c.Displayed) { continue; }
                    if (selections.Contains(envOp, StringComparer.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Found: {0}", envOp);
                        if (!c.Selected)
                        {
                            c.Click();
                        }
                        matchHash.Add(envOp);
                        if (selections.Count == matchHash.Count)
                        {
                            Console.WriteLine("All selections found!");
                            break;
                        }
                    }
                }

                if (selections.Count != matchHash.Count)
                {
                    Console.WriteLine("One or more options were not found:");
                    foreach (var s in selections)
                    {
                        Console.WriteLine(s);
                    }
                    throw new InvalidOperationException("Error: See above for details!");
                }
            }
            else
            {
                foreach (var t in selections)
                {
                    var sel = t.ToLower();
                    
                    int low = 0, high = count - 1;
                    while (low <= high)
                    {
                        var mid = (low + high) / 2;
                        var c = choices[mid];
                        var m = c.Text.ToLower();
                        var comp = sel.CompareTo(m);
                        if (comp == 0)
                        { // match
                            Console.WriteLine("Found: {0}", t);
                            if (!c.Selected)
                            {
                                c.Click();                                
                            }
                            matchHash.Add(t);
                            break;
                        }
                        else if (comp < 0)
                        {
                            high = mid - 1;
                        }
                        else
                        {
                            low = mid + 1;
                        }
                    }
                }

                if (selections.Count == matchHash.Count)
                {
                    Console.WriteLine("All selections found!");
                }
                else
                {
                    Console.WriteLine("One or more options were not found:");
                    foreach (var s in selections)
                    {
                        if (matchHash.Contains(s)) { continue; }
                        Console.WriteLine(s);
                    }
                    throw new InvalidOperationException("Error: See above for details!");
                }
            }
        }

        private static void SetCheckBoxesOld(Config config, IWebDriver driver, IWebElement section, List<string> selections)
        {
            var matchHash = new HashSet<string>();
            var choices = section.FindElements(By.TagName("label"));

            foreach (var c in choices)
            {
                var envOp = c.Text;
                if (!c.Displayed) { continue; }
                if (selections.Contains(envOp, StringComparer.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Found: {0}", envOp);
                    if (!c.Selected)
                    {
                        c.Click();
                    }
                    matchHash.Add(envOp);
                    if(selections.Count== matchHash.Count)
                    {
                        Console.WriteLine("All selections found!");
                        break;
                    }
                }
            }

            if(selections.Count != matchHash.Count)
            {
                Console.WriteLine("One or more options were not found:");
                foreach(var s in selections)
                {
                    if (matchHash.Contains(s)) { continue; }
                    Console.WriteLine(s);
                }
                throw new InvalidOperationException("Error: See above for details!");
            }
        }

        private static void SetCheckBoxes(Config config, IWebDriver driver, IWebElement section, List<string> selections, string filterId)
        {            
            var list = section.FindElements(By.TagName("li"));
            foreach (var sel in selections)
            {
                var search = sel.ToLower();
                if (!string.IsNullOrWhiteSpace(filterId))
                {
                    var tmp = search;
                    var filterBox = driver.FindElement(By.Id(filterId));
                    filterBox.Clear();
                    if(tmp.StartsWith("["))
                    {
                        tmp = tmp.Substring(1);
                    }
                    var index = tmp.IndexOf(']');
                    var filterText = tmp.Substring(0, index);
                    filterBox.SendKeys(filterText);
                    //list = section.FindElements(By.TagName("li"));
                    //list = section.FindElements(By.CssSelector("//li[@style='display: block; visibility: visible;']"));
                    //list = section.FindElements(By.CssSelector("//li[not(contains(@style,'display:none'))]"));
                }

                bool found = false;
                foreach (var l in list)
                {
                    if(!l.Displayed || l.GetAttribute("style").Contains("hidden")) { continue; }
                    var c = l.FindElement(By.TagName("label"));
                    var envOp = c.Text.ToLower();
                    if (envOp.Equals(search))
                    {
                        found = true;
                        Console.WriteLine("Found: {0}", sel);
                        if (!c.Selected)
                        {
                            c.Click();
                        }
                        break;
                    }
                }
                if(!found)
                {
                    throw new InvalidOperationException(string.Format("Error: Unable to find {0} in list of checkboxes", sel));
                }

            }
        }
    }
}
