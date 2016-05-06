using Common;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeekendAway
{
    class Prototype
    {
        string country = "Hawaii, Oahu";
        string city = "Honolulu";
        List<string> locations = new List<string>()
        {
            "Hyatt Place Waikiki Beach hotel address",
            "Outrigger reef Waikiki Beach Resort address",
            "Aston Waikiki Beach Tower",
            "Hilton Hawaiin Village Waikiki Beach Resort & Spa",
            "Hyatt Regency Wakiki Beach Resort & Spa",
            "Sheraton Waikiki"
        };
        HashSet<Road> roadsHash = new HashSet<Road>();
        Graph<Point> graph;

        public Prototype()
        {
            //FindPlaces();
            var destinations = new List<Destination>();
            var points = new List<Point>();
            foreach (var location in locations)
            {
                var geoLoc = GetLatLong(location);
                var dest = destinations.Add(geoLoc);
                var roads = GetNearbyRoads(dest);
                foreach (var r in roads)
                {
                    graph.Add(r);
                }
            }
            graph.MinimumSpanningTree(3);
        }

        private  List<Place> FindPlaces()
        {
            var count = 0;
            var result = new List<Place>();
            foreach (var url in ReadFile("search.txt"))
            {
                //Selenium: Navigate to url
                IWebDriver driver = PhantomJSExt.InitPhantomJS();
                try
                {
                    Logger.Debug("Loading url '{0}'...", url);
                    driver.Navigate().GoToUrl(url);

                    string name, address, hours;
                    Logger.Debug("Looking for Name.....");
                    var nameElem = driver.FindElement(By.CssSelector("data-dtype=d3bn"));
                    Logger.Debug(name = nameElem.Text);
                    Logger.Debug("");

                    Logger.Debug("Looking for Address.....");
                    var addressElem = driver.FindElement(By.CssSelector("data-dtype=d3adr"));
                    var add2 = driver.FindElement(By.CssSelector(":contains('Address')"));
                    Logger.Debug(address = addressElem.Text);
                    Logger.Debug("");

                    Logger.Debug("Looking for Hours.....");
                    var hoursElem = driver.FindElement(By.CssSelector("data-dtype=d3adr"));
                    var h2 = driver.FindElement(By.CssSelector(":contains('Hours')"));
                    Logger.Debug(hours = hoursElem.Text);
                    Logger.Debug("");

                    var loc = new Place(++count,name, address, Convert(hours), null);
                    loc.GeoLocation = GetLatLong(address);

                    result.Add(loc);



                    /*
                                        Logger.Debug("Searching for topics...");
                                        IWebElement questionsLink = driver.FindElement(By.LinkText("Questions"));
                                        questionsLink.Click();

                                        var topicsCombo = driver.FindElement(By.Id("topic"));
                                        var options = topicsCombo.FindElements(By.TagName("option"));

                                        if (options == null || options.Count == 0)
                                        {
                                            throw new InvalidOperationException("Error: Unable to find list of topics!");
                                        }

                                        Logger.Debug("Successfully Retrieved topics");
                                        Console.WriteLine();
                                        foreach (var o in options)
                                        {
                                            careerCupTopics.Add(o.Text);
                                        }
                                        */
                }
                finally
                {
                    driver.Quit();
                }

                throw new NotImplementedException();
            }
            return result;
        }

        private static Dictionary<string,string> Convert(string hours)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<string> ReadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private Graph<Point> CreateGraph(List<Point> points)
        {
            throw new NotImplementedException();
        }

        private List<Road> GetNearbyRoads(Destination dest)
        {
            var roads = new List<Road>();
            foreach (Road newRoad in FindNearbyRoads(dest, 10))
            {
                //GeoLocation start=null, end=null;
                //var newRoad = new Road(start, end);

                var found = roadsHash.Where(road => road.Start == newRoad.Start && newRoad.End == road.End);
                if (found.Count() > 1)
                {
                    throw new InvalidOperationException(string.Format("Error: More than 1 road returned for Road(start={0},end={1})", newRoad.Start, newRoad.End));
                }

                if (found.Count() == 1) { var road = found.ElementAt(0); road.Destinations.Add(dest); roads.Add(road); }
                else
                {
                    roadsHash.Add(newRoad);
                    newRoad.Destinations.Add(dest);
                    roads.Add(newRoad);
                }
            }
            return roads;
        }

        private IEnumerable<Road> FindNearbyRoads(Destination dest, int miles)
        {
            throw new NotImplementedException();
        }

        private GeoLocation GetLatLong(string location)
        {
            // API: https://developers.google.com/maps/documentation/geocoding/intro#BYB
            // Example: http://stackoverflow.com/questions/9562775/how-to-retrieve-the-latitude-and-longitude-of-the-inputed-address-using-google-g
            throw new NotImplementedException();
        }
    }
}
