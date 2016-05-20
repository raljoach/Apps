using Common;
using Common.Web.Driver;
using Google.Maps.DistanceMatrix;
using Google.Maps.Geocoding;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeekendAway.Common;

namespace WeekendAway.Toolkit
{
    public class DistanceResult
    {
        public DistanceResult(DistanceMatrixResponse matrix)
        {
            this.Matrix = matrix;

        }
        public DistanceMatrixResponse Matrix { get; set; }
        public List<Place> DistanceSort { get; set; }
        public List<Place> DurationSort { get; set; }
        public string FileName { get; set; }
        public bool IsNew { get; set; }
    }

    public enum ToolkitType
    {
        Simple,
        Api,
        Website
    }
    public class GoogleToolkitFactory
    {
        public static IMapToolkit Create(ToolkitType toolkitType)
        {
            switch(toolkitType)
            {
                case ToolkitType.Api:
                    return new GoogleMapToolkitApi();
                case ToolkitType.Website:
                    return new GoogleMapToolkitWebUi();
                case ToolkitType.Simple:
                    return new SimpleDistanceToolkit();
                default:
                    throw new InvalidOperationException(string.Format("Error: Unhandled type {0}", toolkitType));
            }
        }
    }

    public enum BrowserType
    {
        Chrome,
        Phantom
    }

    public abstract class MapToolkitBase
    {
        protected bool Init(Place dest, Place origin)
        {
            dest.OriginId = origin.Id;
            dest.DistanceRank = -1;
            dest.DurationRank = -1;
            dest.DistanceValue = double.MaxValue;
            dest.DurationValue = double.MaxValue;
            if (dest.GeoLocation == null)
            {
                Logger.Debug("No geolocation info provided for Id:{0} Name:{1} Address:{2}", dest.Id, dest.Name, dest.Address);
                return false;
            }
            return true;
        }

        protected static void SetDistance(Place origin, int resultId, Place dest, string name, string distText, double distVal, string durText, double durVal)
        {
            Logger.Border();
            Logger.Debug("ResultId: {0}", resultId);
            Logger.Debug("Id: {0}", dest.Id);
            Logger.Debug("Name: {0}", name);
            Logger.Debug("Distance: {0}", distText);
            Logger.Debug("Duration: {0}", durText);
            Logger.Border();
            Logger.Debug();
            dest.OriginId = origin.Id;
            dest.DistanceText = distText;
            dest.DurationText = durText;
            dest.DistanceValue = distVal;
            dest.DurationValue = durVal;
            //Logger.Debug("Hit enter to continue...");
            //Console.ReadKey();
        }

        protected List<Place> CreateSortByDuration(List<Place> others)
        {
            var durSort = new List<Place>();
            durSort.AddRange(others);

            // Sort by duration
            durSort.Sort((x, y) =>
            {
                if (x.GeoLocation == null && y.GeoLocation == null) return 0;
                else if (x.GeoLocation == null) return 1;
                else if (y.GeoLocation == null) return -1;
                else return x.DurationValue.CompareTo(y.DurationValue);
            });
            return durSort;
        }

        protected List<Place> CreateSortByDistance(List<Place> others)
        {
            var distSort = new List<Place>();
            distSort.AddRange(others);

            // Sort by distance
            distSort.Sort((x, y) =>
            {
                if (x.GeoLocation == null && y.GeoLocation == null) return 0;
                else if (x.GeoLocation == null) return 1;
                else if (y.GeoLocation == null) return -1;
                else return x.DistanceValue.CompareTo(y.DistanceValue);
            });
            return distSort;
        }
    }

    public class SimpleDistanceToolkit : MapToolkitBase, IMapToolkit
    {
        public ToolkitType ToolkitType
        {
            get
            {
                return ToolkitType.Simple;
            }
        }

        public DistanceResult DistanceMatrix(Place origin, List<Place> destinations)
        {
            var resultPos = 0;
            foreach (var dest in destinations)
            {
                //if (dest.Visited) { continue; }
                if (!Init(dest, origin)) { continue; }                                
                var x = dest.GeoLocation.Longitude - origin.GeoLocation.Longitude;
                var y = dest.GeoLocation.Latitude - origin.GeoLocation.Latitude;
                var dist = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
                SetDistance(origin, resultPos, dest, dest.Name, dist.ToString(), dist, string.Empty, int.MinValue);
                ++resultPos;
            }
            var distSort = CreateSortByDistance(destinations);
            return new DistanceResult(null) { DistanceSort = distSort, DurationSort = new List<Place>() };
        }
    }

    public class GoogleMapToolkitWebUi : MapToolkitBase, IMapToolkit
    {
        public static BrowserType browserType = BrowserType.Chrome;

        private static string site = @"https://maps.google.com";

        public ToolkitType ToolkitType
        {
            get
            {
                return ToolkitType.Website;
            }
        }

