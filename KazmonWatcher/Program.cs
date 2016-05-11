using Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KazmonWatcher
{
    public class Config
    {
        public List<string> Environments { get; set; }
        public List<string> Counters { get; set; }

        public Creds Creds { get; set; }
    }
    public class Creds
    {
        public string Password { get; set; }
        public string UserName { get; set; }
    }
    public class Program
    {
        private static string kazmonUrl = "https://kazmon.docusign.net:17005/Dashboard/";
        public static void Main(string[] args)
        {
            var file = "config.json";
            var json = FileIO.ReadAll(file);
            var config = JsonConvert.DeserializeObject<Config>(json);
            var driver = new ChromeDriver();//PhantomJSExt.InitPhantomJS();
            //var driver = new RemoteWebDriver(new Uri(kazmonUrl), DesiredCapabilities.Chrome());
            try
            {
                driver.Navigate().GoToUrl(kazmonUrl);
                EnterCreds(driver, config);
                Console.WriteLine("Please enter google authenticator code into browser. Then hit enter in console window.");//id='softTokenAttempt.passcode'
                Console.ReadKey();

                SetEnvironments(config, driver);
                SetCounters(config, driver);
            }
            finally
            {
                driver.Quit();
            }
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

        private static void SetCounters(Config config, IWebDriver driver)
        {
            var section = driver.FindElement(By.Id("metricCounters"));
            SetCheckBoxes(config, driver, section, config.Counters);
        }

        private static void SetEnvironments(Config config, IWebDriver driver)
        {
            var section = driver.FindElement(By.Id("metricEnvironments"));
            SetCheckBoxes(config, driver, section, config.Environments);
        }

        private static void SetCheckBoxes(Config config, IWebDriver driver, IWebElement section, List<string> selections)
        {

            var choices = section.FindElements(By.XPath("//input[@type='checkbox']"));
            //var listItems = section.FindElements(By.TagName("li"));
            //foreach (var li in listItems)
            //{
                //var c = li.FindElement(By.XPath("//input[@type='checkbox']"));
                foreach (var c in choices)
                {
                //var envOp = c.Text;
                if (!c.Displayed) { continue; }
                var envOp = c.GetAttribute("value");
                    if (selections.Contains(envOp, StringComparer.CurrentCulture))
                    {
                        //c.aa
                        //setAttribute(driver, c, "checked", "true");
                        if (!c.Selected)
                        {
                            c.Click();
                        }
                    }
                //}
            }
        }

        //private static void setAttribute(RemoteWebDriver driver, IWebElement element, String attName, String attValue)
        //{
        //    driver.ExecuteScript("arguments[0].setAttribute(arguments[1], arguments[2]);",
        //            element, attName, attValue);
        //}
    }
}
