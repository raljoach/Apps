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
            var tType = GoogleToolkitType.Website;
            var toolkit = GoogleToolkitFactory.Create(tType);

            var infile = "visit.json";
            var outFile = string.Format("visit_{0}.json", Guid.NewGuid());

            if (!File.Exists(infile)) { throw new InvalidOperationException(string.Format("Error: File does not exist! {0}", infile)); }
            var visit = JsonConvert.DeserializeObject<Visit>(FileIO.ReadAll(infile));
            var start = visit.Start;
            var index = start.Id - 1;
            var s = visit.Places[index];
            if (!string.IsNullOrWhiteSpace(start.Name))
            {
                if (!s.Name.ToLower().Equals(start.Name.ToLower()))
                {
                    throw new InvalidOperationException(string.Format("Error: Start Id='{0}' does not match element in Places at index {1}, which is '{2}'", start.Id, index, s.Name));
                }
            }
            var others = visit.Places;
            //var test = MapToolkit.DistanceMatrix(s, new List<Place>() { s });

            var graph = new Graph<Place>();
            var g2 = new Graph<Place>();
            var result = toolkit.DistanceMatrix(s, others);
            EnumerateByDistance(
                s,
                result,
                (source, dest,dist)=> 
                {
                    if(source.Id!=dest.Id)
                    {
                        graph.Add(source, dest, dist);
                    }
            });            

            //EnumerateByDuration(
            //    result,
            //    (dest, dist) =>
            //    {
            //        if (dest.Id != s.Id)
            //        {
            //            g2.Add(s, dest, dist);
            //        }
            //    });

            
            foreach (var p in visit.Places)
            {
                if (p.Id == s.Id || p.GeoLocation==null) { continue; }
                if (tType == GoogleToolkitType.Api) { Thread.Sleep(TimeSpan.FromSeconds(15)); }
                result = toolkit.DistanceMatrix(p, others);

                EnumerateByDistance(
                    p,
                    result,
                    (source, dest, dist) =>
                    {
                        if(p.Id!=source.Id)
                        {
                            Console.WriteLine("Mismatch: SourceId '{0}', P.Id '{1}'", source.Id, p.Id);
                        }
                        if (dest.Id != source.Id)
                        {
                            graph.Add(source, dest, dist);
                        }
                    });

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
            int num = 3;
            //var res = graph.MinimumSpanningTree(num);

            var json = JsonConvert.SerializeObject(visit);
            FileIO.Write(outFile, json);

            var mst = graph.MinimumSpanningTree();
            SearchResult<Place> minResult = null;
            foreach (var v in mst.Vertices)
            {

                SearchResult<Place> local = graph.DFS(mst, v, num - 1);
                if (minResult == null || local.Cost < minResult.Cost)
                {
                    minResult = local;
                }
            }
            //Console.WriteLine(minResult.Cost);
            var res = minResult;

            if (res != null)
            {
                Logger.Debug("Minimum spanning tree route found with {0} destinations:",num);
                var count = 0;
                var total = res.Route.Count;
                foreach (var r in res.Route)
                {
                    //r.Source.Visited = true;
                    Logger.Debug("{0}) Id:{1} {2}", ++count, r.Source.Data.Id, r.Source.Data.Name);
                    if(count==total)
                    {
                        //r.Destination.Visited = true;
                        Logger.Debug("{0}) Id:{1} {2}", ++count, r.Destination.Data.Id, r.Destination.Data.Name);
                    }
                }
                Logger.Debug();
            }      
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

        private static void EnumerateByDistance(Place start, DistanceResult result, Action<Place, Place,double> nextPlace)
        {
            Logger.Debug();
            Logger.Border();
            Logger.Debug("Results sorted by distance: Origin {0}", result.DistanceSort[0].OriginId);
            var rank = 1;
            foreach (var dest in result.DistanceSort)
            {
                dest.DistanceRank = rank++;
                Logger.Border();
                Logger.Debug("DistanceId: {0}", dest.DistanceRank);
                Logger.Debug("Id: {0}", dest.Id);
                Logger.Debug("Name: {0}", dest.Name);
                Logger.Debug("Address: {0}", dest.Address);
                Logger.Debug("Description: {0}", dest.Description);
                Logger.Debug("Distance: {0} => {1}", dest.DistanceText, dest.DistanceValue);
                Logger.Debug("Duration: {0}", dest.DurationText);
                Logger.Border();
                Logger.Debug();
                if (nextPlace != null) { nextPlace(start, dest, dest.DistanceValue); }
                //Logger.Debug("Hit enter to continue...");
                //Console.ReadKey();
            }
        }
    }    
}
