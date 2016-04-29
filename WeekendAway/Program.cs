using Common;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Toolkit;

namespace WeekendAway
{                
    public partial class Program
    {        
        static void Main(string[] args)
        {
            // TODO: Avoid getting the distance matrix using Maps Web API service calls (when it has already been retrieved previously)
            var tType = ToolkitType.Simple;
            var toolkit = GoogleToolkitFactory.Create(tType);

            var infile = "visit.json";
            var outFile = string.Format("visit_{0}.json", Guid.NewGuid());

            if (!File.Exists(infile)) { throw new InvalidOperationException(string.Format("Error: File does not exist! {0}", infile)); }
            var visit = JsonConvert.DeserializeObject<Visit>(FileIO.ReadAll(infile));
            var visitStart = visit.Start;
            var index = visitStart.Id - 1;
            var origin = visit.Places[index];
            if (!string.IsNullOrWhiteSpace(visitStart.Name))
            {
                if (!origin.Name.ToLower().Equals(visitStart.Name.ToLower()))
                {
                    throw new InvalidOperationException(string.Format("Error: Start Id='{0}' does not match element in Places at index {1}, which is '{2}'", visitStart.Id, index, origin.Name));
                }
            }
            var destinations = visit.Places;
            //var test = toolkit.DistanceMatrix(origin, new List<Place>() { origin });


            var g2 = new Graph<Place>();

            var graph = new Graph<Place>();
            var distCalculations = AddDistances(toolkit, origin, destinations, graph);

            //EnumerateByDuration(
            //    result,
            //    (dest, dist) =>
            //    {
            //        if (dest.Id != s.Id)
            //        {
            //            g2.Add(s, dest, dist);
            //        }
            //    });


            foreach (var newOrigin in visit.Places)
            {
                if (newOrigin.Id == origin.Id || newOrigin.GeoLocation == null) { continue; }
                if (tType == ToolkitType.Api)
                {
                    if (distCalculations.IsNew)
                    {
                        var sec = 15;
                        Logger.DebugWrite("Sleeping for {0} seconds to modulate number of API requests sent to service (to not go over api request limit)");
                        Thread.Sleep(TimeSpan.FromSeconds(sec));
                        Logger.DebugWrite("Sleep complete!");
                    }
                }

                AddDistances(toolkit, newOrigin, destinations, graph);

                //EnumerateByDuration(
                //    result,
                //    (dest, dist) =>
                //    {
                //        if (dest.Id != s.Id)
                //        {
                //            g2.Add(s, dest, dist);
                //        }
                //    });
            }

            //var res = graph.MinimumSpanningTree(num);

            //var json = JsonConvert.SerializeObject(visit);
            //FileIO.Write(outFile, json);

            int numPlacesToVisitPerDay = 5;            
            Logger.Border();
            Logger.Debug("Calculating minimum spanning tree for graph");
            Logger.Border();
            var mst = graph.MinimumSpanningTree();
            Logger.Debug("Minimum Spanning Tree completed");

            //SearchResult<Place> chosen = CheapestRoute(graph, numPlacesToVisitPerDay, mst);

            // Show cheap routes for locations that are preferred
            // From closest to furthest away
            var reference = GetDistanceMatrix(toolkit, origin, destinations);
            var distSort = reference.DistanceSort;
            var nearbyToVisit = numPlacesToVisitPerDay - 1;
            var pathCount = 1;
            foreach (var d in distSort)
            {
                var v = graph.Find(d.Id);
                if (!v.Data.Preferred)
                {
                    Logger.Debug("Skipping DFS at node Id:{0} (not preferred)", v.Data.Id);
                    continue;
                }

                Logger.Debug("Begin DFS at Id:{0} (Name: {1} Address: {2})", v.Data.Id, v.Data.Name, v.Data.Address);
                SearchResult<Place> local = graph.DFS(mst, v, nearbyToVisit);
                local.PathId = pathCount;
                Logger.Debug("Finished DFS at Id:{0}", v.Data.Id);
                Logger.Debug("PATH #{0}: Here are the {1} nearby places to visit. Total Distance Cost: {2}", pathCount, nearbyToVisit, local.Cost);
                Logger.Border();
                PrintRoute(local);
                var here = graph.Find(origin.Id);
                
                var pathStart = local.Route.First().Source;
                foreach (var e in here.Edges)
                {
                    if (e.Destination.Data.Id == pathStart.Data.Id)
                    {
                        Logger.Debug("NOTE: Distance from '{0}' to '{1}' is {2}", here.Data.Name, e.Destination.Data.Name, e.Weight);
                        break;
                    }
                }
                Logger.Border();
                Logger.Debug("Hit any key to continue....");
                Console.ReadKey();
            }
        }

