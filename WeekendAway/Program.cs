using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeekendAway
{
    public class GeoLocation
    {
        public double Latitude;
        public double Longitude;

        public GeoLocation(double latitude, double longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
    }

    public class Destination : GeoLocation
    {
        public Destination(double latitude, double longitude) : base(latitude, longitude) { }
    }

    public interface IEdge<T>
    {
        Node<T> Source { get; }
        Node<T> Destination { get; }
    }

    public class Road : IEdge<GeoLocation>
    {
        public GeoLocation Start;
        public GeoLocation End;
        public List<Destination> Destinations = new List<Destination>();

        public Node<GeoLocation> Source
        {
            get
            {
                return new Node<GeoLocation>(Start);
            }
        }

        public Node<GeoLocation> Destination
        {
            get
            {
                return new Node<GeoLocation>(End);
            }
        }

        public Road(GeoLocation start, GeoLocation end)
        {
            Start = start;
            End = end;
        }
    }

    public class Point
    {
        public double X;
        public double Y;
        private GeoLocation start;

        public Point(GeoLocation start)
        {
            this.start = start;
        }

        public Point(double x, double y) { this.X = x; this.Y = y; }
    }

    public class TreeNode<T>
    {
        public T Data;
        public TreeNode<T> Left;
        public TreeNode<T> Right;

        public TreeNode(T data)
        {
            this.Data = data;
        }

        public List<Path> DFS(Node<T> v, int numVertices)
        {
            throw new NotImplementedException();
        }
    }

    public class Graph<T>
    {
        public List<Node<T>> Vertices = new List<Node<T>>();

        public Path MinimumSpanningTree(int numVertices)
        {
            var mst = Kruskal<T>.MinimumSpanningTree(this);
            double min = -1;
            Path found = null;
            foreach (var v in this.Vertices)
            {
                var paths = mst.DFS(v, numVertices);
                foreach (var p in paths)
                {
                    var cost = p.Cost();
                    if (min < 0 || cost < min)
                    {
                        min = cost;
                        found = p;
                    }
                }
            }
            return found;
        }
    }

    public class Kruskal<T>
    {
        private static DisjointSet<TreeNode<T>> roots = new DisjointSet<TreeNode<T>>();

        public static TreeNode<T> MinimumSpanningTree(Graph<T> g)
        {
            foreach (var v in g.Vertices)
            {
                roots.Add(MakeTreeNode(v));
            }
            foreach (var e in FindMinEdge(g))
            {
                var found = roots.Find(e.Source);
                if (found != null)
                {
                    found.Add(e);
                }
                else { throw new InvalidOperationException(); }
            }
            if (roots.Count == 1)
            {
                return roots[0];
            }
            else { throw new InvalidOperationException(); }
        }

        private static TreeNode<T> MakeTreeNode(Node<T> vertex)
        {
            return new TreeNode<T>(vertex.Data);
        }

        private static IEnumerable<IEdge<T>> FindMinEdge(Graph<T> g)
        {
            throw new NotImplementedException();
        }
    }

    public class DisjointSet<T> : List<T>
    {
        public void Union() { }
        public void Find() { }
        public void Insert() { }
        public void Add(T item) { }

        public T Find<T>(T source)
        {
            throw new NotImplementedException();
        }
    }

    public class Node<T>
    {
        public T Data;
        public List<Node<T>> Neighbors = new List<Node<T>>();

        public Node(T data)
        {
            this.Data = data;
        }

        public void Add(IEdge<T> e)
        {
            if (e.Source != this) { throw new InvalidOperationException("Error: Add edge failed. Source doesn't match this node!"); }
            this.Neighbors.Add(e.Destination);
        }
    }

    public class LocationsConfig
    {
        public Location[] Locations { get; set; }
    }

    public class Location
    {
        private string hours;

        public Location(string name, string address, string hours)
        {
            Name = name;
            Address = address;
            this.hours = hours;
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Open { get; set; }
        public string Close { get; set; }
        public string Days { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public GeoLocation GeoLocation { get; internal set; }
    }

    public partial class Program
    {
        static string country = "Hawaii, Oahu";
        static string city = "Honolulu";
        static List<string> locations = new List<string>()
        {
            "Hyatt Place Waikiki Beach hotel address",
            "Outrigger reef Waikiki Beach Resort address",
            "Aston Waikiki Beach Tower",
            "Hilton Hawaiin Village Waikiki Beach Resort & Spa",
            "Hyatt Regency Wakiki Beach Resort & Spa",
            "Sheraton Waikiki"
        };
        static HashSet<Road> roadsHash = new HashSet<Road>();
        static Graph<Point> graph;
        static void Main(string[] args)
        {
            GetLocations();
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

        private static List<Location> GetLocations()
        {
            var result = new List<Location>();
            foreach (var url in ReadFile("search.txt"))
            {
                //Selenium: Navigate to url
                IWebDriver driver = InitPhantomJS();
                try
                {
                    Debug("Loading url '{0}'...", url);
                    driver.Navigate().GoToUrl(url);

                    string name, address, hours;
                    Debug("Looking for Name.....");
                    var nameElem = driver.FindElement(By.CssSelector("data-dtype=d3bn"));
                    Debug(name=nameElem.Text);
                    Debug("");

                    Debug("Looking for Address.....");
                    var addressElem = driver.FindElement(By.CssSelector("data-dtype=d3adr")); 
                    var add2 = driver.FindElement(By.CssSelector(":contains('Address')"));
                    Debug(address=addressElem.Text);
                    Debug("");

                    Debug("Looking for Hours.....");
                    var hoursElem = driver.FindElement(By.CssSelector("data-dtype=d3adr"));
                    var h2 = driver.FindElement(By.CssSelector(":contains('Hours')"));
                    Debug(hours=hoursElem.Text);
                    Debug("");
                                        
                    var loc = new Location(name, address, hours);
                    loc.GeoLocation = GetLatLong(address);

                    result.Add(loc);



                    /*
                                        Debug("Searching for topics...");
                                        IWebElement questionsLink = driver.FindElement(By.LinkText("Questions"));
                                        questionsLink.Click();

                                        var topicsCombo = driver.FindElement(By.Id("topic"));
                                        var options = topicsCombo.FindElements(By.TagName("option"));

                                        if (options == null || options.Count == 0)
                                        {
                                            throw new InvalidOperationException("Error: Unable to find list of topics!");
                                        }

                                        Debug("Successfully Retrieved topics");
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

        

        private static IEnumerable<string> ReadFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private static Graph<Point> CreateGraph(List<Point> points)
        {
            throw new NotImplementedException();
        }

        private static List<Road> GetNearbyRoads(Destination dest)
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

        private static IEnumerable<Road> FindNearbyRoads(Destination dest, int miles)
        {
            throw new NotImplementedException();
        }

        private static GeoLocation GetLatLong(string location)
        {
            // API: https://developers.google.com/maps/documentation/geocoding/intro#BYB
            // Example: http://stackoverflow.com/questions/9562775/how-to-retrieve-the-latitude-and-longitude-of-the-inputed-address-using-google-g
            throw new NotImplementedException();
        }
    }

    public static class Extensions
    {
        public static Destination Add(this List<Destination> locations, GeoLocation geoLoc)
        {
            Destination dest;
            locations.Add(dest = new Destination(geoLoc.Latitude, geoLoc.Latitude));
            return dest;
        }

        public static void Add(this Graph<Point> graph, Road road)
        {
            graph.Add(road);
        }
    }

}