        public DistanceResult DistanceMatrix(Place origin, List<Place> destinations)
        {
            var driver = GetWebDriver();
            try
            {
                var index = 0;
                var reverseLookup = new Dictionary<int, int>();
                for (int j = 0; j < destinations.Count; j++)
                {
                    var dest = destinations[j];
                    //if (dest.Visited) { continue; }
                    if (!Init(dest,origin)) { continue; }

                    reverseLookup.Add(index, j);
                    ++index;

                    driver.Navigate().GoToUrl(site);
                    var q = driver.FindElement(By.CssSelector("[arial-label='Search Google Maps']"));
                    q.SendKeys(origin.Address);
                    var searchBtn = driver.FindElement(By.ClassName("searchbox-searchbutton"));


                    var magBtn = driver.FindElement(By.CssSelector("[arial-label='Search']"));
                    magBtn.Click();

                    var dirBtn = driver.FindElement(By.CssSelector("[vet='13537']"));
                    dirBtn.Click();

                    var reverse = driver.FindElement(By.ClassName("widget-directions-reverse"));
                    reverse.Click();

                    var destBox = driver.FindElement(By.CssSelector("[placeholder='Choose destination, or click on the map...']"));
                    destBox.SendKeys(dest.Address);

                    var driveModeBtn = driver.FindElement(By.CssSelector("[data-tooltip='Driving']")).FindElement(By.XPath(".."));
                    driveModeBtn.Click();

                    var magnifierBtn = driver.FindElement(By.CssSelector("[aria-label='Search']"));
                    magnifierBtn.Click();
                }
            }
            finally
            {
                driver.Quit();
            }
            throw new NotImplementedException();
        }

        private static IWebDriver GetWebDriver()
        {
            switch (browserType)
            {
                case BrowserType.Phantom:
                    return PhantomJSExt.InitPhantomJS();
                case BrowserType.Chrome:
                    return new ChromeDriver();
                default:
                    throw new InvalidOperationException(string.Format("Error: Unhandled browser type {0}"));
            }
        }
    }
    public class GoogleMapToolkitApi : MapToolkitBase, IMapToolkit
    {
        public ToolkitType ToolkitType
        {
            get
            {
                return ToolkitType.Api;
            }
        }

        public GeoLocation GetLatLong(string address)
        {
            GeocodingRequest request = new GeocodingRequest();
            request.Address = address;
            request.Sensor = false;
            GeocodingService svc = new GeocodingService();
            var response = svc.GetResponse(request);
            var result = response.Results.First();

            var longitude = result.Geometry.Location.Longitude;
            var latitude = result.Geometry.Location.Latitude;
            var addr = result.FormattedAddress;
            Logger.Debug("Full Address: " + addr);         // "1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA"
            Logger.Debug("Latitude: " + latitude);   // 37.4230180
            Logger.Debug("Longitude: " + longitude); // -122.0818530
            var loc = new GeoLocation(longitude, latitude, addr);
            return loc;
        }

        public DistanceResult DistanceMatrix(Place origin, List<Place> destinations)
        {
            DistanceMatrixRequest req = new DistanceMatrixRequest();
            req.AddOrigin(new Google.Maps.Waypoint((decimal)origin.GeoLocation.Latitude, (decimal)origin.GeoLocation.Longitude));
            var index = 0;
            var reverseLookup = new Dictionary<int, int>();
            for (int j = 0; j < destinations.Count; j++)
            {
                var dest = destinations[j];
                //if (dest.Visited) { continue; }
                if (!Init(dest, origin)) { continue; }
                req.AddDestination(new Google.Maps.Waypoint((decimal)dest.GeoLocation.Latitude, (decimal)dest.GeoLocation.Longitude));
                reverseLookup.Add(index, j);
                ++index;
            }
            req.Sensor = false;
            req.Units = Google.Maps.Units.imperial;
            DistanceMatrixResponse response = GoogleServiceRequest(req);
            if (response.Rows == null || response.Rows.Count() == 0)
            {
                throw new InvalidOperationException(string.Format("Error: No rows returned, Status {0}", response.Status));
            }
            foreach (var row in response.Rows)
            {
                var resultPos = 0;
                foreach (var cell in row.Elements)
                {
                    var which = reverseLookup[resultPos];
                    var dest = destinations[which];
                    var name = dest.Name;
                    var distText = cell.distance.Text;
                    var distVal = Convert(cell.distance.Value);
                    var durText = cell.duration.Text;
                    var durVal = Convert(cell.duration.Value);
                    SetDistance(origin, resultPos, dest, name, distText, distVal, durText, durVal);
                    resultPos++;
                }
            }

            List<Place> distSort = CreateSortByDistance(destinations);
            List<Place> durSort = CreateSortByDuration(destinations);

            return new DistanceResult(response) { DistanceSort = distSort, DurationSort = durSort };
        }

        private static DistanceMatrixResponse GoogleServiceRequest(DistanceMatrixRequest req)
        {
            Logger.Border('*');
            Logger.Debug("Initiating Google Service API request");
            DistanceMatrixService svc = new DistanceMatrixService();
            var response = svc.GetResponse(req);
            Logger.Debug("Response received for Google Service API request");
            Logger.Debug("Status: {0}", response.Status);
            Logger.Debug("Origins: {0}", response.Rows != null ? response.Rows.Count():0);
            Logger.Debug("Destinations: {0}", response.Rows != null && response.Rows.Count() > 0 ? response.Rows[0].Elements.Count() / response.Rows.Count() : 0);
            Logger.Debug("Calculations: {0}", response.Rows != null && response.Rows.Count() > 0 ? response.Rows[0].Elements.Count() : 0);
            Logger.Border('*');
            return response;
        }


        private static double Convert(string text)
        {
            StringBuilder sb = new StringBuilder();
            foreach(var ch in text)
            {
                if(Char.IsNumber(ch) || ch=='.')
                {
                    sb.Append(ch);
                }
                else
                {
                    break;
                }
            }
            return double.Parse(sb.ToString());
        }
    }
}
