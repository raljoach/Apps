using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeekendAway.Common
{
    public class GeoLocation
    {
        public GeoLocation() { }

        public GeoLocation(double longitude, double latitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
        }

        public GeoLocation(double longitude, double latitude, string addr) : this(longitude, latitude)
        {
            this.Address = addr;
        }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
    }
}
