using Common;
using Google.Maps.DistanceMatrix;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit
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
    }

    public enum GoogleToolkitType
    {
        Api,
        Website
    }
    public class GoogleToolkitFactory
    {
        public static IMapToolkit Create(GoogleToolkitType toolkitType)
        {
            switch(toolkitType)
            {
                case GoogleToolkitType.Api:
                    return new GoogleMapToolkitApi();
                case GoogleToolkitType.Website:
                    return new GoogleMapToolkitWebUi();
                default:
                    throw new InvalidOperationException(string.Format("Error: Unhandled type {0}", toolkitType));
            }
        }
    }

    public class GoogleMapToolkitWebUi : IMapToolkit
    {
        private static string site = @"https://maps.google.com";
        public DistanceResult DistanceMatrix(Place start, List<Place> others)
        {
            var driver = Common.PhantomJSExt.InitPhantomJS();
            try
            {
                var index = 0;
                var reverseLookup = new Dictionary<int, int>();
                for (int j = 0; j < others.Count; j++)
                {
                    var place = others[j];
                    place.OriginId = start.Id;
                    place.DistanceRank = -1;
                    place.DurationRank = -1;
                    place.DistanceValue = double.MaxValue;
                    place.DurationValue = double.MaxValue;

                    if (place.GeoLocation == null)
                    {
                        Logger.Debug("No geolocation info provided for Id:{0} Name:{1} Address:{2}", place.Id, place.Name, place.Address);
                        continue;
                    }

                    reverseLookup.Add(index, j);
                    ++index;

                    driver.Navigate().GoToUrl(site);
                    var q = driver.FindElement(By.Id("searchboxinput"));
                    q.SendKeys(start.Address);
                    var searchBtn = driver.FindElement(By.ClassName("searchbox-searchbutton"));
                    var dirBtn = driver.FindElement(By.CssSelector("[vet='13537']"));
                    dirBtn.Click();

                    var reverse = driver.FindElement(By.ClassName("widget-directions-reverse"));
                    reverse.Click();

                    var destBox = driver.FindElement(By.CssSelector("[placeholder='Choose destination, or click on the map...']"));
                    destBox.SendKeys(place.Address);

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
    }
    public class GoogleMapToolkitApi : IMapToolkit
    {
        public DistanceResult DistanceMatrix(Place start, List<Place> others)
        {
            DistanceMatrixRequest req = new DistanceMatrixRequest();
            req.AddOrigin(new Google.Maps.Waypoint((decimal)start.GeoLocation.Latitude, (decimal)start.GeoLocation.Longitude));
            var index = 0;
            var reverseLookup = new Dictionary<int, int>();
            for (int j = 0; j < others.Count; j++)
            {
                var place = others[j];
                place.OriginId = start.Id;
                place.DistanceRank = -1;
                place.DurationRank = -1;
                place.DistanceValue = double.MaxValue;
                place.DurationValue = double.MaxValue;
                if (place.GeoLocation == null) {
                    Logger.Debug("No geolocation info provided for Id:{0} Name:{1} Address:{2}", place.Id, place.Name, place.Address);
                    continue;
                }
                req.AddDestination(new Google.Maps.Waypoint((decimal)place.GeoLocation.Latitude, (decimal)place.GeoLocation.Longitude));
                reverseLookup.Add(index, j);
                ++index;
            }
            req.Sensor = false;
            req.Units = Google.Maps.Units.imperial;
            DistanceMatrixService svc = new DistanceMatrixService();
            var response = svc.GetResponse(req);
            if (response.Rows == null || response.Rows.Count()==0)
            {
                throw new InvalidOperationException(string.Format("Error: No rows returned, Status {0}", response.Status));
            }
            foreach (var row in response.Rows)
            {
                var resultPos = 0;
                foreach(var cell in row.Elements)
                {
                    //while(skipList.Contains(pos))
                    //{
                    //    ++pos;
                    //}
                    var which = reverseLookup[resultPos];
                    var thisPlace = others[which];
                    var name = thisPlace.Name;
                    Logger.Border();
                    Logger.Debug("ResultId: {0}", resultPos);
                    Logger.Debug("Id: {0}", thisPlace.Id);
                    Logger.Debug("Name: {0}", name);
                    Logger.Debug("Distance: {0}", cell.distance.Text);
                    Logger.Debug("Duration: {0}", cell.duration.Text);
                    Logger.Border();
                    Logger.Debug();
                    thisPlace.OriginId = start.Id;
                    thisPlace.DistanceText = cell.distance.Text;
                    thisPlace.DurationText = cell.duration.Text;
                    thisPlace.DistanceValue = Convert(cell.distance.Value);
                    thisPlace.DurationValue = Convert(cell.duration.Value);
                    //Logger.Debug("Hit enter to continue...");
                    //Console.ReadKey();
                    resultPos++;
                }
            }

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
            

            return new DistanceResult(response) { DistanceSort = distSort, DurationSort = durSort };
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
