using Common;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeekendAway
{
    //Class: Logger
    public partial class Program
    {
        private static void Debug(string message, params object[] args)
        {
            Console.WriteLine(message, args);
        }
    }

    public class Destination : GeoLocation
    {
        public Destination(double latitude, double longitude) : base(longitude, latitude) { }
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
