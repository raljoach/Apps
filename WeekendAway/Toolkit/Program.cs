using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Remote;
using Google.Maps.Geocoding;
using Common;
using Newtonsoft.Json;

namespace Toolkit
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
            List<Place> places = Search();
            places.AddRange(FindPlaces(places.Count));
            Debug("Total: {0}", places.Count);
            var visit = new Visit(places);
            var json = JsonConvert.SerializeObject(visit);
            OutputToFile("output.json", json);
            Console.WriteLine("Program has ended. Hit any key to exit.");
            Console.ReadKey();
        }

        private static void OutputToFile(string file, string content)
        {
            using (var sw = new StreamWriter(new FileStream(file, FileMode.Create, FileAccess.ReadWrite)))
            {
                sw.Write(content);
                sw.Flush();
            }
        }

        private static List<Place> Search()
        {
            var places = new List<Place>();
            var file = "start.txt";
            var driver = new ChromeDriver(); //InitPhantomJS();
            try
            {
                var urlCount = 0;
                var placeCount = 0;
                foreach (var url in ReadFrom(file))
                {
                    ++urlCount;
                    try
                    {
                        Logger.Border();
                        Debug("{0}) Site with Suggestions Url: {1}", urlCount, url);
                        Logger.Border();
                        Debug("Visit site with suggestions '{0}' ...", url);
                        driver.Navigate().GoToUrl(url);
                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                        wait.Until((d) => { return d.Title.ToLower().EndsWith("search"); });
                        Debug("Page title is: " + driver.Title);
                        Debug("Site loaded");
                        Debug();

                        Debug("Getting suggestions....");
                        foreach (var suggestion in GetSuggestions(driver,url,urlCount))
                        {
                            ++placeCount;
                            Place p = Lookup(driver, suggestion, placeCount);
                            places.Add(p);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug();
                        Debug("Error: Unable to process url #{0}\r\nUrl:{1}\r\n{2}", urlCount, url, ex.ToString());
                        Debug();
                        //Debug("Hit Enter to continue...");
                        //Console.ReadKey();
                        //Debug();
                    }
                    finally
                    {
                        Logger.Border();
                    }
                }
            }
            finally
            {
                driver.Quit();
            }
            return places;
        }

        private static List<Place> FindPlaces(int count=0)
        {
            var places = new List<Place>();
            var file = "urls.txt";
            var driver = new ChromeDriver(); //InitPhantomJS();
            try
            {
                //var count = 0;
                foreach (var url in ReadFrom(file))
                {
                    ++count;
                    try
                    {
                        Place p = GetPlace(driver, url, count);
                        places.Add(p);
                    }
                    catch (Exception ex)
                    {
                        Debug();
                        Debug("Error: Unable to process url #{0}\r\nUrl:{1}\r\n{2}", count, url, ex.ToString());
                        Debug();
                        //Debug("Hit Enter to continue...");
                        //Console.ReadKey();
                        //Debug();
                    }
                    finally
                    {
                        Logger.Border();
                    }
                }
            }
            finally
            {
                driver.Quit();
            }
            return places;
        }

        private static Place Lookup(IWebDriver driver, Suggestion suggestion,int placeId)
        {
            var p = suggestion.Place;
            try
            { 
                Lookup(driver, p, p.Url, placeId);
            }
            catch (Exception ex)
            {
                Debug();
                Debug("Error: Unable to process url #{0}\r\nUrl:{1}\r\n{2}", placeId, p.Url, ex.ToString());
                Debug();
                //Debug("Hit Enter to continue...");
                //Console.ReadKey();
                //Debug();
            }
            return p;
        }

        private static IEnumerable<Suggestion> GetSuggestions(ChromeDriver driver, string searchUrl, int searchUrlId)
        {
            var suggestions = new List<Suggestion>();
            var p = new Place();
            var count = 0;
            //while (true)
            //{
                foreach (var section in driver.FindElements(By.ClassName("_Uhb")))
                {
                    var links = section.FindElements(By.TagName("a"));
                    foreach (var a in links)
                    {
                        ++count;
                        var url = a.GetAttribute("href");
                        var nameElem = a.FindElement(By.ClassName("title"));
                        var name = nameElem.Text;

                        var descElem = a.FindElement(By.ClassName("_Ajf"));
                        var desc = descElem.Text;
                        Debug("Suggestion #{0}:\r\nName: {1}\r\nDescription: {2}\r\nUrl: {3}\r\n", count, name, desc, url);
                        var s = 
                         new Suggestion(new Place() { Id = count, Name = name, Url = url, Description = desc }) { SearchUrlId = searchUrlId, SearchUrl = searchUrl };
                        suggestions.Add(s);
                    }
                }
                //var search = driver.FindElements(By.TagName("g-right-button"));
                //if (search == null || search.Count == 0) { break; }
                //search[0].Click();
            //}
            
            return suggestions;
        }        

        private static Place GetPlace(ChromeDriver driver, string url, int count)
        {
            var p = new Place();
            Lookup(driver, p, url, count);
            return p;
        }

        private static void Lookup(IWebDriver driver, Place p, string url, int count)
        {
            Logger.Border();
            Debug("{0}) Url: {1}", count, url);
            Logger.Border();
            Debug("Visit site '{0}' ...", url);
            driver.Navigate().GoToUrl(url);
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until((d) => { return d.Title.ToLower().EndsWith("search"); });

            Debug("Page title is: " + driver.Title);

            Debug("Site loaded");
            Debug();
            DebugWrite("Finding name of location....");
            var name = GetName(driver);
            Debug(name);
            DebugWrite("Finding Address of location....");
            var address = GetAddress(driver);
            Debug(address);
            Debug("Finding Hours of location....");
            var hours = GetHours(driver);
            Debug(hours);
            GeoLocation loc = new GoogleMapToolkitApi().GetLatLong(address);

            p.Id = count;
            p.Name = name;
            p.Address = address;
            p.Hours = hours;
            p.GeoLocation = loc;
        }        

        private static Dictionary<string, string> GetHours(IWebDriver driver)
        {
            var dict = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            //var hoursSection = driver.FindElement(By.CssSelector("div[data-dtype='d3oh']"));
            //var hoursTable = hoursSection.FindElement(By.TagName("table"));//(By.ClassName("ts loht__hours-table"));
            /*
            foreach (var hoursTable in hoursSection.FindElements(By.TagName("table")))
            {
                foreach (var row in hoursTable.FindElements(By.TagName("tr")))
                {
                    var cells = row.FindElements(By.TagName("td"));
                    var day = cells[0].Text;
                    var hours = cells[1].Text;
                    Debug("'{0}' - '{1}'", day, hours);
                    //dict.Add(day, hours);
                }
            }
            */
            //Debug("2nd try: look table rows");
            foreach (var t in driver.FindElements(By.ClassName("loht__day-label")))
            {
                //Debug("Day: {0}", t.Text);
                var remoteWebDriver = (RemoteWebElement)t;
                var javaScriptExecutor = (IJavaScriptExecutor)remoteWebDriver.WrappedDriver;
                var innerHtml = javaScriptExecutor.ExecuteScript("return arguments[0].innerHTML;", t).ToString();
                //Debug("{0}\r\n", innerHtml);
                var day = innerHtml;

                var id = @"//following-sibling::td/span[contains(@class,'loht__open-interval')]";
                var y = driver.FindElement(By.XPath(id));
                //Debug("Hours: {0}", y.Text);
                remoteWebDriver = (RemoteWebElement)y;
                javaScriptExecutor = (IJavaScriptExecutor)remoteWebDriver.WrappedDriver;
                innerHtml = javaScriptExecutor.ExecuteScript("return arguments[0].innerHTML;", y).ToString();
                //Debug("{0}\r\n", innerHtml);
                var hours = innerHtml;
                dict.Add(day, hours);
            }

            /*
            foreach (var t in driver.FindElements(By.ClassName("loht__open-interval")))
            {
                Debug("Hours: {0}", t.Text);

                var remoteWebDriver = (RemoteWebElement)t;
                var javaScriptExecutor = (IJavaScriptExecutor)remoteWebDriver.WrappedDriver;
                var innerHtml = javaScriptExecutor.ExecuteScript("return arguments[0].innerHTML;", t).ToString();
                Debug("{0}\r\n", innerHtml);

            }
            */
            return dict;
        }

        private static string GetAddress(IWebDriver driver)
        {
            var addressSection = driver.FindElement(By.CssSelector("div[data-dtype='d3adr']"));
            var addressElem = addressSection.FindElement(By.CssSelector("span[class='_Xbe']"));
            return addressElem.Text;
        }

        private static string GetName(IWebDriver driver)
        {
            var nameElem = driver.FindElement(By.CssSelector("div[data-dtype='d3bn']"));
            return nameElem.Text;
        }
    }
}
