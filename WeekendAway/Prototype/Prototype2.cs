using Common;
using Newtonsoft.Json;
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
    class Prototype2
    {
        public Prototype2()
        {
            // TODO: Avoid getting the distance matrix using Maps Web API service calls (when it has already been retrieved previously)
            var tType = ToolkitType.Simple;
            var toolkit = GoogleToolkitFactory.Create(tType);

            var infile = "visit3.json";
            var outFile = string.Format("visit_{0}.json", Guid.NewGuid());

            var sourceFile = infile;//@"C:\Users\ralph.joachim\Documents\Visual Studio 2015\Projects\WeekendAway\" + infile;

            if (!File.Exists(infile)) { throw new InvalidOperationException(string.Format("Error: File does not exist! {0}", infile)); }
            var visit = JsonConvert.DeserializeObject<Visit>(FileIO.ReadAll(infile));

            var anyUpdate = false;
            foreach (var v in visit.Places)
            {
                if (v.GeoLocation == null)
                {
                    Logger.DebugWrite("Id:{0}, Name:{1} has null GeoLocation", v.Id, v.Name);
                    if (!string.IsNullOrWhiteSpace(v.Address))
                    {
                        anyUpdate = true;
                        Logger.Debug(", a valid Address:{0}", v.Address);
                        GeoLocation loc = new GoogleMapToolkitApi().GetLatLong(v.Address);
                        v.GeoLocation = loc;
                    }
                    else
                    {
                        Logger.Debug(", but no address");
                    }

                }
            }
            if (anyUpdate)
            {
                Logger.Debug("Updating Source file with new Geocoordinates, '{0}' ", sourceFile);
                FileIO.Write(sourceFile, JsonConvert.SerializeObject(visit));
            }

            var visitStart = visit.Start;
            var origin = FindOrigin(visit, visitStart);
            if (!string.IsNullOrWhiteSpace(visitStart.Name))
            {
                if (!origin.Name.ToLower().Equals(visitStart.Name.ToLower()))
                {
                    throw new InvalidOperationException(string.Format("Error: Start Id='{0}' does not match returned Origin element, which is '{1}'", visitStart.Id, origin.Name));
                }
            }
            var destinations = visit.Places;
            /*for(int j=destinations.Count-1; j>=0; j--)
            {
                var dest = destinations[j];
                if(dest.Visited)
                {
                    destinations.RemoveAt(j);
                }
            }*/
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
                //if (newOrigin.Visited) { continue; }
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

            int numPlacesToVisitPerDay = 12;
            Logger.Border();
            Logger.Debug("Calculating minimum spanning tree for graph");
            Logger.Border();
            var mst = graph.MinimumSpanningTree();
            var x = graph.Find(origin.Id);
            if (x == null) { throw new InvalidOperationException(); }
            graph.Visited.Add(x);
            var w = mst.Find(origin.Id);
            if (w == null) { throw new InvalidOperationException(); }
            mst.Visited.Add(w);
            Logger.Debug("Minimum Spanning Tree completed");

            //SearchResult<Place> chosen = CheapestRoute(graph, numPlacesToVisitPerDay, mst);

            // Show cheap routes for locations that are preferred
            // From closest to furthest away
            var reference = GetDistanceMatrix(toolkit, origin, destinations);
            var distSort = reference.DistanceSort;
            var nearbyToVisit = numPlacesToVisitPerDay - 1;
            var pathCount = 1;
            var resultsFile = "results.txt";
            string output = string.Empty;
            foreach (var d in distSort)
            {
                /*var count = mst.Visited.Count(n => n.Data.Id == d.Id);
                if (count > 0)
                {
                    Logger.Debug("Skipping DFS at node Id:{0} because it has been VISITED already!", d.Id);
                    continue;
                }*/

                var v = graph.Find(d.Id);
                if (v == null)
                {
                    var msg = string.Format("Error: Node with id:{0} name:{1} cannot be found in original graph!", d.Id, d.Name);
                    continue;
                    //throw new InvalidOperationException(msg);
                }
                //if (!v.Data.Preferred)
                //{
                //    Logger.Debug("Skipping DFS at node Id:{0} (not preferred)", v.Data.Id);
                //    continue;
                //}



                Logger.Debug("Begin DFS at Id:{0} (Name: {1} Address: {2})", v.Data.Id, v.Data.Name, v.Data.Address);
                SearchResult<Place> local = Graph<Place>.DFS(mst, v, nearbyToVisit);
                local.PathId = pathCount;
                Logger.Debug("Finished DFS at Id:{0}", v.Data.Id);

                if (local.Route.Count == 0)
                {
                    Logger.Debug("No new route for unvisited places starting from Id:{0}, Name:{1}", v.Data.Id, v.Data.Name);
                    continue;
                }

                output += "\r\n" + string.Format("PATH #{0}: Here are the {1} nearby places to visit. Total Distance Cost: {2}", pathCount, nearbyToVisit, local.Cost);
                Logger.Debug(output);
                output += "\r\n" + Logger.Border();
                output += "\r\n" + local.PrintRoute();
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
                //Logger.Debug("Hit any key to continue....");
                //Console.ReadKey();
                ++pathCount;

                //Now remove path from MST
                //mst.Remove(local.Route.ToList());
            }

            FileIO.Write(resultsFile, output);
            Logger.Debug("Program ended. Hit any key to exit....");
            Console.ReadKey();
        }

        private static Place FindOrigin(Visit visit, Place visitStart)
        {
            Place found = null;
            var index = visitStart.Id - 1;
            var origin = visit.Places[index];
            if (origin.Id == visitStart.Id)
            {
                found = origin;
            }
            else
            {
                if (origin.Id > visitStart.Id)
                {
                    for (var i = index - 1; i >= 0; i--)
                    {
                        var thisPlace = visit.Places[i];
                        if (thisPlace.Id == visitStart.Id)
                        {
                            found = thisPlace;
                            break;
                        }
                    }
                }
                else
                {
                    for (var i = index + 1; i < visit.Places.Count; i++)
                    {
                        var thisPlace = visit.Places[i];
                        if (thisPlace.Id == visitStart.Id)
                        {
                            found = thisPlace;
                            break;
                        }
                    }
                }
            }
            return found;
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
                /*if (!v.Data.Preferred)
                {
                    Logger.Debug("Skipping DFS at node Id:{0} (not preferred)", v.Data.Id);
                    continue;
                }*/

                Logger.Debug("Begin DFS at Id:{0} (Name: {1} Address: {2})", v.Data.Id, v.Data.Name, v.Data.Address);
                SearchResult<Place> local = Graph<Place>.DFS(mst, v, nearbyToVisit);
                local.PathId = pathCount;
                Logger.Debug("Finished DFS at Id:{0}", v.Data.Id);
                Logger.Debug("PATH #{0}: Here are the {1} nearby places to visit. Total Distance Cost: {2}", pathCount, nearbyToVisit, local.Cost);
                Logger.Border();
                local.PrintRoute();
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
                chosen.PrintRoute();
            }
            return chosen;
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
                        //if(dest.Visited) { return; }
                        graph.Add(source, dest, dist);
                        var sourceNode = graph.Find(source.Id);
                        var destNode = graph.Find(dest.Id);

                        if (graph.Find(source.Id) == null)
                        {
                            graph.AddLookupId(source.Id, sourceNode);
                        }
                        if (graph.Find(dest.Id) == null)
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

        private static void EnumerateByDuration(DistanceResult result, Action<Place, double> nextPlace)
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
                if (nextPlace != null) { nextPlace(d, d.DurationValue); }
                //Logger.Debug("Hit enter to continue...");
                //Console.ReadKey();
            }
        }

        private static void EnumerateByDistance(Place origin, DistanceResult distCalculations, Action<Place, Place, double> nextPlace)
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
