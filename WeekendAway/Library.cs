﻿using Common;
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

        public static Visit GetDestinations(string file)
        {
            throw new NotImplementedException();
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

        double Weight { get; }
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

        public double Weight
        {
            get
            {
                return -1;
            }
        }

        public Road(GeoLocation start, GeoLocation end)
        {
            Start = start;
            End = end;
        }
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
        /*
        public List<Path> DFS(Node<T> v, int numVertices)
        {
            throw new NotImplementedException();
        }*/
    }

    public class Edge<T> : IEdge<T>, IComparable<Edge<T>>
    {
        public Edge(Node<T> source, Node<T> dest, double weight)
        {
            this.Source = source;
            this.Destination = dest;
            this.Weight = weight;
            this.IsBackTrack = false;
        }

        public Node<T> Source { get; }

        public Node<T> Destination { get; }

        public double Weight { get; }
        public bool IsBackTrack { get; internal set; }

        public int CompareTo(Edge<T> other)
        {
            return this.Weight.CompareTo(other.Weight);
            //return Convert.ToInt32(this.Weight).CompareTo(Convert.ToInt32(other.Weight));
        }

        public override bool Equals(object obj)
        {
            var other = obj as Edge<T>;
            if (other == null) return false;
            return this.Source.Equals(other.Source) && this.Destination.Equals(other.Destination);
        }
    }

    public class SearchResult<T>
    {
        public double Cost = 0;
        public HashSet<Edge<T>> Route = new HashSet<Edge<T>>();
        public HashSet<Node<T>> Visited = new HashSet<Node<T>>();

        public int PathId { get; internal set; }

        public void AddEdge(Edge<T> e, bool isBackTrack = false)
        {
            e.IsBackTrack = isBackTrack;
            var seen = !this.Route.Add(e);
            if (seen)
            {
                Logger.Debug("Edge already added");
            }
            this.Cost += e.Weight;
            this.Visited.Add(e.Source);
            this.Visited.Add(e.Destination);
        }

        public void RemoveEdge(Edge<T> e)
        {
            this.Route.Remove(e);
            this.Cost -= e.Weight;
        }
    }

    public class Graph<T> where T : IItem
    {
        //private Dictionary<T, Node<T>> nodeLookUp = new Dictionary<T, Node<T>>();
        private Dictionary<int, Node<T>> idLookup = new Dictionary<int, Node<T>>();
        public HashSet<Node<T>> Visited = new HashSet<Node<T>>();

        public List<Node<T>> Vertices
        {
            get
            {
                var list = new List<Node<T>>();
                foreach (var v in idLookup.Values)//nodeLookUp.Values)
                {
                    list.Add(v);
                }
                return list;
            }
        }

        public int NumVertices { get { return idLookup.Count; } }

        public int NumEdges { get { return this.Edges.Count; } }
        public HashSet<Edge<T>> Edges = new HashSet<Edge<T>>();

        public Graph<T> MinimumSpanningTree()
        {
            return Kruskal<T>.MinimumSpanningTree(this);
        }

        public SearchResult<T> MinimumSpanningTree(int visitCount)
        {
            var mst = Kruskal<T>.MinimumSpanningTree(this);
            SearchResult<T> minResult = null;
            foreach (var v in mst.Vertices)
            {

                SearchResult<T> result = DFS(mst, v, visitCount - 1);
                if (minResult == null || result.Cost < minResult.Cost)
                {
                    minResult = result;
                }
            }
            //Console.WriteLine(minResult.Cost);
            return minResult;
        }

        public static SearchResult<T> DFS(Graph<T> graph, Node<T> origin, int visitCount)
        {
            var path = new SearchResult<T>();
            var bestPath = new SearchResult<T>() { Cost = int.MaxValue };
            //graph.Visited.Add(origin);
            var availableDestCount = graph.Vertices.Count - graph.Visited.Count;
            DFS(graph, path, bestPath, origin, visitCount, availableDestCount);
            /*
            if (bestPath.Route.Count > 0)
            {
                foreach (var e in bestPath.Route)
                {
                    graph.Visited.Add(e.Destination);
                }
            }*/
            return bestPath;
        }

        private static void DFS(Graph<T> graph, SearchResult<T> path, SearchResult<T> bestPath, Node<T> root, int visitCount, int availableDestCount)
        {
            var neighbors = root.Edges;
            foreach (var e in neighbors)
            {
                //if (path.Route.Contains(e)) { continue; }
                bool edgeAdded = false;
                if (e.Source != root)
                {
                    throw new InvalidOperationException("Error: We have a neighbor edge, whose source is not equal to the current node!");
                    /*
                    edgeAdded = true;
                    path.AddEdge(e);
                    --edgeCount;
                    if (edgeCount > 0)
                    {
                        DFS(graph, path, bestPath, e.Source, edgeCount);
                    }
                    */
                }
                else if (e.Destination != root) // Let's look at the destination as long as it's not pointing back to itself
                {
                    edgeAdded = true;
                    /*var seenBefore = graph.Visited.Count(n => n.Data.Equals(e.Destination))>0;
                    var visitedBefore = seenBefore || path.Visited.Contains(e.Destination) || graph.Visited.Contains(e.Destination);
                    if(visitedBefore) {
                        continue;
                    }*/
                    bool visitedBefore = false;
                    bool isBackTrack = false;
                    if (path.Visited.Contains(e.Destination))
                    {
                        isBackTrack = true;
                        visitedBefore = true;
                        continue;
                    }

                    if (graph.Visited.Contains(e.Destination))
                    {
                        isBackTrack = true;
                        visitedBefore = true;
                        continue;
                    }

                    if (path.Route.Contains(e))
                    {
                        isBackTrack = true;
                        visitedBefore = true;
                        continue;
                    }
                    path.AddEdge(e, isBackTrack);

                    if (!visitedBefore) // Only count the unique places we visit
                    {
                        --availableDestCount;
                        --visitCount;
                    }
                    if (visitCount > 0)
                    {
                        DFS(graph, path, bestPath, e.Destination, visitCount, availableDestCount);
                    }
                }

                // OK, so we've visited all the places we wanted to, in terms of count
                if (visitCount == 0)
                {
                    if (path.Cost < bestPath.Cost) // Is this path the best path in terms of cost?
                    {
                        bestPath.Route.Clear();
                        bestPath.Cost = 0;
                        foreach (var pe in path.Route)
                        {
                            bestPath.AddEdge(pe);
                        }
                    }
                }

                // So we tried the path thru our neighbor
                // Let's undo it before we go onto the next neighbor (Backtrack)
                if (edgeAdded)
                {
                    path.RemoveEdge(e);
                    ++visitCount;
                }
            }
        }

        public void Remove(List<Edge<T>> edges)
        {
            foreach (var e in edges)
            {
                var sourceNode = e.Source;
                var destNode = e.Destination;

                sourceNode.Remove(e);
                var success = this.Edges.Remove(e);
                if (!success)
                {
                    Logger.Debug("WARNING: Unable to remove edge.....will manually look for match and remove");
                    var numRem = this.Edges.RemoveWhere(
                        (edge) =>
                        {
                            var res = e.Source.Data.Equals(edge.Source.Data) && e.Destination.Data.Equals(edge.Destination.Data);
                            return res;
                        });

                    if (numRem == 0)
                    {
                        Logger.Debug("Failed to remove edge");
                    }
                }
                var found = false;
                success = this.Vertices.Remove(e.Destination);
                if (!success)
                {
                    for (int j = this.Vertices.Count - 1; j >= 0; j--)
                    {
                        var data = this.Vertices[j].Data;
                        if (data.Equals(e.Destination.Data))
                        {
                            this.Vertices.RemoveAt(j);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    Logger.Debug("Error: Could not find destination node");
                }
                found = false;
                success = this.Vertices.Remove(e.Source);
                if (!success)
                {
                    for (int j = this.Vertices.Count - 1; j >= 0; j--)
                    {
                        var data = this.Vertices[j].Data;
                        if (data.Equals(e.Source.Data))
                        {
                            this.Vertices.RemoveAt(j);
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    Logger.Debug("Error: Could not find source node");
                }
            }
        }

        //public Path Prototype_MinimumSpanningTree(int numVertices)
        //{
        //    var mst = Kruskal<T>.MinimumSpanningTree(this);
        //    double min = -1;
        //    Path found = null;
        //    foreach (var v in this.Vertices)
        //    {
        //        var paths = mst.DFS(v, numVertices);
        //        foreach (var p in paths)
        //        {
        //            var cost = p.Cost();
        //            if (min < 0 || cost < min)
        //            {
        //                min = cost;
        //                found = p;
        //            }
        //        }
        //    }
        //    return found;
        //}

        public void Add(T source, T dest, double weight)
        {
            //var sourceNode = FindAdd(source);
            //var destNode = FindAdd(dest);
            var sourceNode = Find(source.Id);
            var destNode = Find(dest.Id);
            if (sourceNode == null)
            {
                sourceNode = new Node<T>(source);
            }
            if (destNode == null)
            {
                destNode = new Node<T>(dest);
            }

            sourceNode.Add(new Edge<T>(sourceNode, destNode, weight));
            Add(new Edge<T>(sourceNode, destNode, weight));
        }

        public void Add(Edge<T> edge)
        {
            if (!this.Edges.Contains(edge))
            {
                this.Edges.Add(edge);
            }

            AddToLookup(edge.Source);
            AddToLookup(edge.Destination);

        }

        private void AddToLookup(Node<T> node)
        {
            //if (!nodeLookUp.ContainsKey(node.Data))           
            //{
            //    nodeLookUp.Add(node.Data, node);
            //}
            if (!idLookup.ContainsKey(node.Data.Id))
            {
                AddLookupId(node.Data.Id, node);
            }
        }

        /*
        private Node<T> FindAdd(T source)
        {
            Node<T> node = Find(source);
            if (node==null)
            {
                nodeLookUp.Add(source, node = new Node<T>(source));
            }

            return node;
        }
        */

        /*
    public Node<T> Find(T source)
    {
        if (nodeLookUp.ContainsKey(source))
        {
            return nodeLookUp[source];
        }
        return null;
    }
    */
        public Node<T> Find(int id)
        {
            if (idLookup.ContainsKey(id))
            {
                return idLookup[id];
            }
            return null;
        }

        public void AddLookupId(int id, Node<T> node)
        {
            if (node == null)
            {
                throw new InvalidOperationException();
            }
            idLookup.Add(id, node);
        }
    }

    public class Kruskal<T> where T : IItem
    {
        public static /*TreeNode<T>*/Graph<T> MinimumSpanningTree(Graph<T> graph)
        {
            int numVert = graph.NumVertices;
            //List<Edge<int>> result = new List<Edge<int>>();
            Graph<T> resultGraph = new Graph<T>();
            int i = 0; // iterator for sorted edges

            //Sort edges
            var edges = new List<Edge<T>>();
            foreach (var edge in graph.Edges)
            {
                edges.Add(edge);
            }
            edges.Sort();

            //Create a unique subset for each vertex
            var subsets = new Dictionary<Node<T>, Subset<Node<T>>>();
            foreach (var v in graph.Vertices)
            {
                subsets.Add(v, new Subset<Node<T>>(v, 0));
            }

            //while (result.Count < (numVert - 1))
            while (resultGraph.Edges.Count < (numVert - 1))
            {
                //Pick the smallest edge
                var nextEdge = edges[i++];
                var x = Find(subsets, nextEdge.Source);
                var y = Find(subsets, nextEdge.Destination);

                if (x != y)
                {
                    //result.Add(nextEdge);
                    resultGraph.Add(nextEdge);
                    Union(subsets, x, y);
                }
            }
            //return result;
            return resultGraph;
        }

        private static Node<T> Find(Dictionary<Node<T>, Subset<Node<T>>> subsets, Node<T> i)
        {
            if (subsets[i].Parent != i)
                subsets[i].Parent = Find(subsets, subsets[i].Parent);

            return subsets[i].Parent;
        }

        private static void Union(Dictionary<Node<T>, Subset<Node<T>>> subsets, Node<T> x, Node<T> y)
        {
            var xroot = Find(subsets, x);
            var yroot = Find(subsets, y);

            // Attach smaller rank tree under root of high rank tree
            // (Union by Rank)
            if (subsets[xroot].Rank < subsets[yroot].Rank)
            {
                subsets[xroot].Parent = yroot;
            }
            else if (subsets[xroot].Rank > subsets[yroot].Rank)
            {
                subsets[yroot].Parent = xroot;
            }

            // If ranks are same, then make one as root and increment
            // its rank by one
            else
            {
                subsets[yroot].Parent = xroot;
                subsets[xroot].Rank++;
            }
        }
    }
    public class Subset<T>
    {
        public Subset(T parent, int rank) { this.Parent = parent; this.Rank = rank; }
        public T Parent;
        public int Rank;
    }

    /*
    public class Graph<T>
    {
        public int NumVertices { get { return this.Vertices.Count; } }
        public int NumEdges { get { return this.Edges.Count; } }
        public HashSet<Edge<T>> Edges = new HashSet<Edge<T>>();
        public Dictionary<T, List<Edge<T>>> Vertices = new Dictionary<T, List<Edge<T>>>();

        public void AddEdge(Edge<T> edge)
        {
            if (!this.Edges.Contains(edge))
            {
                this.Edges.Add(edge);
                AddEdge(edge.Source, edge);
                AddEdge(edge.Destination, edge);
            }
        }

        private void AddEdge(T key, Edge<T> edge)
        {
            List<Edge<T>> list;
            if (!this.Vertices.ContainsKey(key))
            {
                list = new List<Edge<T>>();
                this.Vertices.Add(key, list);
            }
            else
            {
                list = this.Vertices[key];
            }
            list.Add(edge);
        }
    }

    

    public class Edge<T> : IComparable<Edge<T>>
    {
        public Edge(T v1, T v2, int weight)
        {
            this.Source = v1;
            this.Destination = v2;
            this.Weight = weight;
        }

        public T Source;
        public T Destination;
        public int Weight;

        public int CompareTo(Edge<T> other)
        {
            return Convert.ToInt32(this.Weight).CompareTo(Convert.ToInt32(other.Weight));
        }
    }
    
    public class Prototype_Kruskal<T>
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
    */
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
        public List<Edge<T>> Edges = new List<Edge<T>>();

        public Node(T data)
        {
            this.Data = data;
        }

        public void Add(Edge<T> e)
        {
            if (e.Source != this) { throw new InvalidOperationException("Error: Add edge failed. Source doesn't match this node!"); }
            this.Neighbors.Add(e.Destination);
            this.Edges.Add(e);
        }

        public void Remove(Edge<T> e)
        {
            var success = this.Neighbors.Remove(e.Destination);
            if (!success) { throw new InvalidOperationException("Error: Unable to remove neighbor!"); }
            success = this.Edges.Remove(e);
            if (!success) { throw new InvalidOperationException("Error: Unable to remove from edge list!"); }
        }

        public override bool Equals(object obj)
        {
            var other = obj as Node<T>;
            if (other == null) return false;
            return this.Data.Equals(other.Data);
        }
    }

    public static class Extensions
    {
        public static string PrintRoute(this SearchResult<Place> local)
        {
            var stepCount = 1;
            var sb = new StringBuilder();
            foreach (var step in local.Route)
            {
                var sourceData = step.Source.Data;
                var destData = step.Destination.Data;
                string backtrack = step.IsBackTrack ? "[BACKTRACK]" : string.Empty;
                sb.AppendLine(string.Format("{0}) From {1} go to {2} (Distance:{3}) {4}", stepCount++, sourceData.Name + " [" + sourceData.Address + "] ", destData.Name + " [" + destData.Address + "] ", step.Weight, backtrack));

            }
            sb.AppendLine();
            var str = sb.ToString();
            Logger.Debug(str);
            return str;
        }

        public static string Print(this Edge<Place> e)
        {
            var sourceData = e.Source.Data;
            var destData = e.Destination.Data;
            string backtrack = e.IsBackTrack ? "[BACKTRACK]" : string.Empty;
            var str = string.Format("From {0} go to {1} (Distance:{2}) {3}", sourceData.Name, destData.Name, e.Weight, backtrack);
            return str;
        }

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