        private static SearchResult<Place> CheapestRoute(Graph<Place> graph, int numPlacesToVisitPerDay, Graph<Place> mst)
        {
            var nearbyToVisit = numPlacesToVisitPerDay - 1;
            Logger.Border();
            Logger.Debug("Performing DFS at each node in Minimum Spanning Tree for {0} nearby places to visit", nearbyToVisit);
            Logger.Debug("Then I will choose the local route that has the cheapest total distance cost.");
            Logger.Border();
            SearchResult<Place> minResult = null;
            var pathCount = 1;
            foreach (var v in mst.Vertices)
            {
                if (!v.Data.Preferred)
                {
                    Logger.Debug("Skipping DFS at node Id:{0} (not preferred)", v.Data.Id);
                    continue;
                }

                Logger.Debug("Begin DFS at Id:{0} (Name: {1} Address: {2})", v.Data.Id, v.Data.Name, v.Data.Address);
                SearchResult<Place> local = graph.DFS(mst, v, nearbyToVisit);
                local.PathId = pathCount;
                Logger.Debug("Finished DFS at Id:{0}", v.Data.Id);
                Logger.Debug("PATH #{0}: Here are the {1} nearby places to visit. Total Distance Cost: {2}", pathCount, nearbyToVisit, local.Cost);
                Logger.Border();
                PrintRoute(local);
                Logger.Border();

                if (minResult == null || local.Cost < minResult.Cost)
                {
                    minResult = local;
                    Logger.Debug("NOTE: Least cost has been changed to this path #{0} with cost: {1}", minResult.PathId, minResult.Cost);
                    Logger.Debug();
                }
                ++pathCount;
                //Logger.Debug("Hit any key to continue...");
                //Console.ReadKey();
            }
            //Console.WriteLine(minResult.Cost);
            var chosen = minResult;
            if (chosen != null)
            {
                Logger.Debug("Minimum spanning tree route found with {0} destinations: Path #{1} was chosen", numPlacesToVisitPerDay, chosen.PathId);
                PrintRoute(chosen);
            }
            return chosen;
        }

        private static void PrintRoute(SearchResult<Place> local)
        {
            var stepCount = 1;
            foreach (var step in local.Route)
            {
                var sourceData = step.Source.Data;
                var destData = step.Destination.Data;
                Logger.Debug("{0}) From {1} go to {2} (Distance:{3})", stepCount++, sourceData.Name, destData.Name, step.Weight);
            }
            Logger.Debug();
        }

        private static DistanceResult AddDistances(IMapToolkit toolkit, Place origin, List<Place> destinations, Graph<Place> graph)
        {
            DistanceResult distCalculations = GetDistanceMatrix(toolkit, origin, destinations);
            
            EnumerateByDistance(
                origin,
                distCalculations,
                (source, dest, dist) =>
                {
                    if (origin.Id != source.Id)
                    {
                        throw new InvalidOperationException(string.Format("Mismatch: SourceId '{0}', Origin.Id '{1}'", source.Id, origin.Id));
                    }
                    if (source.Id != dest.Id)
                    {
                        graph.Add(source, dest, dist);
                        var sourceNode = graph.Find(source);
                        var destNode = graph.Find(dest);
                        if (graph.Find(source.Id) == null) {
                            graph.AddLookupId(source.Id, sourceNode);
                        }
                        if (graph.Find(source.Id) == null)
                        {
                            graph.AddLookupId(dest.Id, destNode);
                        }
                    }
                });

            // Calling SaveToFile again so that DistanceId's are also saved
            if (distCalculations.IsNew)
            {
                SaveToFile(distCalculations.FileName, origin, distCalculations);
            }
            return distCalculations;
        }

