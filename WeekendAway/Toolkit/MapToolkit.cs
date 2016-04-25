using Common;
using Google.Maps.DistanceMatrix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Toolkit
{
    public class MapToolkit
    {
        public static DistanceMatrixResponse DistanceMatrix(Place start, List<Place> others)
        {
            DistanceMatrixRequest req = new DistanceMatrixRequest();
            req.AddOrigin(new Google.Maps.Waypoint((decimal)start.GeoLocation.Latitude, (decimal)start.GeoLocation.Longitude));
            var index = 0;
            var skipList = new HashSet<int>();
            foreach (var o in others)
            {                
                if (o.GeoLocation == null) { skipList.Add(index); continue; }                
                req.AddDestination(new Google.Maps.Waypoint((decimal)o.GeoLocation.Latitude, (decimal)o.GeoLocation.Longitude));
                ++index;
            }
            req.Sensor = false;
            req.Units = Google.Maps.Units.imperial;
            DistanceMatrixService svc = new DistanceMatrixService();
            var response = svc.GetResponse(req);
            foreach (var row in response.Rows)
            {
                var pos = 0;
                foreach(var cell in row.Elements)
                {
                    while(skipList.Contains(pos))
                    {
                        ++pos;
                    }
                    var name = others[pos++].Name;
                    Logger.Border();
                    Logger.Debug("Id: {0}", pos);
                    Logger.Debug("Name: {0}", name);
                    Logger.Debug("Distance: {0}", cell.distance.Text);
                    Logger.Debug("Duration: {0}", cell.duration.Text);
                    Logger.Border();
                    Logger.Debug();
                    Logger.Debug("Hit enter to continue...");
                    Console.ReadKey();
                }
            }
            return response;
        }
    }
}