        private static DistanceResult GetDistanceMatrix(IMapToolkit toolkit, Place origin, List<Place> destinations)
        {
            // Find by file first
            // {0} toolkit type, {1} Origin Id
            string fileFormat = "{0}_{1}_Distance.json";
            string fileName = string.Format(fileFormat, toolkit.ToolkitType, origin.Id);
            if (File.Exists(fileName))
            {
                var visit = JsonConvert.DeserializeObject<Visit>(FileIO.ReadAll(fileName));
                var distCalculations = new DistanceResult(null) { DistanceSort = visit.Places };
                distCalculations.FileName = fileName;
                distCalculations.IsNew = false;
                return distCalculations;
            }
            else
            {
                // Otherwise call API
                var distCalculations = toolkit.DistanceMatrix(origin, destinations);
                distCalculations.FileName = fileName;
                distCalculations.IsNew = true;
                SaveToFile(fileName, origin, distCalculations);
                return distCalculations;
            }
        }

        private static void SaveToFile(string fileName, Place origin, DistanceResult distCalculations)
        {
            var start = new Place();
            start.Id = origin.Id;
            var visit = new Visit(distCalculations.DistanceSort) { Start = start };
            var content = JsonConvert.SerializeObject(visit);
            FileIO.Write(fileName, content);
        }

        private static void EnumerateByDuration(DistanceResult result, Action<Place,double> nextPlace)
        {
            int rank;
            Logger.Debug();
            Logger.Border();
            Logger.Debug("Results sorted by duration: Origin {0}", result.DurationSort[0].OriginId);
            rank = 1;
            foreach (var d in result.DurationSort)
            {
                d.DurationRank = rank++;
                Logger.Border();
                Logger.Debug("DurationId: {0}", d.DurationRank);
                Logger.Debug("Id: {0}", d.Id);
                Logger.Debug("Name: {0}", d.Name);
                Logger.Debug("Description: {0}", d.Description);
                Logger.Debug("Duration: {0} => {1}", d.DurationText, d.DurationValue);
                Logger.Debug("Distance: {0}", d.DistanceText);
                Logger.Border();
                Logger.Debug();
                if (nextPlace != null) { nextPlace(d,d.DurationValue); }
                //Logger.Debug("Hit enter to continue...");
                //Console.ReadKey();
            }
        }

        private static void EnumerateByDistance(Place origin, DistanceResult distCalculations, Action<Place, Place,double> nextPlace)
        {
            Logger.Debug();
            Logger.Border();
            Logger.Debug("Origin Id:{0} - Enumerating all {1} destinations and adding their edges from origin to the graph", origin.Id, distCalculations.DistanceSort.Count);
            //Logger.Debug("Results sorted by distance: Origin {0}", distCalculations.DistanceSort[0].OriginId);
            var rank = 1;
            foreach (var dest in distCalculations.DistanceSort)
            {
                dest.DistanceRank = rank++;
                Logger.Border();
                Logger.Debug("OriginId: {0}", dest.OriginId);
                Logger.Debug("DistanceId: {0}", dest.DistanceRank);
                Logger.Debug("Id: {0}", dest.Id);
                Logger.Debug("Name: {0}", dest.Name);
                Logger.Debug("Address: {0}", dest.Address);
                Logger.Debug("Description: {0}", dest.Description);
                Logger.Debug("Distance: {0} => {1}", dest.DistanceText, dest.DistanceValue);
                Logger.Debug("Duration: {0}", dest.DurationText);
                Logger.Border();
                Logger.Debug();
                if (nextPlace != null) { nextPlace(origin, dest, dest.DistanceValue); }
                //Logger.Debug("Hit enter to continue...");
                //Console.ReadKey();
            }

            Logger.Debug("Origin Id:{0} - Enumeration complete", origin.Id);
        }
    }    
}
